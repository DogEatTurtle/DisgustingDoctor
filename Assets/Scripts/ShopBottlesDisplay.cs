using UnityEngine;

public class ShopBottlesDisplay : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BlackMarketShopManager shopManager;

    [Header("Bottles (left to right)")]
    [Tooltip("Bottles in display order from left to right. They disappear from right to left as cards are bought.")]
    [SerializeField] private GameObject[] bottles;

    private int lastOfferCount = -1;

    private void Update()
    {
        if (shopManager == null || bottles == null) return;

        int currentCount = shopManager.TodaysOffers.Count;
        if (currentCount == lastOfferCount) return;

        lastOfferCount = currentCount;
        RefreshBottles(currentCount);
    }

    private void RefreshBottles(int visibleCount)
    {
        for (int i = 0; i < bottles.Length; i++)
        {
            if (bottles[i] != null)
                bottles[i].SetActive(i < visibleCount);
        }
    }
}