using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlackMarketSpreaderUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private VirusLabManager virusLabManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("Economy")]
    [SerializeField] private int releasePrice = 200;

    [Header("Panels")]
    [SerializeField] private GameObject spreaderPanel;
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private GameObject victimListPanel;
    [SerializeField] private GameObject victimDetailPanel;

    [Header("Dialogue")]
    [SerializeField] private TMP_Text dialogueText;

    [Header("Victim List")]
    [SerializeField] private Transform victimListContainer;
    [SerializeField] private PatientRecordEntryUI victimEntryPrefab;

    [Header("Victim Detail")]
    [SerializeField] private TMP_Text victimNameText;
    [SerializeField] private TMP_Text victimStatusText;
    [SerializeField] private TMP_Text victimAgeText;
    [SerializeField] private TMP_Text victimProfessionText;
    [SerializeField] private TMP_Text victimPersonalityText;
    [SerializeField] private TMP_Text victimSocialTraitText;
    [SerializeField] private TMP_Text victimSkillTraitText;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private bool isOpen;
    private NPCActor selectedVictim;

    private void Start()
    {
        if (spreaderPanel != null) spreaderPanel.SetActive(false);
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (victimListPanel != null) victimListPanel.SetActive(false);
        if (victimDetailPanel != null) victimDetailPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseAll();
        }
    }

    public void OpenSpreaderUI()
    {
        isOpen = true;
        selectedVictim = null;

        if (spreaderPanel != null) spreaderPanel.SetActive(true);

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        ShowDialogue();
    }

    public void CloseAll()
    {
        isOpen = false;
        selectedVictim = null;

        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (victimListPanel != null) victimListPanel.SetActive(false);
        if (victimDetailPanel != null) victimDetailPanel.SetActive(false);
        if (spreaderPanel != null) spreaderPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowDialogue()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(true);
        if (victimListPanel != null) victimListPanel.SetActive(false);
        if (victimDetailPanel != null) victimDetailPanel.SetActive(false);

        string message;

        if (activeVirusManager != null && activeVirusManager.HasExternalVirusActive)
            message = "Something is already spreading through the village. I won't make it worse. Come back when it's gone.";
        else if (activeVirusManager != null && activeVirusManager.HasPlayerVirusActive)
            message = "There's already a virus out there. Come back when it's done.";
        else if (virusLabManager == null || !virusLabManager.CurrentBlueprint.IsComplete)
            message = "You don't have a complete virus ready. Go finish it in your lab first.";
        else if (moneyManager == null || !moneyManager.HasEnough(releasePrice))
            message = $"I can infect someone for the right price. {releasePrice} coins. You don't have enough.";
        else
            message = $"I can infect someone for the right price. {releasePrice} coins and we have a deal.";

        if (dialogueText != null)
            dialogueText.text = message;

        SetFeedback("");
    }

    public void OnPickVictimClicked()
    {
        if (!CanReleaseVirus())
        {
            SetFeedback("Cannot release virus right now.");
            return;
        }

        ShowVictimList();
    }

    public void OnDeclineDialogueClicked()
    {
        CloseAll();
    }

    public void OnBackToDialogueClicked()
    {
        selectedVictim = null;
        ShowDialogue();
    }

    private void ShowVictimList()
    {
        if (dialoguePanel != null) dialoguePanel.SetActive(false);
        if (victimListPanel != null) victimListPanel.SetActive(true);
        if (victimDetailPanel != null) victimDetailPanel.SetActive(false);

        BuildVictimList();
    }

    private void BuildVictimList()
    {
        if (victimListContainer == null || victimEntryPrefab == null || dailySystem == null) return;

        foreach (Transform child in victimListContainer)
            Destroy(child.gameObject);

        foreach (var npc in dailySystem.AllNPCs)
        {
            if (npc == null || !npc.isAlive) continue;

            PatientRecordEntryUI entry = Instantiate(victimEntryPrefab, victimListContainer);
            entry.Setup(npc, null);
            entry.SetCustomClickAction(() => OnVictimSelected(npc));
        }
    }

    private void OnVictimSelected(NPCActor npc)
    {
        if (npc == null || !npc.isAlive) return;

        selectedVictim = npc;

        if (victimListPanel != null) victimListPanel.SetActive(false);
        if (victimDetailPanel != null) victimDetailPanel.SetActive(true);

        var record = npc.patientRecord;

        SetText(victimNameText, string.IsNullOrEmpty(record.patientName) ? npc.npcName : record.patientName);
        SetText(victimStatusText, record.status.ToString());
        SetText(victimAgeText, record.ageUnlocked ? record.age.ToString() : "???");
        SetText(victimProfessionText, record.professionUnlocked ? record.professionName : "???");
        SetText(victimPersonalityText, record.personalityUnlocked ? record.personalityName : "???");
        SetText(victimSocialTraitText, record.socialTraitUnlocked ? record.socialTraitName : "???");
        SetText(victimSkillTraitText, record.skillTraitUnlocked ? record.skillTraitName : "???");

        SetFeedback("");
    }

    public void OnBackToListClicked()
    {
        selectedVictim = null;
        ShowVictimList();
    }

    public void OnInfectClicked()
    {
        if (selectedVictim == null || !selectedVictim.isAlive)
        {
            SetFeedback("Invalid target.");
            return;
        }

        if (!CanReleaseVirus())
        {
            SetFeedback("Cannot release virus.");
            return;
        }

        moneyManager.SpendMoney(releasePrice);

        bool released = activeVirusManager.ReleaseVirus(virusLabManager.CurrentBlueprint, selectedVictim);

        if (released)
        {
            virusLabManager.ConsumeBlueprint();
            SetFeedback($"Virus released on {selectedVictim.npcName}!");
            Debug.Log($"[Spreader] Virus released on {selectedVictim.npcName}. Paid {releasePrice} coins.");
        }
        else
        {
            moneyManager.AddMoney(releasePrice);
            SetFeedback("Something went wrong. Money refunded.");
        }
    }

    private bool CanReleaseVirus()
    {
        if (activeVirusManager == null || activeVirusManager.HasActiveVirus) return false;
        if (virusLabManager == null || !virusLabManager.CurrentBlueprint.IsComplete) return false;
        if (moneyManager == null || !moneyManager.HasEnough(releasePrice)) return false;
        return true;
    }

    private void SetText(TMP_Text field, string value)
    {
        if (field != null) field.text = value;
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null) feedbackText.text = msg;
    }
}