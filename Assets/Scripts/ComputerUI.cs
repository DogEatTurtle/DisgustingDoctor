using UnityEngine;
using UnityEngine.InputSystem;

public class ComputerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject computerPanel;

    [Header("Dependencies")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    public bool IsOpen => computerPanel != null && computerPanel.activeSelf;

    private void Start()
    {
        if (computerPanel != null)
            computerPanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseComputerUI();
        }
    }

    public void OpenComputerUI()
    {
        if (computerPanel != null)
            computerPanel.SetActive(true);

        if (fpsController != null)
            fpsController.enabled = false;

        if (lookInteractor != null)
            lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseComputerUI()
    {
        if (computerPanel != null)
            computerPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        if (lookInteractor != null)
            lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}