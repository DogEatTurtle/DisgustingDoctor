using System.Collections.Generic;
using UnityEngine;

public class DailySystem : MonoBehaviour
{
    [Header("NPCs")]
    [SerializeField] private List<NPCActor> npcs = new();

    [Header("Diseases")]
    [SerializeField] private List<DiseaseSO> diseases = new();

    [Header("Chances")]
    [SerializeField, Range(0f, 1f)] private float baseSicknessChance = 0.4f;
    [SerializeField, Range(0f, 1f)] private float baseClinicVisitChance = 0.7f;

    [Header("Today's Patients (Read Only)")]
    [SerializeField] private List<NPCActor> todaysPatients = new();

    public List<NPCActor> TodaysPatients => todaysPatients;
    public List<NPCActor> AllNPCs => npcs;

    public void ProcessNewDay()
    {
        Debug.Log("Processing new day...");

        if (npcs.Count == 0)
        {
            Debug.LogWarning("No NPCs assigned to DailySystem.");
            return;
        }

        if (diseases.Count == 0)
        {
            Debug.LogWarning("No diseases assigned to DailySystem.");
            return;
        }

        todaysPatients.Clear();

        // 1) Gerar quem fica doente hoje
        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            float sicknessModifier = GetSicknessTraitModifier(npc);
            float finalSicknessChance = Mathf.Clamp01(baseSicknessChance * sicknessModifier);

            float roll = Random.value;

            if (roll < finalSicknessChance)
            {
                DiseaseSO randomDisease = diseases[Random.Range(0, diseases.Count)];
                npc.SetDisease(randomDisease);
            }
            else
            {
                npc.SetDisease(null);
            }
        }

        // 2) Recolher só os NPCs doentes
        List<NPCActor> sickNPCs = new();

        foreach (var npc in npcs)
        {
            if (npc != null && npc.isSick)
                sickNPCs.Add(npc);
        }

        // 3) Limpar flags do consultório
        foreach (var npc in npcs)
        {
            if (npc != null)
                npc.willVisitClinic = false;
        }

        // 4) Se ninguém ficou doente, não vai ninguém
        if (sickNPCs.Count == 0)
        {
            Debug.Log("Nenhum NPC ficou doente hoje. Não há pacientes no consultório.");
            LogDailyStatus();
            return;
        }

        // 5) Dos doentes, calcular quem quer ir ao médico
        List<NPCActor> willingSickNPCs = new();

        foreach (var npc in sickNPCs)
        {
            if (npc == null) continue;

            float traitModifier = GetClinicVisitTraitModifier(npc);

            // Confiança:
            // 0.0 -> 0.5x
            // 0.5 -> 1.0x
            // 1.0 -> 1.5x
            float trustModifier = Mathf.Lerp(0.5f, 1.5f, npc.trustInDoctor);

            float finalVisitChance = Mathf.Clamp01(baseClinicVisitChance * traitModifier * trustModifier);

            if (Random.value < finalVisitChance)
            {
                willingSickNPCs.Add(npc);
            }

            Debug.Log(
                $"{npc.npcName} | Trust: {npc.trustInDoctor:0.00} | " +
                $"VisitChance: {finalVisitChance:0.00}"
            );
        }

        // 6) Se há doentes mas nenhum quis ir, o consultório fica vazio
        if (willingSickNPCs.Count == 0)
        {
            Debug.Log("Há NPCs doentes, mas nenhum quis ir ao consultório hoje.");
            LogDailyStatus();
            return;
        }

        // 7) Máximo de 3 pacientes naturais por dia
        int targetCount = Mathf.Clamp(willingSickNPCs.Count, 1, 3);

        // 8) Baralhar a lista
        for (int i = 0; i < willingSickNPCs.Count; i++)
        {
            int randIndex = Random.Range(i, willingSickNPCs.Count);
            NPCActor temp = willingSickNPCs[i];
            willingSickNPCs[i] = willingSickNPCs[randIndex];
            willingSickNPCs[randIndex] = temp;
        }

        // 9) Escolher os primeiros
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

        if (npc.socialTrait != null)
            modifier *= npc.socialTrait.infectionRiskMultiplier;

        if (npc.skillTrait != null)
            modifier *= npc.skillTrait.infectionRiskMultiplier;

        return modifier;
    }

    private float GetClinicVisitTraitModifier(NPCActor npc)
    {
        float modifier = 1f;

        if (npc.socialTrait != null)
            modifier *= npc.socialTrait.doctorVisitChanceMultiplier;

        if (npc.skillTrait != null)
            modifier *= npc.skillTrait.doctorVisitChanceMultiplier;

        return modifier;
    }

    private void LogDailyStatus()
    {
        Debug.Log("=== Daily NPC Status ===");

        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            string diseaseName = npc.currentDisease ? npc.currentDisease.name : "None";

            Debug.Log(
                $"{npc.npcName} | Sick: {npc.isSick} | Disease: {diseaseName} | " +
                $"Trust: {npc.trustInDoctor:0.00} | Will Visit Clinic: {npc.willVisitClinic}"
            );
        }

        Debug.Log($"Today's patients count: {todaysPatients.Count}");
    }
}