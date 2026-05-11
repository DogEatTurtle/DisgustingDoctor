using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndGameStatsUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private GameStats gameStats;
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private EndGameUI endGameUI;
    [SerializeField] private EndGameManager endGameManager;

    [Header("UI")]
    [SerializeField] private GameObject statsPanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text reportText;
    [SerializeField] private Button backButton;
    [SerializeField] private Button exportButton;
    [SerializeField] private TMP_Text feedbackText;

    [Header("Settings")]
    [SerializeField] private string panelTitle = "Run Details";

    public bool IsOpen => statsPanel != null && statsPanel.activeSelf;

    private string lastReport;
    private EndGameManager.EndingType lastEnding;

    private void Start()
    {
        if (statsPanel != null) statsPanel.SetActive(false);

        if (backButton != null)
        {
            backButton.onClick.RemoveAllListeners();
            backButton.onClick.AddListener(OnBackClicked);
        }

        if (exportButton != null)
        {
            exportButton.onClick.RemoveAllListeners();
            exportButton.onClick.AddListener(OnExportClicked);
        }

        if (feedbackText != null)
            feedbackText.text = "";
    }

    // Called by EndGameUI when the user clicks "Details"
    public void ShowStats(EndGameManager.EndingType ending)
    {
        if (statsPanel == null) return;

        lastEnding = ending;

        // Build report
        int alive = 0, dead = 0;
        float trustSum = 0f;
        if (dailySystem != null)
        {
            foreach (var npc in dailySystem.AllNPCs)
            {
                if (npc == null) continue;
                if (npc.isAlive)
                {
                    alive++;
                    trustSum += npc.trustInDoctor;
                }
                else dead++;
            }
        }
        float averageTrust = alive > 0 ? trustSum / alive : 0f;

        int currentDay = dayManager != null ? dayManager.GetCurrentDay() : 0;
        int finalMoney = moneyManager != null ? moneyManager.GetCurrentMoney() : 0;

        lastReport = gameStats != null
            ? gameStats.BuildReport(alive, dead, averageTrust, currentDay, ending, finalMoney)
            : "(No stats available)";

        if (titleText != null) titleText.text = panelTitle;
        if (reportText != null) reportText.text = lastReport;
        if (feedbackText != null) feedbackText.text = "";

        statsPanel.SetActive(true);
    }

    private void OnBackClicked()
    {
        if (statsPanel != null) statsPanel.SetActive(false);
    }

    private void OnExportClicked()
    {
        if (gameStats == null || string.IsNullOrEmpty(lastReport))
        {
            if (feedbackText != null) feedbackText.text = "Nothing to export.";
            return;
        }

        string path = gameStats.ExportToFile(lastReport);
        if (feedbackText != null)
        {
            feedbackText.text = string.IsNullOrEmpty(path)
                ? "Export failed."
                : $"Exported to:\n{path}";
        }
    }
}