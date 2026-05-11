using UnityEngine;
using UnityEngine.UI;

public class IntroPanelController : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("UI")]
    [SerializeField] private GameObject introPanel;
    [SerializeField] private Button understoodButton;
    [SerializeField] private Button closeButton;

    private void Start()
    {
        // Show panel at game start with paused time and free cursor
        if (introPanel != null) introPanel.SetActive(true);

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Time.timeScale = 0f;

        if (understoodButton != null)
        {
            understoodButton.onClick.RemoveAllListeners();
            understoodButton.onClick.AddListener(OnDismiss);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(OnDismiss);
        }
    }

    private void OnDismiss()
    {
        if (introPanel != null) introPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        Time.timeScale = 1f;
    }
}