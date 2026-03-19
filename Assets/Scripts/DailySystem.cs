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

        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            float roll = Random.value;

            if (roll < baseSicknessChance)
            {
                DiseaseSO randomDisease = diseases[Random.Range(0, diseases.Count)];
                npc.SetDisease(randomDisease);
            }
            else
            {
                npc.SetDisease(null);
            }
        }

        List<NPCActor> sickNPCs = new();

        foreach (var npc in npcs)
        {
            if (npc != null && npc.isSick)
                sickNPCs.Add(npc);
        }

        // limpar flags anteriores
        foreach (var npc in npcs)
        {
            if (npc != null)
                npc.willVisitClinic = false;
        }

        // se ninguém estiver doente, não vai ninguém para o consultório
        if (sickNPCs.Count == 0)
        {
            Debug.Log("Nenhum NPC ficou doente hoje. Não há pacientes no consultório.");
            LogDailyStatus();
            return;
        }

        int targetCount = Mathf.Clamp(sickNPCs.Count, 1, 3);

        // baralhar lista
        for (int i = 0; i < sickNPCs.Count; i++)
        {
            int randIndex = Random.Range(i, sickNPCs.Count);
            var temp = sickNPCs[i];
            sickNPCs[i] = sickNPCs[randIndex];
            sickNPCs[randIndex] = temp;
        }

        // escolher os primeiros
        for (int i = 0; i < targetCount && i < sickNPCs.Count; i++)
        {
            sickNPCs[i].willVisitClinic = true;
            todaysPatients.Add(sickNPCs[i]);
        }

        LogDailyStatus();
    }

    private void LogDailyStatus()
    {
        Debug.Log("=== Daily NPC Status ===");

        foreach (var npc in npcs)
        {
            if (npc == null) continue;

            string diseaseName = npc.currentDisease ? npc.currentDisease.name : "None";

            Debug.Log(
                $"{npc.npcName} | Sick: {npc.isSick} | Disease: {diseaseName} | Will Visit Clinic: {npc.willVisitClinic}"
            );
        }

        Debug.Log($"Today's patients count: {todaysPatients.Count}");
    }
}