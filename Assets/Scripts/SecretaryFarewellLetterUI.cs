using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SecretaryFarewellLetterUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("UI")]
    [SerializeField] private GameObject letterPanel;
    [SerializeField] private TMP_Text letterText;
    [SerializeField] private Button closeButton;

    [Header("Letter Content")]
    [TextArea(5, 15)]
    [SerializeField]
    private string letterBody =
        "Doctor,\n\n" +
        "I cannot stay here any longer. Too many people have died in too few days, and the fear in this village has become unbearable. " +
        "I joined this clinic to help, but what I see now is beyond anything I can endure.\n\n" +
        "I hope you find peace, whatever path leads you there.\n\n" +
        "Goodbye.";

    public bool IsOpen => letterPanel != null && letterPanel.activeSelf;

    private void Start()
    {
        if (letterPanel != null)
            letterPanel.SetActive(false);

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
        if (letterPanel != null) letterPanel.SetActive(true);
        if (letterText != null) letterText.text = letterBody;

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseUI()
    {
        if (letterPanel != null) letterPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}