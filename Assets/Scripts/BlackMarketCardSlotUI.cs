using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlackMarketCardSlotUI : MonoBehaviour
{
    [SerializeField] private GameObject cardContent;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text rarityText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button buyButton;

    [Header("Rarity Background")]
    [Tooltip("Image, RawImage or any Graphic whose color changes based on the upgrade's rarity.")]
    [SerializeField] private Graphic rarityBackground;

    [SerializeField] private Color commonColor = new Color(0.40f, 0.80f, 0.40f, 1f); // green
    [SerializeField] private Color rareColor = new Color(0.30f, 0.55f, 0.95f, 1f);   // blue

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
                stats += $"Lethality: {FormatSignedPercent(upgrade.lethalityPerDay)}/day\n";
            if (upgrade.dailyInfectionsCap != 0)
                stats += $"Daily spread: {FormatSignedInt(upgrade.dailyInfectionsCap)}\n";
            if (upgrade.totalInfectionsCap != 0)
                stats += $"Total spread: {FormatSignedInt(upgrade.totalInfectionsCap)}";
            statsText.text = stats.TrimEnd();
        }

        Color rarityColor = upgrade.rarity == VirusUpgradeRarity.Rare ? rareColor : commonColor;

        if (rarityText != null)
        {
            rarityText.text = upgrade.rarity.ToString();
            rarityText.color = rarityColor;
        }

        if (rarityBackground != null)
            rarityBackground.color = rarityColor;

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

    private static string FormatSignedPercent(float value)
    {
        return value >= 0
            ? $"+{value * 100f:0}%"
            : $"{value * 100f:0}%";
    }

    private static string FormatSignedInt(int value)
    {
        return value >= 0
            ? $"+{value}"
            : value.ToString();
    }
}