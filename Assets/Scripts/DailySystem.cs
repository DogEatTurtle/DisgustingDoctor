using System.Collections.Generic;
using UnityEngine;

public class DailySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private ExternalVirusEvent externalVirusEvent;
    [SerializeField] private VirusLabUI virusLabUI;
    [SerializeField] private BlackMarketShopManager blackMarketShop;

    [Header("NPCs")]
    [SerializeField] private List<NPCActor> npcs = new();

    [Header("Diseases")]
    [SerializeField] private List<DiseaseSO> diseases = new();

    [Header("Chances")]
    [SerializeField, Range(0f, 1f)] private float baseSicknessChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float baseClinicVisitChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float deathChanceAfterThreeDays = 0.10f;

    [Header("Clinic Caps")]
    [Tooltip("Maximum total patients in the clinic per day (chair limit).")]
    [SerializeField, Min(1)] private int maxPatientsPerDay = 5;

    [Tooltip("Maximum patients with normal diseases per day. Virus-infected patients can fill the remaining slots up to maxPatientsPerDay.")]
    [SerializeField, Min(0)] private int maxNormalPatientsPerDay = 3;

    [Header("Today's Patients (Read Only)")]
    [SerializeField] private List<NPCActor> todaysPatients = new();

    public List<NPCActor> TodaysPatients => todaysPatients;
    public List<NPCActor> AllNPCs => npcs;

    public void ProcessNewDay()
    {
        Debug.Log("Processing new day...");

        if (npcs.Count == 0) { Debug.LogWarning("No NPCs assigned to DailySystem."); return; }
        if (diseases.Count == 0) { Debug.LogWarning("No diseases assigned to DailySystem."); return; }

        todaysPatients.Clear();

        // Reset cure cooldown and external virus discovery flag for the new day
        if (virusLabUI != null)
            virusLabUI.OnNewDay();

        // 0a) Process the active virus (lethality, propagation, extinction)
        if (activeVirusManager != null)
            activeVirusManager.ProcessDailyVirusUpdate();

        // 0b) Maybe trigger an external virus event (only if no virus is active)
        if (externalVirusEvent != null)
            externalVirusEvent.TickAndMaybeTrigger();

        // 1) NPCs already sick advance one day (excluding virus-infected, handled above)
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (npc.isSick)
                npc.AdvanceDayWithDisease();
        }

        // 2) Death/spontaneous cure roll for normal diseases (after day 3).
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (activeVirusManager != null && activeVirusManager.IsPendingPatientZero(npc)) continue;
            if (npc.isSick && npc.daysSick > 3)
            {
                if (Random.value < deathChanceAfterThreeDays)
                {
                    Debug.Log($"{npc.npcName} died from {npc.currentDisease?.diseaseName}.");
                    npc.Die();
                }
                else
                {
                    Debug.Log($"{npc.npcName} survived and recovered spontaneously from {npc.currentDisease?.diseaseName}.");
                    npc.CureDisease();
                }
            }
        }

        // 3) Decrement immunity for the living
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.daysImmune > 0)
                npc.daysImmune--;
        }

        // 4) Assign new diseases to healthy, non-immune, non-infected NPCs
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.isSick) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (npc.daysImmune > 0) continue;
            if (activeVirusManager != null && activeVirusManager.IsPendingPatientZero(npc)) continue;

            float sicknessModifier = GetSicknessTraitModifier(npc);
            float finalSicknessChance = Mathf.Clamp01(baseSicknessChance * sicknessModifier);

            if (Random.value < finalSicknessChance)
            {
                DiseaseSO randomDisease = diseases[Random.Range(0, diseases.Count)];
                npc.CatchDisease(randomDisease);
            }
        }

        // 5) Clear clinic flags for the living
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            npc.willVisitClinic = false;
        }

        // Refresh black market offers
        if (blackMarketShop != null)
            blackMarketShop.RefreshDailyOffers();

        // 6) Collect alive sick NPCs into two pools: normal and virus
        List<NPCActor> sickNormal = new();
        List<NPCActor> sickVirus = new();
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (!npc.isSick) continue;

            if (npc.infectedByPlayerVirus)
                sickVirus.Add(npc);
            else
                sickNormal.Add(npc);
        }

        if (sickNormal.Count == 0 && sickVirus.Count == 0)
        {
            Debug.Log("Nenhum NPC vivo está doente. Consultório vazio.");
            LogDailyStatus();
            return;
        }

        // 7) Roll which sick NPCs want to visit the clinic (each pool independently)
        List<NPCActor> willingNormal = new();
        foreach (var npc in sickNormal)
        {
            float chance = GetFinalClinicVisitChance(npc);
            if (Random.value < chance)
                willingNormal.Add(npc);
        }

        List<NPCActor> willingVirus = new();
        foreach (var npc in sickVirus)
        {
            float chance = GetFinalClinicVisitChance(npc);
            if (Random.value < chance)
                willingVirus.Add(npc);
        }

        if (willingNormal.Count == 0 && willingVirus.Count == 0)
        {
            Debug.Log("Há NPCs doentes, mas nenhum quis ir ao consultório hoje.");
            LogDailyStatus();
            return;
        }

        // 8) Apply caps
        // Normal patients: capped at maxNormalPatientsPerDay
        // Virus patients: can fill the remaining slots up to maxPatientsPerDay
        ShuffleInPlace(willingNormal);
        ShuffleInPlace(willingVirus);

        int normalsToTake = Mathf.Min(willingNormal.Count, maxNormalPatientsPerDay);
        int remainingSlots = Mathf.Max(0, maxPatientsPerDay - normalsToTake);
        int virusToTake = Mathf.Min(willingVirus.Count, remainingSlots);

        for (int i = 0; i < normalsToTake; i++)
        {
            willingNormal[i].willVisitClinic = true;
            todaysPatients.Add(willingNormal[i]);
        }

        for (int i = 0; i < virusToTake; i++)
        {
            willingVirus[i].willVisitClinic = true;
            todaysPatients.Add(willingVirus[i]);
        }

        Debug.Log($"[Clinic] Selected {normalsToTake} normal + {virusToTake} virus patients (cap: {maxNormalPatientsPerDay} normal / {maxPatientsPerDay} total).");

        LogDailyStatus();
    }

    private void ShuffleInPlace(List<NPCActor> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            int randIndex = Random.Range(i, list.Count);
            (list[i], list[randIndex]) = (list[randIndex], list[i]);
        }
    }

    private float GetSicknessTraitModifier(NPCActor npc)
    {
        float modifier = 1f;
        if (npc.socialTrait != null) modifier *= npc.socialTrait.infectionRiskMultiplier;
        if (npc.skillTrait != null) modifier *= npc.skillTrait.infectionRiskMultiplier;
        return modifier;
    }

    private float GetClinicVisitTraitModifier(NPCActor npc)
    {
        float modifier = 1f;
        if (npc.socialTrait != null) modifier *= npc.socialTrait.doctorVisitChanceMultiplier;
        if (npc.skillTrait != null) modifier *= npc.skillTrait.doctorVisitChanceMultiplier;
        return modifier;
    }

    private float GetFinalClinicVisitChance(NPCActor npc)
    {
        float traitModifier = GetClinicVisitTraitModifier(npc);
        float trustModifier = Mathf.Lerp(0.5f, 1.5f, npc.trustInDoctor);
        return Mathf.Clamp01(baseClinicVisitChance * traitModifier * trustModifier);
    }

    public float GetFinalClinicVisitChanceForNPC(NPCActor npc)
    {
        if (npc == null) return 0f;
        if (!npc.isAlive) return 0f;
        return GetFinalClinicVisitChance(npc);
    }

    public string GetFormattedClinicVisitChance(NPCActor npc)
    {
        return $"{GetFinalClinicVisitChanceForNPC(npc) * 100f:0}%";
    }

    private void LogDailyStatus()
    {
        Debug.Log("=== Daily NPC Status ===");
        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            string diseaseName = npc.currentDisease ? npc.currentDisease.diseaseName : "None";
            string aliveStr = npc.isAlive ? "Alive" : "DEAD";
            string virusStr = npc.infectedByPlayerVirus ? " [VIRUS]" : "";
            string pendingStr = (activeVirusManager != null && activeVirusManager.IsPendingPatientZero(npc)) ? " [PENDING_VIRUS]" : "";
            Debug.Log(
                $"{npc.npcName} | {aliveStr}{virusStr}{pendingStr} | Sick: {npc.isSick} | Disease: {diseaseName} | " +
                $"DaysSick: {npc.daysSick} | DaysImmune: {npc.daysImmune} | " +
                $"Trust: {npc.trustInDoctor:0.00} | WillVisit: {npc.willVisitClinic}"
            );
        }
        Debug.Log($"Today's patients count: {todaysPatients.Count}");
    }
}