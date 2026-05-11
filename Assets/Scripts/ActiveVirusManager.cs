using System.Collections.Generic;
using UnityEngine;

public class ActiveVirusManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private DiseaseSO playerVirusDiseaseSO;
    [SerializeField] private DiseaseSO externalVirusDiseaseSO;
    [SerializeField] private GameStats gameStats;
    [SerializeField] private EventFeedbackUI eventFeedbackUI;

    [Header("State")]
    [SerializeField] private ActiveVirus activeVirus;

    [Header("Propagation Weights")]
    [SerializeField] private float sameProfessionWeight = 2.0f;
    [SerializeField] private float sociabilityWeightFactor = 1.0f;

    public bool HasActiveVirus => activeVirus != null && activeVirus.IsActive;
    public ActiveVirus CurrentVirus => activeVirus;
    public bool HasExternalVirusActive => HasActiveVirus && activeVirus.isExternal;
    public bool HasPlayerVirusActive => HasActiveVirus && !activeVirus.isExternal;

    public bool IsPendingPatientZero(NPCActor npc)
    {
        return activeVirus != null && activeVirus.pendingPatientZero == npc && npc != null;
    }

    public IReadOnlyList<VirusUpgradeSO> GetExternalVirusUpgrades()
    {
        if (!HasExternalVirusActive) return null;
        return activeVirus.sourceUpgrades;
    }

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

        if (dailySystem != null)
        {
            foreach (var npc in dailySystem.AllNPCs)
            {
                if (npc != null) npc.ResetPlayerVirusImmunity();
            }
        }

        var sourceUpgrades = new List<VirusUpgradeSO>();
        var symptomNames = new List<string>();
        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            if (blueprint.Slots[i] != null)
            {
                sourceUpgrades.Add(blueprint.Slots[i]);
                symptomNames.Add(blueprint.Slots[i].shortName);
            }
        }

        activeVirus = new ActiveVirus
        {
            virusDiseaseSO = playerVirusDiseaseSO,
            combinedSymptoms = blueprint.BuildCombinedLLMSymptoms(),
            lethalityPerDay = blueprint.TotalLethalityPerDay,
            dailyInfectionsCap = blueprint.TotalDailyInfectionsCap,
            totalInfectionsBudget = blueprint.TotalInfectionsCap,
            totalInfectionsUsed = 0,
            pendingPatientZero = patientZero,
            isExternal = false,
            sourceUpgrades = sourceUpgrades
        };

        Debug.Log(
            $"[Virus] Released! Patient zero queued: {patientZero.npcName} | " +
            $"Lethality/day: {activeVirus.lethalityPerDay:0.00} | " +
            $"Daily cap: {activeVirus.dailyInfectionsCap} | " +
            $"Total budget: {activeVirus.totalInfectionsBudget} | " +
            $"Will manifest on the next day."
        );

        if (gameStats != null)
        {
            gameStats.RecordPlayerVirusReleased(
                patientZero.npcName,
                activeVirus.lethalityPerDay,
                activeVirus.dailyInfectionsCap,
                activeVirus.totalInfectionsBudget,
                symptomNames
            );
        }

        return true;
    }

    public bool ReleaseExternalVirus(List<VirusUpgradeSO> upgrades, NPCActor patientZero)
    {
        if (HasActiveVirus)
        {
            Debug.LogWarning("[Virus] Cannot release external: a virus is already active.");
            return false;
        }
        if (upgrades == null || upgrades.Count != VirusBlueprint.SlotCount)
        {
            Debug.LogWarning("[Virus] Cannot release external: invalid upgrade list.");
            return false;
        }
        if (patientZero == null || !patientZero.isAlive)
        {
            Debug.LogWarning("[Virus] Cannot release external: invalid patient zero.");
            return false;
        }
        if (externalVirusDiseaseSO == null)
        {
            Debug.LogError("[Virus] No externalVirusDiseaseSO assigned in ActiveVirusManager.");
            return false;
        }

        if (dailySystem != null)
        {
            foreach (var npc in dailySystem.AllNPCs)
            {
                if (npc != null) npc.ResetPlayerVirusImmunity();
            }
        }

        float lethality = 0f;
        int dailyCap = 0;
        int totalCap = 0;
        var combinedSymptoms = new List<string>();
        var symptomNames = new List<string>();
        foreach (var u in upgrades)
        {
            if (u == null) continue;
            lethality += u.lethalityPerDay;
            dailyCap += u.dailyInfectionsCap;
            totalCap += u.totalInfectionsCap;
            if (!string.IsNullOrWhiteSpace(u.llmSymptomSentence))
                combinedSymptoms.Add(u.llmSymptomSentence);
            symptomNames.Add(u.shortName);
        }
        lethality = Mathf.Clamp(lethality, 0f, 1f);
        dailyCap = Mathf.Max(0, dailyCap);
        totalCap = Mathf.Max(0, totalCap);

        activeVirus = new ActiveVirus
        {
            virusDiseaseSO = externalVirusDiseaseSO,
            combinedSymptoms = combinedSymptoms,
            lethalityPerDay = lethality,
            dailyInfectionsCap = dailyCap,
            totalInfectionsBudget = totalCap,
            totalInfectionsUsed = 0,
            pendingPatientZero = null,
            isExternal = true,
            sourceUpgrades = new List<VirusUpgradeSO>(upgrades)
        };

        if (patientZero.isSick && !patientZero.infectedByPlayerVirus)
        {
            Debug.Log($"[ExternalVirus] {patientZero.npcName} was sick with {patientZero.currentDisease?.diseaseName}. External virus overrides it.");
            patientZero.CureDisease();
        }

        InfectNPC(patientZero);

        Debug.Log(
            $"[ExternalVirus] Released! Patient zero: {patientZero.npcName} | " +
            $"Lethality/day: {lethality:0.00} | Daily cap: {dailyCap} | Total budget: {totalCap} | " +
            $"Symptoms: {string.Join(", ", symptomNames)}"
        );

        if (gameStats != null)
            gameStats.RecordExternalVirusReleased(patientZero.npcName, lethality, dailyCap, totalCap, symptomNames);

        return true;
    }

    public void ProcessDailyVirusUpdate()
    {
        if (activeVirus == null) return;

        bool manifestedThisTurn = false;
        if (activeVirus.pendingPatientZero != null)
        {
            var pz = activeVirus.pendingPatientZero;
            activeVirus.pendingPatientZero = null;

            if (pz != null && pz.isAlive)
            {
                if (pz.isSick && !pz.infectedByPlayerVirus)
                {
                    Debug.Log($"[Virus] {pz.npcName} was sick with {pz.currentDisease?.diseaseName}. Virus takes over.");
                    pz.CureDisease();
                }

                InfectNPC(pz);
                manifestedThisTurn = true;
                Debug.Log($"[Virus] {pz.npcName} now shows symptoms of the virus.");
            }
            else
            {
                Debug.Log("[Virus] Pending patient zero is no longer valid (dead or null). Virus did not manifest.");
            }
        }

        if (!HasActiveVirus)
        {
            bool wasExternal = activeVirus != null && activeVirus.isExternal;
            Debug.Log("[Virus] The virus has gone extinct (no infected NPCs after manifestation).");
            if (gameStats != null) gameStats.RecordVirusExtinct(wasExternal);
            ShowExtinctFeedback(wasExternal);
            activeVirus = null;
            return;
        }

        var diedThisDay = new List<NPCActor>();
        var survivedAndCured = new List<NPCActor>();
        var freshlyManifestedThisTurn = new HashSet<NPCActor>();
        if (manifestedThisTurn)
        {
            foreach (var infected in activeVirus.currentlyInfected)
            {
                if (infected != null && infected.daysSick == 1)
                    freshlyManifestedThisTurn.Add(infected);
            }
        }

        bool isExternalNow = activeVirus.isExternal;

        foreach (var npc in activeVirus.currentlyInfected)
        {
            if (npc == null || !npc.isAlive) continue;
            if (freshlyManifestedThisTurn.Contains(npc)) continue;

            if (Random.value < activeVirus.lethalityPerDay)
            {
                Debug.Log($"[Virus] {npc.npcName} died from the virus on day {npc.daysSick}.");
                npc.Die();
                diedThisDay.Add(npc);

                if (gameStats != null)
                {
                    string cause = isExternalNow ? "ExternalVirus" : "PlayerVirus";
                    int aliveAfter = CountAliveSafe();
                    gameStats.RecordDeath(npc.npcName, cause, aliveAfter);
                }
                continue;
            }

            npc.daysSick++;
            npc.AdvanceVirusDay(activeVirus.isExternal);

            if (npc.daysSick > 4)
            {
                Debug.Log($"[Virus] {npc.npcName} survived the virus and is now immune.");
                npc.CurePlayerVirusAndBecomeImmune();
                survivedAndCured.Add(npc);
            }
        }

        foreach (var npc in diedThisDay) activeVirus.UnregisterInfection(npc);
        foreach (var npc in survivedAndCured) activeVirus.UnregisterInfection(npc);

        if (!manifestedThisTurn)
            PropagateToNewVictims();

        if (!HasActiveVirus)
        {
            Debug.Log("[Virus] The virus has gone extinct (no more infected NPCs).");
            if (gameStats != null) gameStats.RecordVirusExtinct(isExternalNow);
            ShowExtinctFeedback(isExternalNow);
            activeVirus = null;
        }
    }

    // Helper for feedback when a virus goes extinct (not by player cure)
    private void ShowExtinctFeedback(bool wasExternal)
    {
        if (eventFeedbackUI == null) return;
        eventFeedbackUI.Show(wasExternal
            ? EventFeedbackUI.FeedbackType.ExternalVirusExtinctNaturally
            : EventFeedbackUI.FeedbackType.PlayerVirusExtinct);
    }

    private int CountAliveSafe()
    {
        if (dailySystem == null) return 0;
        int alive = 0;
        foreach (var npc in dailySystem.AllNPCs)
            if (npc != null && npc.isAlive) alive++;
        return alive;
    }

    private void PropagateToNewVictims()
    {
        if (activeVirus == null) return;
        if (activeVirus.RemainingTotalBudget <= 0) return;
        if (activeVirus.dailyInfectionsCap <= 0) return;
        if (activeVirus.currentlyInfected.Count == 0) return;
        if (dailySystem == null) return;

        var candidates = new List<NPCActor>();
        var weights = new List<float>();

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
            if (npc.isSick) continue;

            float weight = 1f;

            if (npc.profession != null && infectedProfessions.Contains(npc.profession))
                weight *= sameProfessionWeight;

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

        var initialIndices = PickInitialRevealedIndices(npc);

        npc.CatchPlayerVirus(activeVirus.virusDiseaseSO, activeVirus.combinedSymptoms, initialIndices);
        activeVirus.RegisterInfection(npc);

        Debug.Log(
            $"[Virus] Infected {npc.npcName} with revealed indices [{string.Join(",", initialIndices)}]. " +
            $"Total used: {activeVirus.totalInfectionsUsed}/{activeVirus.totalInfectionsBudget}"
        );

        if (gameStats != null && !activeVirus.isExternal)
            gameStats.RecordPlayerVirusInfection(npc.npcName);
    }

    private List<int> PickInitialRevealedIndices(NPCActor npc)
    {
        int totalSymptoms = activeVirus.combinedSymptoms != null ? activeVirus.combinedSymptoms.Count : 0;
        int count = Mathf.Min(2, totalSymptoms);

        var indices = new List<int>();
        if (totalSymptoms == 0) return indices;

        bool isExternal = activeVirus.isExternal;
        bool isSecondInfected = isExternal && activeVirus.currentlyInfected.Count == 1;

        if (isSecondInfected)
        {
            var firstInfected = activeVirus.currentlyInfected[0];
            var firstIndices = firstInfected != null
                ? new HashSet<int>(firstInfected.GetRevealedSymptomIndices())
                : new HashSet<int>();

            var preferred = new List<int>();
            var fallback = new List<int>();
            for (int i = 0; i < totalSymptoms; i++)
            {
                if (firstIndices.Contains(i)) fallback.Add(i);
                else preferred.Add(i);
            }

            ShuffleInPlace(preferred);
            ShuffleInPlace(fallback);
            var combined = new List<int>(preferred);
            combined.AddRange(fallback);

            for (int i = 0; i < count && i < combined.Count; i++)
                indices.Add(combined[i]);
        }
        else
        {
            var pool = new List<int>();
            for (int i = 0; i < totalSymptoms; i++) pool.Add(i);
            ShuffleInPlace(pool);
            for (int i = 0; i < count && i < pool.Count; i++)
                indices.Add(pool[i]);
        }

        return indices;
    }

    private void ShuffleInPlace(List<int> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int j = Random.Range(i, list.Count);
            (list[i], list[j]) = (list[j], list[i]);
        }
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

        bool wasExternal = activeVirus.isExternal;
        activeVirus.UnregisterInfection(npc);

        if (!HasActiveVirus)
        {
            Debug.Log("[Virus] The virus has gone extinct (last infected was cured by the player).");
            if (gameStats != null) gameStats.RecordVirusExtinct(wasExternal);
            ShowExtinctFeedback(wasExternal);
            activeVirus = null;
        }
    }

    public List<NPCActor> CureAllExternalVirusInfected()
    {
        var cured = new List<NPCActor>();
        if (!HasExternalVirusActive) return cured;

        foreach (var npc in activeVirus.currentlyInfected.ToArray())
        {
            if (npc == null) continue;
            npc.CurePlayerVirusAndBecomeImmune();
            cured.Add(npc);
        }
        activeVirus.currentlyInfected.Clear();

        Debug.Log($"[ExternalVirus] Cure successful. {cured.Count} NPCs cured. Virus extinct.");

        if (gameStats != null)
            gameStats.RecordExternalVirusCured(cured.Count);

        // Different feedback from natural extinction: player cured the unknown virus
        if (eventFeedbackUI != null)
            eventFeedbackUI.Show(EventFeedbackUI.FeedbackType.ExternalVirusCured);

        activeVirus = null;
        return cured;
    }
}