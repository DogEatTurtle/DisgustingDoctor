using UnityEngine;
using UnityEngine.InputSystem;

public class PatientRecordsComputerUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject patientRecordsPanel;
    [SerializeField] private PatientRecordsUI patientRecordsUI;

    [Header("Dependencies")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    public bool IsOpen => patientRecordsPanel != null && patientRecordsPanel.activeSelf;

    private void Start()
    {
        if (patientRecordsPanel != null)
            patientRecordsPanel.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseUI();
        }
    }

    public void OpenUI()
    {
        if (patientRecordsPanel != null)
            patientRecordsPanel.SetActive(true);

        if (patientRecordsUI != null)
            patientRecordsUI.OpenListView();

        if (fpsController != null)
            fpsController.enabled = false;

        if (lookInteractor != null)
            lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseUI()
    {
        if (patientRecordsPanel != null)
            patientRecordsPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        if (lookInteractor != null)
            lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}