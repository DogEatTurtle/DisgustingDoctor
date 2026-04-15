using System.Collections.Generic;
using UnityEngine;

public class ActiveVirusManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private DiseaseSO playerVirusDiseaseSO;

    [Header("State")]
    [SerializeField] private ActiveVirus activeVirus;

    [Header("Propagation Weights")]
    [Tooltip("Weight multiplier for NPCs sharing the profession of an infected NPC.")]
    [SerializeField] private float sameProfessionWeight = 2.0f;

    [Tooltip("Weight multiplier applied to candidates based on their social trait's doctor visit chance multiplier (proxy for sociability). Higher = more social = more vulnerable.")]
    [SerializeField] private float sociabilityWeightFactor = 1.0f;

    public bool HasActiveVirus => activeVirus != null && activeVirus.IsActive;
    public ActiveVirus CurrentVirus => activeVirus;

    public bool ReleaseVirus(VirusBlueprint blueprint, NPCActor patientZero)
    {
        if (HasActiveVirus)
        {
            Debug.LogWarning("[Virus] Cannot release: a virus is already active.");
            return false;
        }
        if (blueprint == null || !blueprint.IsComplete)
        {
            Debug.LogWarning("[Virus] Cannot release: blueprint is incomplete.");
            return false;
        }
        if (patientZero == null || !patientZero.isAlive)
        {
            Debug.LogWarning("[Virus] Cannot release: invalid patient zero.");
            return false;
        }
        if (playerVirusDiseaseSO == null)
        {
            Debug.LogError("[Virus] No playerVirusDiseaseSO assigned in ActiveVirusManager.");
            return false;
        }

        // Reset immunity for everyone (new virus = immunity wiped)
        if (dailySystem != null)
        {
            foreach (var npc in dailySystem.AllNPCs)
            {
                if (npc != null) npc.ResetPlayerVirusImmunity();
            }
        }

        activeVirus = new ActiveVirus
        {
            virusDiseaseSO = playerVirusDiseaseSO,
            combinedSymptoms = blueprint.BuildCombinedLLMSymptoms(),
            lethalityPerDay = blueprint.TotalLethalityPerDay,
            dailyInfectionsCap = blueprint.TotalDailyInfectionsCap,
            totalInfectionsBudget = blueprint.TotalInfectionsCap,
            totalInfectionsUsed = 0
        };

        // Patient zero counts toward the total budget
        InfectNPC(patientZero);

        Debug.Log(
            $"[Virus] Released! Patient zero: {patientZero.npcName} | " +
            $"Lethality/day: {activeVirus.lethalityPerDay:0.00} | " +
            $"Daily cap: {activeVirus.dailyInfectionsCap} | " +
            $"Total budget: {activeVirus.totalInfectionsBudget}"
        );

        return true;
    }

    public void ProcessDailyVirusUpdate()
    {
        if (!HasActiveVirus) return;

        // 1. Roll lethality for each currently infected NPC
        var diedThisDay = new List<NPCActor>();
        var survivedAndCured = new List<NPCActor>();

        foreach (var npc in activeVirus.currentlyInfected)
        {
            if (npc == null || !npc.isAlive) continue;

            // Lethality roll first
            if (Random.value < activeVirus.lethalityPerDay)
            {
                Debug.Log($"[Virus] {npc.npcName} died from the virus on day {npc.daysSick}.");
                npc.Die();
                diedThisDay.Add(npc);
                continue;
            }

            // Advance day
            npc.daysSick++;

            // Spontaneous cure if survived 4 days
            if (npc.daysSick > 4)
            {
                Debug.Log($"[Virus] {npc.npcName} survived the virus and is now immune.");
                npc.CurePlayerVirusAndBecomeImmune();
                survivedAndCured.Add(npc);
            }
        }

        foreach (var npc in diedThisDay) activeVirus.UnregisterInfection(npc);
        foreach (var npc in survivedAndCured) activeVirus.UnregisterInfection(npc);

        // 2. Propagate to new candidates
        PropagateToNewVictims();

        // 3. Check extinction
        if (!activeVirus.IsActive)
        {
            Debug.Log("[Virus] The virus has gone extinct (no more infected NPCs).");
            activeVirus = null;
        }
    }

    private void PropagateToNewVictims()
    {
        if (activeVirus == null) return;
        if (activeVirus.RemainingTotalBudget <= 0) return;
        if (activeVirus.dailyInfectionsCap <= 0) return;
        if (activeVirus.currentlyInfected.Count == 0) return;
        if (dailySystem == null) return;

        // Build candidate list: alive, not infected by virus, not immune to virus, not currently sick with virus
        var candidates = new List<NPCActor>();
        var weights = new List<float>();

        // Collect professions of currently infected for proximity bonus
        var infectedProfessions = new HashSet<ProfessionSO>();
        foreach (var infected in activeVirus.currentlyInfected)
        {
            if (infected != null && infected.profession != null)
                infectedProfessions.Add(infected.profession);
        }

        foreach (var npc in dailySystem.AllNPCs)
        {
            if (npc == null) continue;
            if (!npc.isAlive) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (npc.immuneToCurrentPlayerVirus) continue;

            float weight = 1f;

            // Same profession as someone already infected
            if (npc.profession != null && infectedProfessions.Contains(npc.profession))
                weight *= sameProfessionWeight;

            // Sociability proxy: use the NPC's doctor visit multipliers as a stand-in for being social
            float socialMult = 1f;
            if (npc.socialTrait != null) socialMult *= npc.socialTrait.doctorVisitChanceMultiplier;
            if (npc.skillTrait != null) socialMult *= npc.skillTrait.doctorVisitChanceMultiplier;
            weight *= Mathf.Lerp(1f, socialMult, sociabilityWeightFactor);

            candidates.Add(npc);
            weights.Add(weight);
        }

        int infectionsThisDay = Mathf.Min(activeVirus.dailyInfectionsCap, activeVirus.RemainingTotalBudget);
        infectionsThisDay = Mathf.Min(infectionsThisDay, candidates.Count);

        for (int i = 0; i < infectionsThisDay; i++)
        {
            int chosen = WeightedSample(weights);
            if (chosen < 0) break;

            var victim = candidates[chosen];
            InfectNPC(victim);

            candidates.RemoveAt(chosen);
            weights.RemoveAt(chosen);
        }
    }

    private void InfectNPC(NPCActor npc)
    {
        if (npc == null || !npc.isAlive) return;
        if (activeVirus == null) return;

        npc.CatchPlayerVirus(activeVirus.virusDiseaseSO, activeVirus.combinedSymptoms);
        activeVirus.RegisterInfection(npc);

        Debug.Log($"[Virus] Infected {npc.npcName}. Total used: {activeVirus.totalInfectionsUsed}/{activeVirus.totalInfectionsBudget}");
    }

    private int WeightedSample(List<float> weights)
    {
        float total = 0f;
        foreach (var w in weights) total += w;
        if (total <= 0f) return -1;

        float roll = Random.value * total;
        float cumulative = 0f;
        for (int i = 0; i < weights.Count; i++)
        {
            cumulative += weights[i];
            if (roll <= cumulative) return i;
        }
        return weights.Count - 1;
    }

    public void NotifyPlayerCuredInfectedNPC(NPCActor npc)
    {
        if (!HasActiveVirus) return;
        if (npc == null) return;

        activeVirus.UnregisterInfection(npc);

        if (!activeVirus.IsActive)
        {
            Debug.Log("[Virus] The virus has gone extinct (last infected was cured by the player).");
            activeVirus = null;
        }
    }
}