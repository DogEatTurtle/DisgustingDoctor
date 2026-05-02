using System.Collections.Generic;
using UnityEngine;

public class VirusLabManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerUpgradeInventory playerInventory;
    [SerializeField] private ActiveVirusManager activeVirusManager;

    [Header("Current Blueprint")]
    [SerializeField] private VirusBlueprint currentBlueprint = new VirusBlueprint();

    public VirusBlueprint CurrentBlueprint => currentBlueprint;
    public PlayerUpgradeInventory Inventory => playerInventory;

    public bool TryPutUpgradeInSlot(int slotIndex, VirusUpgradeSO upgrade)
    {
        if (playerInventory == null) return false;
        if (upgrade == null) return false;

        if (!playerInventory.Contains(upgrade))
        {
            Debug.LogWarning($"[Lab] Player does not own {upgrade.shortName}.");
            return false;
        }

        if (currentBlueprint.Contains(upgrade))
        {
            Debug.LogWarning($"[Lab] {upgrade.shortName} is already in the blueprint.");
            return false;
        }

        bool placed = currentBlueprint.SetSlot(slotIndex, upgrade);
        if (placed)
            Debug.Log($"[Lab] Placed {upgrade.shortName} in slot {slotIndex}.");
        else
            Debug.LogWarning($"[Lab] Could not place {upgrade.shortName} in slot {slotIndex}.");

        return placed;
    }

    public void RemoveFromSlot(int slotIndex)
    {
        var removed = currentBlueprint.ClearSlot(slotIndex);
        if (removed != null)
            Debug.Log($"[Lab] Removed {removed.shortName} from slot {slotIndex}.");
    }

    public void ClearBlueprint()
    {
        currentBlueprint.ClearAll();
        Debug.Log("[Lab] Blueprint cleared.");
    }

    [ContextMenu("Print Blueprint")]
    public void PrintBlueprint()
    {
        Debug.Log("[Lab] Current blueprint:");
        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            var u = currentBlueprint.Slots[i];
            Debug.Log($"  Slot {i}: {(u != null ? u.shortName : "(empty)")}");
        }
        Debug.Log(
            $"[Lab] Totals -> Lethality/day: {currentBlueprint.TotalLethalityPerDay:0.00} | " +
            $"Daily cap: {currentBlueprint.TotalDailyInfectionsCap} | " +
            $"Total cap: {currentBlueprint.TotalInfectionsCap} | " +
            $"Complete: {currentBlueprint.IsComplete}"
        );
    }

    public void ConsumeBlueprint()
    {
        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            var upgrade = currentBlueprint.Slots[i];
            if (upgrade != null && playerInventory != null)
                playerInventory.RemoveUpgrade(upgrade);
        }

        currentBlueprint.ClearAll();
        Debug.Log("[Lab] Blueprint consumed. Upgrades returned to market pool.");
    }

    // ---------------- CURE LOGIC ----------------

    public class CureResult
    {
        public int correctCount;
        public int totalSlots;
        public List<int> correctSlotIndices = new();
        public List<VirusUpgradeSO> correctUpgrades = new();
        public List<VirusUpgradeSO> wrongUpgrades = new();
        public bool isPerfect;
    }

    // Validates the current blueprint against the active external virus.
    // Correct symptoms stay in the blueprint AND in the inventory.
    // Wrong symptoms are removed from the blueprint AND from the inventory.
    // Returns a CureResult describing what happened.
    public CureResult TryCure()
    {
        var result = new CureResult { totalSlots = VirusBlueprint.SlotCount };

        if (activeVirusManager == null || !activeVirusManager.HasExternalVirusActive)
        {
            Debug.LogWarning("[Lab] TryCure called but no external virus is active.");
            return result;
        }

        if (!currentBlueprint.IsComplete)
        {
            Debug.LogWarning("[Lab] TryCure called but blueprint is incomplete.");
            return result;
        }

        var virusUpgrades = activeVirusManager.GetExternalVirusUpgrades();
        if (virusUpgrades == null)
        {
            Debug.LogWarning("[Lab] No external virus upgrades available for matching.");
            return result;
        }

        // Compare each slot against the virus's source upgrades
        var virusSet = new HashSet<VirusUpgradeSO>(virusUpgrades);

        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            var slotUpgrade = currentBlueprint.Slots[i];
            if (slotUpgrade == null) continue;

            if (virusSet.Contains(slotUpgrade))
            {
                result.correctCount++;
                result.correctSlotIndices.Add(i);
                result.correctUpgrades.Add(slotUpgrade);
            }
            else
            {
                result.wrongUpgrades.Add(slotUpgrade);
            }
        }

        result.isPerfect = result.correctCount == result.totalSlots;

        // Remove wrong upgrades from blueprint and inventory
        foreach (var wrong in result.wrongUpgrades)
        {
            for (int i = 0; i < VirusBlueprint.SlotCount; i++)
            {
                if (currentBlueprint.Slots[i] == wrong)
                {
                    currentBlueprint.ClearSlot(i);
                    break;
                }
            }
            if (playerInventory != null)
                playerInventory.RemoveUpgrade(wrong);
        }

        Debug.Log(
            $"[Lab] Cure attempt: {result.correctCount}/{result.totalSlots} correct. " +
            $"Wrong removed: {result.wrongUpgrades.Count}. Perfect: {result.isPerfect}"
        );

        return result;
    }

    // Called after a successful 4/4 cure to clear remaining correct upgrades
    // (they're consumed as the cure is applied)
    public void ConsumeCureBlueprint()
    {
        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            var upgrade = currentBlueprint.Slots[i];
            if (upgrade != null && playerInventory != null)
                playerInventory.RemoveUpgrade(upgrade);
        }
        currentBlueprint.ClearAll();
        Debug.Log("[Lab] Cure blueprint consumed.");
    }
}