using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ActiveVirus
{
    public DiseaseSO virusDiseaseSO;
    public List<string> combinedSymptoms = new();

    public float lethalityPerDay;
    public int dailyInfectionsCap;
    public int totalInfectionsBudget;
    public int totalInfectionsUsed;

    public List<NPCActor> currentlyInfected = new();

    // Patient zero queued for infection on the next day.
    // While the virus is "in transit", this NPC is immune to natural death
    // from any disease they currently have, and the virus has not yet
    // started propagating.
    public NPCActor pendingPatientZero;

    public bool IsActive => currentlyInfected.Count > 0 || pendingPatientZero != null;
    public int RemainingTotalBudget => Mathf.Max(0, totalInfectionsBudget - totalInfectionsUsed);

    public void RegisterInfection(NPCActor npc)
    {
        if (npc == null) return;
        if (currentlyInfected.Contains(npc)) return;
        currentlyInfected.Add(npc);
        totalInfectionsUsed++;
    }

    public void UnregisterInfection(NPCActor npc)
    {
        if (npc == null) return;
        currentlyInfected.Remove(npc);
    }
}