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

        // devolver todos os NPCs à posição original
        foreach (var npc in dailySystem.AllNPCs)
        {
            if (npc == null) continue;
            npc.ReturnToOriginalPosition();
        }

        List<NPCActor> todaysPatients = dailySystem.TodaysPatients;

        if (todaysPatients == null || todaysPatients.Count == 0)
        {
            Debug.Log("ClinicPatientPlacer: No patients to place today.");
            return;
        }

        // criar cópia dos slots e baralhar
        List<Transform> shuffledSlots = new List<Transform>(patientSlots);

        for (int i = 0; i < shuffledSlots.Count; i++)
        {
            int randIndex = Random.Range(i, shuffledSlots.Count);
            Transform temp = shuffledSlots[i];
            shuffledSlots[i] = shuffledSlots[randIndex];
            shuffledSlots[randIndex] = temp;
        }

        int count = Mathf.Min(todaysPatients.Count, shuffledSlots.Count);

        for (int i = 0; i < count; i++)
        {
            NPCActor npc = todaysPatients[i];
            Transform slot = shuffledSlots[i];

            if (npc == null || slot == null) continue;

            npc.transform.position = slot.position;
            npc.transform.rotation = slot.rotation;

            Debug.Log($"{npc.npcName} colocado aleatoriamente no slot {slot.name}");
        }
    }
}