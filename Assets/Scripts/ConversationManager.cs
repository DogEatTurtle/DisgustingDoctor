using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class ConversationManager : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private OllamaClient ollamaClient;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

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

        string systemPrompt = BuildSystemPrompt(activeNPC);
        string userPrompt = BuildUserPrompt(activeNPC, playerMessage);

        string reply = await ollamaClient.ChatOnceAsync(systemPrompt, userPrompt);

        if (npcReplyText != null)
            npcReplyText.text = reply;

        isBusy = false;

        if (inputField != null)
            inputField.ActivateInputField();
    }

    public void SubmitFromInput(string _)
    {
        Submit();
    }

    private string BuildSystemPrompt(NPCActor npc)
    {
        string personalityName = npc.basePersonality != null ? npc.basePersonality.profileName : "Unknown";
        string speakingStyle = npc.basePersonality != null ? npc.basePersonality.speakingStyleNotes : "";
        string socialTrait = npc.socialTrait != null ? npc.socialTrait.traitName : "None";
        string socialHint = npc.socialTrait != null ? npc.socialTrait.llmHint : "";
        string skillTrait = npc.skillTrait != null ? npc.skillTrait.traitName : "None";
        string skillHint = npc.skillTrait != null ? npc.skillTrait.llmHint : "";
        string diseaseName = npc.currentDisease != null ? npc.currentDisease.diseaseName : "None";

        float talkativeness = npc.basePersonality != null ? npc.basePersonality.talkativeness : 0.5f;
        float directness = npc.basePersonality != null ? npc.basePersonality.directness : 0.5f;
        float cooperativeness = npc.basePersonality != null ? npc.basePersonality.cooperativeness : 0.5f;
        float dramatization = npc.basePersonality != null ? npc.basePersonality.dramatization : 0.5f;

        return
$@"You are roleplaying as a villager speaking to the doctor during a medical consultation in a game.

Character data:
- Name: {npc.npcName}
- Age: {npc.age}
- Base personality: {personalityName}
- Speaking style: {speakingStyle}
- Social trait: {socialTrait}
- Social trait hint: {socialHint}
- Skill trait: {skillTrait}
- Skill trait hint: {skillHint}
- Current health problem: {diseaseName}
- Talkativeness: {talkativeness:0.00}
- Directness: {directness:0.00}
- Cooperativeness: {cooperativeness:0.00}
- Dramatization: {dramatization:0.00}

Rules:
- Speak like a believable patient, not like a medical textbook.
- Do not directly reveal the disease name unless it would make sense.
- Describe symptoms, sensations, recent context, and worries naturally.
- Stay consistent with the assigned disease and personality.
- Keep replies fairly short, usually 1 to 3 sentences.";
    }

    private string BuildUserPrompt(NPCActor npc, string playerMessage)
    {
        return
$@"The doctor asks:
{playerMessage}

Reply as the patient.";
    }
}