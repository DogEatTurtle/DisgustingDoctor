using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseMenuController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;
    [SerializeField] private EndGameUI endGameUI;
    [SerializeField] private GuideController guideController;

    [Header("Pause Panels")]
    [SerializeField] private GameObject pauseMenuPanel;
    [SerializeField] private GameObject guideMenuPanel;
    [SerializeField] private GameObject guideTopicPanel;

    [Header("Buttons")]
    [SerializeField] private Button resumeButton;
    [SerializeField] private Button guideButton;
    [SerializeField] private Button returnToMenuButton;

    [Header("Block Pause When These Panels Are Active")]
    [Tooltip("If any of these GameObjects is active, pressing ESC will not open the pause menu. The active UI handles its own ESC.")]
    [SerializeField] private List<GameObject> blockingPanels = new();

    [Header("Scenes")]
    [SerializeField] private string mainMenuSceneName = "MainMenu";

    private bool isPaused;

    public bool IsPaused => isPaused;

    private void Start()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (guideMenuPanel != null) guideMenuPanel.SetActive(false);
        if (guideTopicPanel != null) guideTopicPanel.SetActive(false);

        if (resumeButton != null)
        {
            resumeButton.onClick.RemoveAllListeners();
            resumeButton.onClick.AddListener(OnResumeClicked);
        }
        if (guideButton != null)
        {
            guideButton.onClick.RemoveAllListeners();
            guideButton.onClick.AddListener(OnGuideClicked);
        }
        if (returnToMenuButton != null)
        {
            returnToMenuButton.onClick.RemoveAllListeners();
            returnToMenuButton.onClick.AddListener(OnReturnToMenuClicked);
        }
    }

    private void Update()
    {
        if (Keyboard.current == null) return;
        if (!Keyboard.current.escapeKey.wasPressedThisFrame) return;

        // Don't react to ESC if the end-game screen is showing
        if (endGameUI != null && endGameUI.IsShowing) return;

        // If we're already paused, ESC closes the pause menu (or the guide if open)
        if (isPaused)
        {
            // If guide topic is open, close it back to topics menu
            if (guideTopicPanel != null && guideTopicPanel.activeSelf)
            {
                if (guideController != null)
                    guideController.ShowTopicsMenu();
                return;
            }

            // If guide menu is open, close it back to pause menu
            if (guideMenuPanel != null && guideMenuPanel.activeSelf)
            {
                ShowPauseMenu();
                return;
            }

            // Otherwise, resume the game
            Resume();
            return;
        }

        // Not paused — only open if no other UI is blocking
        if (IsAnyBlockingPanelActive()) return;

        OpenPause();
    }

    private bool IsAnyBlockingPanelActive()
    {
        foreach (var panel in blockingPanels)
        {
            if (panel != null && panel.activeInHierarchy)
                return true;
        }
        return false;
    }

    private void OpenPause()
    {
        isPaused = true;

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        ShowPauseMenu();
    }

    private void Resume()
    {
        isPaused = false;

        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (guideMenuPanel != null) guideMenuPanel.SetActive(false);
        if (guideTopicPanel != null) guideTopicPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }

    private void ShowPauseMenu()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(true);
        if (guideMenuPanel != null) guideMenuPanel.SetActive(false);
        if (guideTopicPanel != null) guideTopicPanel.SetActive(false);
    }

    private void OnResumeClicked()
    {
        Resume();
    }

    private void OnGuideClicked()
    {
        if (pauseMenuPanel != null) pauseMenuPanel.SetActive(false);
        if (guideMenuPanel != null) guideMenuPanel.SetActive(true);

        if (guideController != null)
            guideController.ShowTopicsMenu();
    }

    // Called by GuideController when the player clicks "back to title/menu" inside the guide
    public void OnGuideReturnToMenu()
    {
        ShowPauseMenu();
    }

    private void OnReturnToMenuClicked()
    {
        // Restore time before scene change
        Time.timeScale = 1f;

        if (string.IsNullOrEmpty(mainMenuSceneName))
        {
            Debug.LogError("[PauseMenu] mainMenuSceneName is empty.");
            return;
        }

        SceneManager.LoadScene(mainMenuSceneName);
    }
}