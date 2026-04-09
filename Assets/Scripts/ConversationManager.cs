using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConversationManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private OllamaClient ollamaClient;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;
    [SerializeField] private PatientInfoExtractor infoExtractor;

    [Header("UI")]
    [SerializeField] private GameObject conversationPanel;
    [SerializeField] private TMP_Text npcNameText;
    [SerializeField] private TMP_Text npcReplyText;
    [SerializeField] private TMP_InputField inputField;

    private NPCActor activeNPC;
    private bool isOpen = false;
    private bool isBusy = false;

    public bool IsOpen => isOpen;
    public NPCActor ActiveNPC => activeNPC;

    private void Start()
    {
        if (conversationPanel != null)
            conversationPanel.SetActive(false);

        if (inputField != null)
            inputField.onSubmit.AddListener(SubmitFromInput);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseConversation();
        }
    }

    public void OpenConversation(NPCActor npc)
    {
        if (npc == null) return;

        activeNPC = npc;
        isOpen = true;

        if (conversationPanel != null)
            conversationPanel.SetActive(true);

        if (npcNameText != null)
            npcNameText.text = npc.npcName;

        if (npcReplyText != null)
            npcReplyText.text = "";

        if (lookInteractor != null)
            lookInteractor.enabled = false;

        if (inputField != null)
        {
            inputField.text = "";
            inputField.ActivateInputField();
        }

        if (fpsController != null)
            fpsController.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        Debug.Log($"Conversation opened with {npc.npcName}");
    }

    public void CloseConversation()
    {
        isOpen = false;
        isBusy = false;
        activeNPC = null;

        if (conversationPanel != null)
            conversationPanel.SetActive(false);

        if (inputField != null)
            inputField.text = "";

        if (lookInteractor != null)
            lookInteractor.enabled = true;

        if (fpsController != null)
            fpsController.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public async void Submit()
    {
        if (!isOpen || isBusy || activeNPC == null || ollamaClient == null || inputField == null)
            return;

        string playerMessage = inputField.text.Trim();
        if (string.IsNullOrEmpty(playerMessage))
            return;

        isBusy = true;

        if (npcReplyText != null)
            npcReplyText.text = "...";

        inputField.text = "";

        NPCActor npcForExtraction = activeNPC;

        var prompt = PromptBuilder.BuildPatientRoleplay(activeNPC, playerMessage);
        string reply = await ollamaClient.ChatOnceAsync(prompt.system, prompt.user);

        if (npcReplyText != null)
            npcReplyText.text = reply;

        if (infoExtractor != null)
            _ = infoExtractor.ExtractAndUnlockAsync(npcForExtraction, playerMessage, reply);

        isBusy = false;

        if (inputField != null)
            inputField.ActivateInputField();
    }

    public void SubmitFromInput(string _)
    {
        Submit();
    }
}