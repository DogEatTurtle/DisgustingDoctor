using System.Collections.Generic;
using UnityEngine;

public class PlayerUpgradeInventory : MonoBehaviour
{
    [Header("Owned Upgrades (Current)")]
    [SerializeField] private List<VirusUpgradeSO> ownedUpgrades = new();

    [Header("Ever Owned Upgrades (Persistent)")]
    [Tooltip("Tracks every upgrade the player has ever bought. Used by the external virus event to know which symptoms the player can recognize.")]
    [SerializeField] private List<VirusUpgradeSO> everOwnedUpgrades = new();

    public IReadOnlyList<VirusUpgradeSO> OwnedUpgrades => ownedUpgrades;
    public IReadOnlyList<VirusUpgradeSO> EverOwnedUpgrades => everOwnedUpgrades;

    public bool Contains(VirusUpgradeSO upgrade)
    {
        return upgrade != null && ownedUpgrades.Contains(upgrade);
    }

    public bool HasEverOwned(VirusUpgradeSO upgrade)
    {
        return upgrade != null && everOwnedUpgrades.Contains(upgrade);
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

        if (!everOwnedUpgrades.Contains(upgrade))
            everOwnedUpgrades.Add(upgrade);

        Debug.Log($"[Inventory] Added {upgrade.shortName}. Total owned: {ownedUpgrades.Count} | Ever owned: {everOwnedUpgrades.Count}");
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

        Debug.Log($"[Inventory] Ever owned: {everOwnedUpgrades.Count}");
        foreach (var up in everOwnedUpgrades)
        {
            if (up == null) continue;
            Debug.Log($"  - {up.shortName}");
        }
    }
}