using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class VacationUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private EndGameManager endGameManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("UI")]
    [SerializeField] private GameObject vacationPanel;
    [SerializeField] private TMP_Text descriptionText;
    [SerializeField] private TMP_Text feedbackText;
    [SerializeField] private Button buyButton;
    [SerializeField] private Button closeButton;

    [Header("Description Template")]
    [TextArea(2, 4)]
    [Tooltip("Use {price} as a placeholder for the vacation price.")]
    [SerializeField]
    private string descriptionTemplate =
        "Sun, sea, and silence. Leave it all behind for {price} coins.";

    public bool IsOpen => vacationPanel != null && vacationPanel.activeSelf;

    private void Start()
    {
        if (vacationPanel != null)
            vacationPanel.SetActive(false);

        if (buyButton != null)
        {
            buyButton.onClick.RemoveAllListeners();
            buyButton.onClick.AddListener(OnBuyClicked);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseUI);
        }
    }

    private void Update()
    {
        if (!IsOpen) return;
        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseUI();
    }

    public void OpenUI()
    {
        if (vacationPanel != null) vacationPanel.SetActive(true);

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshUI();
    }

    public void CloseUI()
    {
        if (vacationPanel != null) vacationPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Closes the panel without restoring fps/cursor state. Used when
    // transitioning to another UI (the end-game screen) that will manage
    // input control itself.
    public void CloseUISilently()
    {
        if (vacationPanel != null) vacationPanel.SetActive(false);
    }

    private void RefreshUI()
    {
        int price = endGameManager != null ? endGameManager.VacationCost : 0;

        if (descriptionText != null)
            descriptionText.text = descriptionTemplate.Replace("{price}", price.ToString());

        if (feedbackText != null)
            feedbackText.text = "";

        if (buyButton != null)
            buyButton.interactable = endGameManager != null && endGameManager.CanAffordVacation;
    }

    private void OnBuyClicked()
    {
        if (endGameManager == null) return;

        bool success = endGameManager.TryTriggerVacation();
        if (!success)
        {
            if (feedbackText != null)
                feedbackText.text = "You can't afford this.";
            return;
        }

        // The end-game UI is now showing and managing input/cursor.
        // Just hide our panel without touching cursor or fps controller.
        CloseUISilently();
    }
}