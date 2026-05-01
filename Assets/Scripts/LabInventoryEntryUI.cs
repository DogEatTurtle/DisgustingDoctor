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
            parts.Add($"Leth {FormatSignedPercent(upgrade.lethalityPerDay)}/day");
        if (upgrade.dailyInfectionsCap != 0)
            parts.Add($"Daily {FormatSignedInt(upgrade.dailyInfectionsCap)}");
        if (upgrade.totalInfectionsCap != 0)
            parts.Add($"Total {FormatSignedInt(upgrade.totalInfectionsCap)}");

        return string.Join(" | ", parts);
    }

    private static string FormatSignedPercent(float value)
    {
        return value >= 0
            ? $"+{value * 100f:0}%"
            : $"{value * 100f:0}%"; // negative numbers already include the sign
    }

    private static string FormatSignedInt(int value)
    {
        return value >= 0
            ? $"+{value}"
            : value.ToString();
    }
}