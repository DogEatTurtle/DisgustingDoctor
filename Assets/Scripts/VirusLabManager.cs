using UnityEngine;

public class VirusLabManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private PlayerUpgradeInventory playerInventory;

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
}