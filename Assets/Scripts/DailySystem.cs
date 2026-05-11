using System.Collections.Generic;
using UnityEngine;

public class DailySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private ExternalVirusEvent externalVirusEvent;
    [SerializeField] private VirusLabUI virusLabUI;
    [SerializeField] private BlackMarketShopManager blackMarketShop;
    [SerializeField] private SecretaryActor secretaryActor;
    [SerializeField] private SecretaryInfo secretaryInfo;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private EndGameManager endGameManager;

    [Header("NPCs")]
    [SerializeField] private List<NPCActor> npcs = new();

    [Header("Diseases")]
    [SerializeField] private List<DiseaseSO> diseases = new();

    [Header("Chances")]
    [SerializeField, Range(0f, 1f)] private float baseSicknessChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float baseClinicVisitChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float deathChanceAfterThreeDays = 0.10f;

    [Header("Clinic Caps")]
    [SerializeField, Min(1)] private int maxPatientsPerDay = 5;
    [SerializeField, Min(0)] private int maxNormalPatientsPerDay = 3;

    [Header("Secretary")]
    [SerializeField, Min(1)] private int secretaryAbandonThreshold = 5;

    [Header("Today's Patients (Read Only)")]
    [SerializeField] private List<NPCActor> todaysPatients = new();

    public List<NPCActor> TodaysPatients => todaysPatients;
    public List<NPCActor> AllNPCs => npcs;

    public void ProcessNewDay()
    {
        Debug.Log("Processing new day...");

        if (npcs.Count == 0) { Debug.LogWarning("No NPCs assigned to DailySystem."); return; }
        if (diseases.Count == 0) { Debug.LogWarning("No diseases assigned to DailySystem."); return; }

        // Don't process if game already ended
        if (endGameManager != null && endGameManager.GameEnded) return;

        todaysPatients.Clear();

        if (virusLabUI != null)
            virusLabUI.OnNewDay();

        if (activeVirusManager != null)
            activeVirusManager.ProcessDailyVirusUpdate();

        if (externalVirusEvent != null)
            externalVirusEvent.TickAndMaybeTrigger();

        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (npc.isSick)
                npc.AdvanceDayWithDisease();
        }

        var deathsToRecord = new List<NPCActor>();
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
                    deathsToRecord.Add(npc);
                }
                else
                {
                    Debug.Log($"{npc.npcName} survived and recovered spontaneously from {npc.currentDisease?.diseaseName}.");
                    npc.CureDisease();
                }
            }
        }

        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.daysImmune > 0)
                npc.daysImmune--;
        }

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

        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            npc.willVisitClinic = false;
        }

        if (blackMarketShop != null)
            blackMarketShop.RefreshDailyOffers();

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

        Debug.Log($"[Clinic] Selected {normalsToTake} normal + {virusToTake} virus patients.");

        UpdateSecretary(deathsToRecord, sickNormal, sickVirus);

        LogDailyStatus();

        // Check end-of-day conditions for game ending
        if (endGameManager != null)
            endGameManager.CheckConditionsForDay();
    }

    private void UpdateSecretary(List<NPCActor> naturalDeathsThisTurn, List<NPCActor> sickNormal, List<NPCActor> sickVirus)
    {
        if (secretaryInfo == null) return;
        int currentDay = dayManager != null ? dayManager.GetCurrentDay() : 0;

        foreach (var npc in naturalDeathsThisTurn)
        {
            if (npc != null)
                secretaryInfo.RecordEvent(SecretaryEvent.EventType.Death, npc.npcName, currentDay);
        }

        var deathsAlreadyRecordedToday = new HashSet<string>();
        foreach (var n in naturalDeathsThisTurn)
            if (n != null) deathsAlreadyRecordedToday.Add(n.npcName);

        foreach (var npc in npcs)
        {
            if (npc == null) continue;
            if (npc.isAlive) continue;
            if (deathsAlreadyRecordedToday.Contains(npc.npcName)) continue;
            if (!HasDeathInBuffer(npc.npcName))
            {
                secretaryInfo.RecordEvent(SecretaryEvent.EventType.Death, npc.npcName, currentDay);
                deathsAlreadyRecordedToday.Add(npc.npcName);
            }
        }

        foreach (var npc in sickNormal)
        {
            if (npc != null && !npc.willVisitClinic)
                secretaryInfo.RecordEvent(SecretaryEvent.EventType.SickNotVisiting, npc.npcName, currentDay);
        }
        foreach (var npc in sickVirus)
        {
            if (npc != null && !npc.willVisitClinic)
                secretaryInfo.RecordEvent(SecretaryEvent.EventType.SickNotVisiting, npc.npcName, currentDay);
        }

        secretaryInfo.PruneOldEvents(currentDay);

        UpdateSecretaryAbandonState();
    }

    private bool HasDeathInBuffer(string npcName)
    {
        if (secretaryInfo == null) return true;

        int currentDay = dayManager != null ? dayManager.GetCurrentDay() : 0;
        var allRecent = secretaryInfo.GetRecentDeaths(currentDay);
        foreach (var e in allRecent)
            if (e.npcName == npcName) return true;
        return false;
    }

    private void UpdateSecretaryAbandonState()
    {
        if (secretaryActor == null) return;
        if (secretaryActor.HasLeft) return;

        int aliveCount = 0;
        foreach (var npc in npcs)
            if (npc != null && npc.isAlive) aliveCount++;

        if (secretaryActor.IsActive && aliveCount < secretaryAbandonThreshold)
        {
            secretaryActor.EnterFarewellDay();
        }
        else if (secretaryActor.IsOnFarewellDay)
        {
            secretaryActor.Leave();
        }
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