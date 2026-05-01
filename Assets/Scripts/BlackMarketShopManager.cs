using System.Collections.Generic;
using UnityEngine;

public class BlackMarketShopManager : MonoBehaviour
{
    [Header("All Upgrades In Game")]
    [SerializeField] private List<VirusUpgradeSO> allUpgrades = new();

    [Header("Dependencies")]
    [SerializeField] private PlayerUpgradeInventory playerInventory;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private DiagnosisCounter diagnosisCounter;

    [Header("Today's Offers (Read Only)")]
    [SerializeField] private List<VirusUpgradeSO> todaysOffers = new();

    public IReadOnlyList<VirusUpgradeSO> TodaysOffers => todaysOffers;

    public void RefreshDailyOffers()
    {
        todaysOffers.Clear();

        var pool = BuildAvailablePool();
        if (pool.Count == 0)
        {
            Debug.Log("[Shop] No upgrades available in the pool today.");
            return;
        }

        int offerCount = Mathf.Min(3, pool.Count);

        for (int i = 0; i < offerCount; i++)
        {
            int idx = Random.Range(0, pool.Count);
            todaysOffers.Add(pool[idx]);
            pool.RemoveAt(idx);
        }

        Debug.Log($"[Shop] Today's offers: {todaysOffers.Count}");
        foreach (var offer in todaysOffers)
            Debug.Log($"  - {offer.shortName} ({offer.rarity}) | Price: {offer.basePrice}");
    }

    public bool TryBuyUpgrade(VirusUpgradeSO upgrade)
    {
        if (upgrade == null) return false;

        if (!todaysOffers.Contains(upgrade))
        {
            Debug.LogWarning($"[Shop] {upgrade.shortName} is not available today.");
            return false;
        }

        if (playerInventory != null && playerInventory.Contains(upgrade))
        {
            Debug.LogWarning($"[Shop] Player already owns {upgrade.shortName}.");
            return false;
        }

        if (moneyManager == null)
        {
            Debug.LogError("[Shop] No MoneyManager assigned.");
            return false;
        }

        if (!moneyManager.HasEnough(upgrade.basePrice))
        {
            Debug.LogWarning($"[Shop] Not enough money for {upgrade.shortName}. Need {upgrade.basePrice}.");
            return false;
        }

        moneyManager.SpendMoney(upgrade.basePrice);

        if (playerInventory != null)
            playerInventory.AddUpgrade(upgrade);

        todaysOffers.Remove(upgrade);

        Debug.Log($"[Shop] Bought {upgrade.shortName} for {upgrade.basePrice} coins.");
        return true;
    }

    private List<VirusUpgradeSO> BuildAvailablePool()
    {
        var pool = new List<VirusUpgradeSO>();

        foreach (var upgrade in allUpgrades)
        {
            if (upgrade == null) continue;

            // Skip if player already owns it
            if (playerInventory != null && playerInventory.Contains(upgrade))
                continue;

            // Rare upgrades require enough correct diagnoses of a specific disease
            if (upgrade.IsRareLocked && !IsRareUnlocked(upgrade))
                continue;

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
}