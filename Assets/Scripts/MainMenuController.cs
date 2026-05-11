using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainMenuController : MonoBehaviour
{
    [Header("Scenes")]
    [SerializeField] private string gameSceneName = "Game";

    [Header("Panels")]
    [SerializeField] private GameObject titlePanel;
    [SerializeField] private GameObject guideMenuPanel;

    [Header("Buttons (Title)")]
    [SerializeField] private Button startButton;
    [SerializeField] private Button guideButton;
    [SerializeField] private Button quitButton;

    [Header("Optional")]
    [SerializeField] private GuideController guideController;

    private void Start()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        if (titlePanel != null) titlePanel.SetActive(true);
        if (guideMenuPanel != null) guideMenuPanel.SetActive(false);

        if (startButton != null)
        {
            startButton.onClick.RemoveAllListeners();
            startButton.onClick.AddListener(OnStartClicked);
        }
        if (guideButton != null)
        {
            guideButton.onClick.RemoveAllListeners();
            guideButton.onClick.AddListener(OnGuideClicked);
        }
        if (quitButton != null)
        {
            quitButton.onClick.RemoveAllListeners();
            quitButton.onClick.AddListener(OnQuitClicked);
        }
    }

    private void OnStartClicked()
    {
        if (string.IsNullOrEmpty(gameSceneName))
        {
            Debug.LogError("[MainMenu] gameSceneName is empty.");
            return;
        }

        Time.timeScale = 1f;
        SceneManager.LoadScene(gameSceneName);
    }

    private void OnGuideClicked()
    {
        if (titlePanel != null) titlePanel.SetActive(false);
        if (guideMenuPanel != null) guideMenuPanel.SetActive(true);

        if (guideController != null)
            guideController.ShowTopicsMenu();
    }

    public void OnReturnToTitleClicked()
    {
        if (guideMenuPanel != null) guideMenuPanel.SetActive(false);
        if (titlePanel != null) titlePanel.SetActive(true);
    }

    private void OnQuitClicked()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}