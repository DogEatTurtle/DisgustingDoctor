using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class VirusBlueprint
{
    public const int SlotCount = 4;

    [SerializeField] private VirusUpgradeSO[] slots = new VirusUpgradeSO[SlotCount];

    public IReadOnlyList<VirusUpgradeSO> Slots => slots;

    public bool IsComplete
    {
        get
        {
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] == null) return false;
            return true;
        }
    }

    public int FilledSlotCount
    {
        get
        {
            int count = 0;
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] != null) count++;
            return count;
        }
    }

    public bool Contains(VirusUpgradeSO upgrade)
    {
        if (upgrade == null) return false;
        for (int i = 0; i < SlotCount; i++)
            if (slots[i] == upgrade) return true;
        return false;
    }

    public bool SetSlot(int slotIndex, VirusUpgradeSO upgrade)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return false;
        if (upgrade == null) return false;
        if (Contains(upgrade)) return false;

        slots[slotIndex] = upgrade;
        return true;
    }

    public VirusUpgradeSO ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= SlotCount) return null;
        var removed = slots[slotIndex];
        slots[slotIndex] = null;
        return removed;
    }

    public void ClearAll()
    {
        for (int i = 0; i < SlotCount; i++)
            slots[i] = null;
    }

    public float TotalLethalityPerDay
    {
        get
        {
            float total = 0f;
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] != null) total += slots[i].lethalityPerDay;
            return Mathf.Clamp01(total);
        }
    }

    public int TotalDailyInfectionsCap
    {
        get
        {
            int total = 0;
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] != null) total += slots[i].dailyInfectionsCap;
            return Mathf.Max(0, total);
        }
    }

    public int TotalInfectionsCap
    {
        get
        {
            int total = 0;
            for (int i = 0; i < SlotCount; i++)
                if (slots[i] != null) total += slots[i].totalInfectionsCap;
            return Mathf.Max(0, total);
        }
    }

    public List<string> BuildCombinedLLMSymptoms()
    {
        var result = new List<string>();
        for (int i = 0; i < SlotCount; i++)
        {
            if (slots[i] != null && !string.IsNullOrWhiteSpace(slots[i].llmSymptomSentence))
                result.Add(slots[i].llmSymptomSentence);
        }
        return result;
    }
}