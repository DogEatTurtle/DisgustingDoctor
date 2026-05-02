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

    // Patient zero queued for infection on the next day (player virus only).
    // External viruses infect immediately, so this stays null for them.
    public NPCActor pendingPatientZero;

    // Identifies whether this virus was created by the player or appeared as
    // an external event. Affects rewards, blocking rules, and cure mechanics.
    public bool isExternal;

    // Original upgrade SOs that compose this virus. Used by the cure mechanic
    // to validate symptom matching against the external virus.
    public List<VirusUpgradeSO> sourceUpgrades = new();

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