using System.Collections.Generic;
using UnityEngine;

public class ClinicPatientPlacer : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private DailySystem dailySystem;

    [Header("Clinic Slots")]
    [SerializeField] private List<Transform> patientSlots = new();

    public void PlaceTodaysPatients()
    {
        Debug.Log("ClinicPatientPlacer -> PlaceTodaysPatients chamado");

        if (dailySystem == null)
        {
            Debug.LogWarning("ClinicPatientPlacer: DailySystem is missing.");
            return;
        }

        // 1. devolver todos os NPCs à posição original
        foreach (var npc in dailySystem.AllNPCs)
        {
            if (npc == null) continue;
            npc.ReturnToOriginalPosition();
        }

        // 2. buscar os pacientes do dia
        List<NPCActor> todaysPatients = dailySystem.TodaysPatients;

        if (todaysPatients == null || todaysPatients.Count == 0)
        {
            Debug.Log("ClinicPatientPlacer: No patients to place today.");
            return;
        }

        Debug.Log($"ClinicPatientPlacer: {todaysPatients.Count} pacientes para colocar.");

        int count = Mathf.Min(todaysPatients.Count, patientSlots.Count);

        for (int i = 0; i < count; i++)
        {
            NPCActor npc = todaysPatients[i];
            Transform slot = patientSlots[i];

            if (npc == null || slot == null) continue;

            npc.transform.position = slot.position;
            npc.transform.rotation = slot.rotation;

            Debug.Log($"{npc.npcName} colocado no slot {i + 1}");
        }
    }
}