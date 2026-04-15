using System.Collections.Generic;
using UnityEngine;

public class DailySystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActiveVirusManager activeVirusManager;

    [Header("NPCs")]
    [SerializeField] private List<NPCActor> npcs = new();

    [Header("Diseases")]
    [SerializeField] private List<DiseaseSO> diseases = new();

    [Header("Chances")]
    [SerializeField, Range(0f, 1f)] private float baseSicknessChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float baseClinicVisitChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float deathChanceAfterThreeDays = 0.10f;

    [Header("Clinic")]
    [SerializeField, Min(1)] private int maxPatientsPerDay = 5;

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

        // 0) Processar o vírus do jogador primeiro (letalidade diária, propagação, extinção)
        if (activeVirusManager != null)
            activeVirusManager.ProcessDailyVirusUpdate();

        // 1) Quem já estava doente avança um dia (exceto infectados pelo vírus, já tratados acima)
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (npc.isSick)
                npc.AdvanceDayWithDisease();
        }

        // 2) Quem ultrapassou o 3.º dia rola morte / cura espontânea (apenas doenças normais)
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.infectedByPlayerVirus) continue;
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

        // 3) Decrementar imunidade dos vivos
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.daysImmune > 0)
                npc.daysImmune--;
        }

        // 4) Atribuir doenças novas a quem está saudável, fora de cooldown e não infectado pelo vírus
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.isSick) continue;
            if (npc.infectedByPlayerVirus) continue;
            if (npc.daysImmune > 0) continue;

            float sicknessModifier = GetSicknessTraitModifier(npc);
            float finalSicknessChance = Mathf.Clamp01(baseSicknessChance * sicknessModifier);

            if (Random.value < finalSicknessChance)
            {
                DiseaseSO randomDisease = diseases[Random.Range(0, diseases.Count)];
                npc.CatchDisease(randomDisease);
            }
        }

        // 5) Limpar flags do consultório dos vivos
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            npc.willVisitClinic = false;
        }

        // 6) Recolher NPCs vivos e doentes (inclui infectados pelo vírus)
        List<NPCActor> sickNPCs = new();
        foreach (var npc in npcs)
        {
            if (npc == null || !npc.isAlive) continue;
            if (npc.isSick) sickNPCs.Add(npc);
        }

        if (sickNPCs.Count == 0)
        {
            Debug.Log("Nenhum NPC vivo está doente. Consultório vazio.");
            LogDailyStatus();
            return;
        }

        // 7) Quem dos doentes quer ir ao médico
        List<NPCActor> willingSickNPCs = new();
        foreach (var npc in sickNPCs)
        {
            float finalVisitChance = GetFinalClinicVisitChance(npc);
            if (Random.value < finalVisitChance)
                willingSickNPCs.Add(npc);
        }

        if (willingSickNPCs.Count == 0)
        {
            Debug.Log("Há NPCs doentes, mas nenhum quis ir ao consultório hoje.");
            LogDailyStatus();
            return;
        }

        // 8) Máximo de maxPatientsPerDay pacientes por dia
        int targetCount = Mathf.Clamp(willingSickNPCs.Count, 1, maxPatientsPerDay);

        for (int i = 0; i < willingSickNPCs.Count; i++)
        {
            int randIndex = Random.Range(i, willingSickNPCs.Count);
            (willingSickNPCs[i], willingSickNPCs[randIndex]) = (willingSickNPCs[randIndex], willingSickNPCs[i]);
        }

        for (int i = 0; i < targetCount && i < willingSickNPCs.Count; i++)
        {
            willingSickNPCs[i].willVisitClinic = true;
            todaysPatients.Add(willingSickNPCs[i]);
        }

        LogDailyStatus();
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
            Debug.Log(
                $"{npc.npcName} | {aliveStr}{virusStr} | Sick: {npc.isSick} | Disease: {diseaseName} | " +
                $"DaysSick: {npc.daysSick} | DaysImmune: {npc.daysImmune} | " +
                $"Trust: {npc.trustInDoctor:0.00} | WillVisit: {npc.willVisitClinic}"
            );
        }
        Debug.Log($"Today's patients count: {todaysPatients.Count}");
    }
}