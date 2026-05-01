using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class LabBlueprintSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Filled Card Content")]
    [SerializeField] private GameObject filledContent;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;

    [Header("Empty State")]
    [SerializeField] private GameObject emptyContent;

    [Header("Remove Button (shown on hover when filled)")]
    [SerializeField] private Button removeButton;

    private VirusLabUI labUI;
    private int slotIndex;
    private bool isFilled;

    public void Setup(int index, VirusLabUI ui)
    {
        slotIndex = index;
        labUI = ui;

        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(OnRemoveClicked);
            removeButton.gameObject.SetActive(false);
        }
    }

    public void SetFilled(VirusUpgradeSO upgrade)
    {
        isFilled = true;

        if (filledContent != null) filledContent.SetActive(true);
        if (emptyContent != null) emptyContent.SetActive(false);

        if (nameText != null)
            nameText.text = upgrade.shortName;

        if (statsText != null)
            statsText.text = BuildFullStats(upgrade);

        // o botão só aparece em hover; por defeito fica escondido
        if (removeButton != null)
            removeButton.gameObject.SetActive(false);
    }

    public void SetEmpty()
    {
        isFilled = false;

        if (filledContent != null) filledContent.SetActive(false);
        if (emptyContent != null) emptyContent.SetActive(true);

        if (removeButton != null)
            removeButton.gameObject.SetActive(false);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isFilled && removeButton != null)
            removeButton.gameObject.SetActive(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (removeButton != null)
            removeButton.gameObject.SetActive(false);
    }

    private void OnRemoveClicked()
    {
        if (labUI != null)
            labUI.OnSlotRemoveClicked(slotIndex);
    }

    private static string BuildFullStats(VirusUpgradeSO upgrade)
    {
        string stats = "";
        if (upgrade.lethalityPerDay != 0)
            stats += $"Lethality: {FormatSignedPercent(upgrade.lethalityPerDay)}/day\n";
        if (upgrade.dailyInfectionsCap != 0)
            stats += $"Daily spread: {FormatSignedInt(upgrade.dailyInfectionsCap)}\n";
        if (upgrade.totalInfectionsCap != 0)
            stats += $"Total spread: {FormatSignedInt(upgrade.totalInfectionsCap)}";
        return stats.TrimEnd();
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