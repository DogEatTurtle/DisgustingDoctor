using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackMarketCardSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject cardContent;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    private BlackMarketShopUI shopUI;
    private int cardIndex;

    public void Setup(VirusUpgradeSO upgrade, int index, BlackMarketShopUI ui)
    {
        shopUI = ui;
        cardIndex = index;

        if (cardContent != null)
            cardContent.SetActive(true);

        if (nameText != null)
            nameText.text = upgrade.shortName;

        if (statsText != null)
        {
            string stats = "";
            if (upgrade.lethalityPerDay != 0)
                stats += $"Lethality: +{upgrade.lethalityPerDay * 100f:0}%/day\n";
            if (upgrade.dailyInfectionsCap != 0)
                stats += $"Daily spread: +{upgrade.dailyInfectionsCap}\n";
            if (upgrade.totalInfectionsCap != 0)
                stats += $"Total spread: +{upgrade.totalInfectionsCap}";
            statsText.text = stats.TrimEnd();
        }

        if (priceText != null)
            priceText.text = $"{upgrade.basePrice} coins";

        if (buyButton != null)
        {
            buyButton.interactable = true;
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(() => shopUI.OnCardBuyClicked(cardIndex));
        }
    }

    public void SetEmpty()
    {
        if (cardContent != null)
            cardContent.SetActive(false);

        if (buyButton != null)
            buyButton.interactable = false;
    }
}