using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class EndGameUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("UI")]
    [SerializeField] private GameObject endGamePanel;
    [SerializeField] private TMP_Text titleText;
    [SerializeField] private TMP_Text bodyText;
    [SerializeField] private Button mainMenuButton;
    [SerializeField] private Button detailsButton;

    [Header("Ending Texts (Configurable)")]
    [TextArea(3, 6)]
    [SerializeField] private string villageExtinctTitle = "The Village Falls Silent";
    [TextArea(3, 6)]
    [SerializeField]
    private string villageExtinctBody =
        "Every villager has perished. The clinic is empty, the streets quiet. " +
        "Whatever brought you here, your work is done — and so is the village.";

    [TextArea(3, 6)]
    [SerializeField] private string banishedTitle = "Run Out of Town";
    [TextArea(3, 6)]
    [SerializeField]
    private string banishedBody =
        "No one in the village trusts you anymore. They gather at your door with quiet, hard faces, " +
        "and tell you it would be best if you left before nightfall. You pack what you can carry.";

    [TextArea(3, 6)]
    [SerializeField] private string nobelTitle = "A Nobel Honor";
    [TextArea(3, 6)]
    [SerializeField]
    private string nobelBody =
        "Thirty days, and not a single life lost under your care. Word travels far — first to the next town, " +
        "then to the capital, then beyond. A letter arrives from Stockholm. The village waves you off with pride.";

    [TextArea(3, 6)]
    [SerializeField] private string vacationTitle = "A Quiet Departure";
    [TextArea(3, 6)]
    [SerializeField]
    private string vacationBody =
        "You buy the ticket without telling anyone. Whatever was happening in the village — good or bad — " +
        "is no longer your problem. The road south is long, and the sun is warm on your face.";

    [Header("Main Menu Scene")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isShowing;

    public bool IsShowing => isShowing;

    private void Start()
    {
        if (endGamePanel != null)
            endGamePanel.SetActive(false);

        if (mainMenuButton != null)
        {
            mainMenuButton.onClick.RemoveAllListeners();
            mainMenuButton.onClick.AddListener(OnMainMenuClicked);
        }
        if (detailsButton != null)
        {
            detailsButton.onClick.RemoveAllListeners();
            detailsButton.onClick.AddListener(OnDetailsClicked);
        }
    }

    // Forces cursor visible every frame while the end-game is showing.
    // This protects against other UIs that close after the ending and reset
    // the cursor state.
    private void LateUpdate()
    {
        if (!isShowing) return;

        if (Cursor.lockState != CursorLockMode.None)
            Cursor.lockState = CursorLockMode.None;
        if (!Cursor.visible)
            Cursor.visible = true;
    }

    public void ShowEnding(EndGameManager.EndingType type)
    {
        if (endGamePanel == null) return;

        isShowing = true;

        endGamePanel.SetActive(true);

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // Pause the game (timers, animations, etc.)
        Time.timeScale = 0f;

        switch (type)
        {
            case EndGameManager.EndingType.VillageExtinct:
                SetTexts(villageExtinctTitle, villageExtinctBody);
                break;
            case EndGameManager.EndingType.Banished:
                SetTexts(banishedTitle, banishedBody);
                break;
            case EndGameManager.EndingType.NobelPrize:
                SetTexts(nobelTitle, nobelBody);
                break;
            case EndGameManager.EndingType.Vacation:
                SetTexts(vacationTitle, vacationBody);
                break;
        }
    }

    private void SetTexts(string title, string body)
    {
        if (titleText != null) titleText.text = title;
        if (bodyText != null) bodyText.text = body;
    }

    private void OnMainMenuClicked()
    {
        // Restore time before changing scene
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogWarning("[EndGameUI] No mainMenuSceneName configured. Quitting application instead.");
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }

    private void OnDetailsClicked()
    {
        Debug.Log("[EndGameUI] Details button clicked. Stats panel not implemented yet.");
    }
}