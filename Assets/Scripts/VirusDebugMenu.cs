using UnityEngine;

public class VirusDebugMenu : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerUpgradeInventory inventory;
    [SerializeField] private VirusLabManager lab;

    [Header("Testing")]
    [SerializeField] private VirusUpgradeSO upgradeToAdd;
    [SerializeField, Range(0, 3)] private int slotIndex = 0;

    [ContextMenu("Add Upgrade To Inventory")]
    public void AddUpgradeToInventory()
    {
        if (inventory == null || upgradeToAdd == null) return;
        inventory.AddUpgrade(upgradeToAdd);
    }

    [ContextMenu("Put Upgrade In Slot")]
    public void PutUpgradeInSlot()
    {
        if (lab == null || upgradeToAdd == null) return;
        lab.TryPutUpgradeInSlot(slotIndex, upgradeToAdd);
    }

    [ContextMenu("Remove From Slot")]
    public void RemoveFromSlot()
    {
        if (lab == null) return;
        lab.RemoveFromSlot(slotIndex);
    }

    [ContextMenu("Print Inventory")]
    public void PrintInventory()
    {
        if (inventory == null) return;
        inventory.PrintInventory();
    }

    [ContextMenu("Print Blueprint")]
    public void PrintBlueprint()
    {
        if (lab == null) return;
        lab.PrintBlueprint();
    }

    [ContextMenu("Clear Blueprint")]
    public void ClearBlueprint()
    {
        if (lab == null) return;
        lab.ClearBlueprint();
    }
}