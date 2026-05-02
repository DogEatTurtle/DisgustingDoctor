using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class VirusLabUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private VirusLabManager labManager;
    [SerializeField] private PlayerUpgradeInventory inventory;
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private ExternalVirusEvent externalVirusEvent;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("Panels")]
    [SerializeField] private GameObject labPanel;

    [Header("Inventory List (Left)")]
    [SerializeField] private Transform inventoryListContainer;
    [SerializeField] private LabInventoryEntryUI inventoryEntryPrefab;

    [Header("Blueprint Slots (Right, 4)")]
    [SerializeField] private List<LabBlueprintSlotUI> blueprintSlots = new();

    [Header("Totals")]
    [SerializeField] private TMP_Text totalsText;
    [SerializeField] private TMP_Text statusText;

    [Header("Cure Button (shown only when external virus is active and discovered)")]
    [SerializeField] private GameObject tryCureButton;
    [SerializeField] private TMP_Text cureButtonLabelText;

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private bool isOpen;
    private bool externalVirusDiscovered; // true after at least one correct Unknown Virus diagnosis
    private bool cureAttemptUsedToday;

    public bool ExternalVirusDiscovered => externalVirusDiscovered;

    private void Start()
    {
        if (labPanel != null)
            labPanel.SetActive(false);

        for (int i = 0; i < blueprintSlots.Count; i++)
        {
            if (blueprintSlots[i] != null)
                blueprintSlots[i].Setup(i, this);
        }

        if (tryCureButton != null)
            tryCureButton.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseLab();
        }
    }

    public void OpenLab()
    {
        if (labManager == null || inventory == null) return;

        isOpen = true;

        if (labPanel != null)
            labPanel.SetActive(true);

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshAll();
    }

    public void CloseLab()
    {
        isOpen = false;

        if (labPanel != null)
            labPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Called by DiagnosisUI when player correctly diagnoses Unknown Virus
    public void NotifyUnknownVirusDiagnosed()
    {
        externalVirusDiscovered = true;
        Debug.Log("[Lab] External virus discovered. 'Try cure' button will be available in the lab.");
    }

    // Called by DailySystem.ProcessNewDay
    public void OnNewDay()
    {
        cureAttemptUsedToday = false;

        // Reset discovery flag when there's no external virus active anymore
        if (activeVirusManager == null || !activeVirusManager.HasExternalVirusActive)
            externalVirusDiscovered = false;
    }

    // ---------------- Event handlers chamados pelos filhos ----------------

    public void OnInventoryEntryClicked(VirusUpgradeSO upgrade)
    {
        if (labManager == null || upgrade == null) return;

        var blueprint = labManager.CurrentBlueprint;
        int firstEmpty = -1;
        for (int i = 0; i < VirusBlueprint.SlotCount; i++)
        {
            if (blueprint.Slots[i] == null)
            {
                firstEmpty = i;
                break;
            }
        }

        if (firstEmpty < 0)
        {
            SetFeedback("Blueprint is full.");
            return;
        }

        bool placed = labManager.TryPutUpgradeInSlot(firstEmpty, upgrade);
        if (placed)
        {
            // Adding a new symptom invalidates the previous correctness state
            ClearCorrectMarks();
            SetFeedback($"Added {upgrade.shortName} to slot {firstEmpty + 1}.");
            RefreshAll();
        }
        else
        {
            SetFeedback("Could not add upgrade.");
        }
    }

    public void OnSlotRemoveClicked(int slotIndex)
    {
        if (labManager == null) return;

        var blueprint = labManager.CurrentBlueprint;
        if (slotIndex < 0 || slotIndex >= VirusBlueprint.SlotCount) return;
        if (blueprint.Slots[slotIndex] == null) return;

        labManager.RemoveFromSlot(slotIndex);
        ClearCorrectMarks();
        SetFeedback($"Removed upgrade from slot {slotIndex + 1}.");
        RefreshAll();
    }

    public void OnClearBlueprintClicked()
    {
        if (labManager == null) return;
        labManager.ClearBlueprint();
        ClearCorrectMarks();
        SetFeedback("Blueprint cleared.");
        RefreshAll();
    }

    // Called by the "Try cure" button
    public void OnTryCureClicked()
    {
        if (labManager == null || activeVirusManager == null) return;

        if (!activeVirusManager.HasExternalVirusActive)
        {
            SetFeedback("No external virus active.");
            return;
        }

        if (cureAttemptUsedToday)
        {
            SetFeedback("You already attempted a cure today. Try again tomorrow.");
            return;
        }

        if (!labManager.CurrentBlueprint.IsComplete)
        {
            SetFeedback("Fill all 4 slots before attempting a cure.");
            return;
        }

        var result = labManager.TryCure();
        cureAttemptUsedToday = true;

        // Mark correct slots visually
        for (int i = 0; i < blueprintSlots.Count; i++)
        {
            if (blueprintSlots[i] == null) continue;
            bool isCorrect = result.correctSlotIndices.Contains(i);
            blueprintSlots[i].SetCorrectMark(isCorrect);
        }

        if (result.isPerfect)
        {
            int reward = externalVirusEvent != null ? externalVirusEvent.CureCompletionReward : 0;

            // Cure all infected NPCs and end the virus
            var cured = activeVirusManager.CureAllExternalVirusInfected();

            // Consume the remaining (correct) blueprint upgrades
            labManager.ConsumeCureBlueprint();

            if (moneyManager != null && reward > 0)
                moneyManager.AddMoney(reward);

            externalVirusDiscovered = false;

            SetFeedback($"Cure successful! {cured.Count} NPCs healed. +{reward} coins.");
            Debug.Log($"[Lab] Perfect cure! {cured.Count} cured. Reward: {reward} coins.");
        }
        else
        {
            SetFeedback($"Partial match: {result.correctCount}/{result.totalSlots} correct. Wrong symptoms lost.");
        }

        RefreshAll();
    }

    private void ClearCorrectMarks()
    {
        foreach (var slot in blueprintSlots)
        {
            if (slot != null) slot.ClearCorrectMark();
        }
    }

    // ---------------- Refresh ----------------

    private void RefreshAll()
    {
        RefreshInventoryList();
        RefreshSlots();
        RefreshTotals();
        RefreshCureButton();
    }

    private void RefreshInventoryList()
    {
        if (inventoryListContainer == null || inventoryEntryPrefab == null || inventory == null)
            return;

        foreach (Transform child in inventoryListContainer)
            Destroy(child.gameObject);

        var blueprint = labManager.CurrentBlueprint;

        foreach (var upgrade in inventory.OwnedUpgrades)
        {
            if (upgrade == null) continue;
            if (blueprint.Contains(upgrade)) continue;

            var entry = Instantiate(inventoryEntryPrefab, inventoryListContainer);
            entry.Setup(upgrade, this);
        }
    }

    private void RefreshSlots()
    {
        var blueprint = labManager.CurrentBlueprint;

        for (int i = 0; i < blueprintSlots.Count && i < VirusBlueprint.SlotCount; i++)
        {
            var slot = blueprintSlots[i];
            if (slot == null) continue;

            var upgrade = blueprint.Slots[i];
            if (upgrade != null)
                slot.SetFilled(upgrade);
            else
                slot.SetEmpty();
        }
    }

    private void RefreshTotals()
    {
        if (labManager == null) return;

        var blueprint = labManager.CurrentBlueprint;

        if (totalsText != null)
        {
            string totals =
                $"Lethality: +{blueprint.TotalLethalityPerDay * 100f:0}%/day\n" +
                $"Daily spread: +{blueprint.TotalDailyInfectionsCap}\n" +
                $"Total spread: +{blueprint.TotalInfectionsCap}";
            totalsText.text = totals;
        }

        if (statusText != null)
        {
            int filled = blueprint.FilledSlotCount;
            statusText.text = blueprint.IsComplete
                ? $"Virus status: {filled}/{VirusBlueprint.SlotCount} complete"
                : $"Virus status: {filled}/{VirusBlueprint.SlotCount}";
        }
    }

    private void RefreshCureButton()
    {
        if (tryCureButton == null) return;

        bool shouldShow =
            activeVirusManager != null &&
            activeVirusManager.HasExternalVirusActive &&
            externalVirusDiscovered;

        tryCureButton.SetActive(shouldShow);

        if (shouldShow && cureButtonLabelText != null)
        {
            cureButtonLabelText.text = cureAttemptUsedToday
                ? "Try cure (used today)"
                : "Try cure";
        }
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}