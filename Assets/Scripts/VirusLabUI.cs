using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class VirusLabUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private VirusLabManager labManager;
    [SerializeField] private PlayerUpgradeInventory inventory;
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

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private bool isOpen;

    private void Start()
    {
        if (labPanel != null)
            labPanel.SetActive(false);

        // configurar os slots com o seu índice
        for (int i = 0; i < blueprintSlots.Count; i++)
        {
            if (blueprintSlots[i] != null)
                blueprintSlots[i].Setup(i, this);
        }
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

    // ---------------- Event handlers chamados pelos filhos ----------------

    public void OnInventoryEntryClicked(VirusUpgradeSO upgrade)
    {
        if (labManager == null || upgrade == null) return;

        // encontrar o primeiro slot vazio
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
        SetFeedback($"Removed upgrade from slot {slotIndex + 1}.");
        RefreshAll();
    }

    // Ligado ao botão "Clear" no Inspector
    public void OnClearBlueprintClicked()
    {
        if (labManager == null) return;
        labManager.ClearBlueprint();
        SetFeedback("Blueprint cleared.");
        RefreshAll();
    }

    // ---------------- Refresh ----------------

    private void RefreshAll()
    {
        RefreshInventoryList();
        RefreshSlots();
        RefreshTotals();
    }

    private void RefreshInventoryList()
    {
        if (inventoryListContainer == null || inventoryEntryPrefab == null || inventory == null)
            return;

        // limpar lista actual
        foreach (Transform child in inventoryListContainer)
            Destroy(child.gameObject);

        var blueprint = labManager.CurrentBlueprint;

        // só mostrar os upgrades que o jogador tem E que não estão já na blueprint
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

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}