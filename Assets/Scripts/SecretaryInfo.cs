using System.Collections.Generic;
using UnityEngine;

public class SecretaryInfo : MonoBehaviour
{
    [Header("Settings")]
    [Tooltip("How many past days the secretary remembers. Today + N-1 previous days.")]
    [SerializeField] private int memoryWindowDays = 3;

    [Tooltip("Trust value at or above which an NPC is considered 'speaking well' of the doctor.")]
    [SerializeField, Range(0f, 1f)] private float trustHighThreshold = 0.80f;

    [Tooltip("Trust value at or below which an NPC is considered 'speaking ill' of the doctor.")]
    [SerializeField, Range(0f, 1f)] private float trustLowThreshold = 0.25f;

    [Header("State (Read Only)")]
    [SerializeField] private List<SecretaryEvent> eventBuffer = new();

    public int MemoryWindowDays => memoryWindowDays;
    public float TrustHighThreshold => trustHighThreshold;
    public float TrustLowThreshold => trustLowThreshold;

    public void RecordEvent(SecretaryEvent.EventType type, string npcName, int dayRecorded)
    {
        eventBuffer.Add(new SecretaryEvent(type, npcName, dayRecorded));
    }

    // Called at the end of each day to drop entries older than the memory window
    public void PruneOldEvents(int currentDay)
    {
        int cutoff = currentDay - memoryWindowDays + 1; // events from this day or later are kept
        eventBuffer.RemoveAll(e => e.dayRecorded < cutoff);
    }

    // ---------------- Query methods (used by the UI / prompt builder) ----------------

    public List<SecretaryEvent> GetRecentDeaths(int currentDay)
    {
        return eventBuffer.FindAll(e =>
            e.type == SecretaryEvent.EventType.Death &&
            IsWithinWindow(e.dayRecorded, currentDay));
    }

    public List<SecretaryEvent> GetRecentSickNotVisiting(int currentDay)
    {
        return eventBuffer.FindAll(e =>
            e.type == SecretaryEvent.EventType.SickNotVisiting &&
            IsWithinWindow(e.dayRecorded, currentDay));
    }

    private bool IsWithinWindow(int eventDay, int currentDay)
    {
        return eventDay > currentDay - memoryWindowDays;
    }

    [ContextMenu("Print Buffer")]
    public void PrintBuffer()
    {
        Debug.Log($"[SecretaryInfo] Buffer has {eventBuffer.Count} events:");
        foreach (var e in eventBuffer)
            Debug.Log($"  - Day {e.dayRecorded}: {e.type} ({e.npcName})");
    }
}