using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LabInventoryEntryUI : MonoBehaviour
{
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private Button selectButton;

    private VirusUpgradeSO upgrade;
    private VirusLabUI labUI;

    public void Setup(VirusUpgradeSO source, VirusLabUI ui)
    {
        upgrade = source;
        labUI = ui;

        if (nameText != null)
            nameText.text = upgrade.shortName;

        if (statsText != null)
            statsText.text = BuildCompactStats(upgrade);

        if (selectButton != null)
        {
            selectButton.onClick.RemoveAllListeners();
            selectButton.onClick.AddListener(OnClicked);
        }
    }

    private void OnClicked()
    {
        if (labUI != null && upgrade != null)
            labUI.OnInventoryEntryClicked(upgrade);
    }

    private static string BuildCompactStats(VirusUpgradeSO upgrade)
    {
        var parts = new System.Collections.Generic.List<string>();

        if (upgrade.lethalityPerDay != 0)
            parts.Add($"Leth +{upgrade.lethalityPerDay * 100f:0}%/day");
        if (upgrade.dailyInfectionsCap != 0)
            parts.Add($"Daily +{upgrade.dailyInfectionsCap}");
        if (upgrade.totalInfectionsCap != 0)
            parts.Add($"Total +{upgrade.totalInfectionsCap}");

        return string.Join(" | ", parts);
    }
}