using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeInventory : MonoBehaviour
{
    [Header("Owned Upgrades")]
    [SerializeField] private List<VirusUpgradeSO> ownedUpgrades = new();

    public IReadOnlyList<VirusUpgradeSO> OwnedUpgrades => ownedUpgrades;

    public bool Contains(VirusUpgradeSO upgrade)
    {
        return upgrade != null && ownedUpgrades.Contains(upgrade);
    }

    public bool AddUpgrade(VirusUpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        if (ownedUpgrades.Contains(upgrade))
        {
            Debug.LogWarning($"[Inventory] Player already owns {upgrade.shortName}, cannot add twice.");
            return false;
        }
        ownedUpgrades.Add(upgrade);
        Debug.Log($"[Inventory] Added {upgrade.shortName}. Total owned: {ownedUpgrades.Count}");
        return true;
    }

    public bool RemoveUpgrade(VirusUpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        bool removed = ownedUpgrades.Remove(upgrade);
        if (removed)
            Debug.Log($"[Inventory] Removed {upgrade.shortName}. Total owned: {ownedUpgrades.Count}");
        return removed;
    }

    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        Debug.Log($"[Inventory] Player owns {ownedUpgrades.Count} upgrades:");
        foreach (var up in ownedUpgrades)
        {
            if (up == null) continue;
            Debug.Log($"  - {up.shortName} (lethality +{up.lethalityPerDay:0.00}, dailyCap +{up.dailyInfectionsCap}, totalCap +{up.totalInfectionsCap})");
        }
    }
}