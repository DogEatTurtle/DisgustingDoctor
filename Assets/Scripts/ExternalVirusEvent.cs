using System.Collections.Generic;
using UnityEngine;

public class ExternalVirusEvent : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private BlackMarketShopManager blackMarketShop;
    [SerializeField] private DiagnosisCounter diagnosisCounter;

    [Header("Trigger Settings")]
    [Tooltip("Days without an active virus before the event can trigger.")]
    [SerializeField] private int gracePeriodDays = 10;

    [Tooltip("Probability curve from grace period day onward. Index 0 = day 10, index 1 = day 11, etc. The last value applies for all subsequent days.")]
    [SerializeField] private float[] triggerProbabilityCurve = new float[] { 0.25f, 0.50f, 0.75f, 1.00f };

    [Header("Reward")]
    [SerializeField] private int cureCompletionReward = 200;

    [Header("State (Read Only)")]
    [SerializeField] private int daysSinceLastVirus = 0;

    public int CureCompletionReward => cureCompletionReward;

    // Called by DailySystem.ProcessNewDay AFTER virus update, BEFORE everything else.
    public void TickAndMaybeTrigger()
    {
        // If a virus is currently active, reset the counter
        if (activeVirusManager != null && activeVirusManager.HasActiveVirus)
        {
            daysSinceLastVirus = 0;
            return;
        }

        daysSinceLastVirus++;

        if (daysSinceLastVirus < gracePeriodDays)
        {
            Debug.Log($"[ExternalEvent] Day {daysSinceLastVirus}/{gracePeriodDays} of grace period.");
            return;
        }

        // Compute current probability based on the curve
        int curveIndex = Mathf.Clamp(daysSinceLastVirus - gracePeriodDays, 0, triggerProbabilityCurve.Length - 1);
        float probability = triggerProbabilityCurve[curveIndex];

        Debug.Log($"[ExternalEvent] Day {daysSinceLastVirus}: rolling for trigger ({probability * 100f:0}% chance).");

        if (Random.value < probability)
        {
            TriggerEvent();
        }
    }

    private void TriggerEvent()
    {
        if (activeVirusManager == null || dailySystem == null)
        {
            Debug.LogWarning("[ExternalEvent] Cannot trigger: missing references.");
            return;
        }

        // 1. Build the pool of unlocked symptoms (anything that can appear in the shop)
        var unlockedPool = BuildUnlockedSymptomPool();
        if (unlockedPool.Count < VirusBlueprint.SlotCount)
        {
            Debug.LogWarning($"[ExternalEvent] Not enough unlocked symptoms ({unlockedPool.Count}). Need at least {VirusBlueprint.SlotCount}. Cancelling event.");
            return;
        }

        // 2. Pick 4 random distinct symptoms
        var picked = new List<VirusUpgradeSO>();
        var workingPool = new List<VirusUpgradeSO>(unlockedPool);
        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            int idx = Random.Range(0, workingPool.Count);
            picked.Add(workingPool[idx]);
            workingPool.RemoveAt(idx);
        }

        // 3. Pick a random alive NPC as patient zero
        var aliveNPCs = new List<NPCActor>();
        foreach (var npc in dailySystem.AllNPCs)
        {
            if (npc == null) continue;
            if (!npc.isAlive) continue;
            aliveNPCs.Add(npc);
        }

        if (aliveNPCs.Count == 0)
        {
            Debug.LogWarning("[ExternalEvent] No alive NPCs available for patient zero. Cancelling event.");
            return;
        }

        var patientZero = aliveNPCs[Random.Range(0, aliveNPCs.Count)];

        // 4. Release the external virus
        bool released = activeVirusManager.ReleaseExternalVirus(picked, patientZero);
        if (released)
        {
            daysSinceLastVirus = 0;
            Debug.Log($"[ExternalEvent] Triggered! Patient zero: {patientZero.npcName}. Symptoms: {string.Join(", ", picked.ConvertAll(p => p.shortName))}.");
        }
    }

    private List<VirusUpgradeSO> BuildUnlockedSymptomPool()
    {
        var pool = new List<VirusUpgradeSO>();
        if (blackMarketShop == null) return pool;

        // Use reflection-free approach: ask the shop manager for its full list,
        // and filter by what would currently be available in the pool (commons + unlocked rares)
        foreach (var upgrade in blackMarketShop.AllUpgrades)
        {
            if (upgrade == null) continue;
            if (upgrade.IsRareLocked && !IsRareUnlocked(upgrade)) continue;
            pool.Add(upgrade);
        }
        return pool;
    }

    private bool IsRareUnlocked(VirusUpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        if (!upgrade.IsRareLocked) return true;
        if (diagnosisCounter == null) return false;

        int count = diagnosisCounter.GetCount(upgrade.requiredDiseaseToCure);
        return count >= upgrade.curesNeededToUnlock;
    }

    [ContextMenu("Force Trigger Event Now")]
    public void DebugForceTrigger()
    {
        TriggerEvent();
    }
}