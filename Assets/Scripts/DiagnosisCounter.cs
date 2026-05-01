using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class DiagnosisCounter : MonoBehaviour
{
    [Serializable]
    public class DiseaseCount
    {
        public DiseaseSO disease;
        public int correctDiagnoses;
    }

    [Header("Excluded From Counting")]
    [Tooltip("Player virus and externally-introduced viruses do not count toward unlocking rare upgrades.")]
    [SerializeField] private List<DiseaseSO> excludedDiseases = new();

    [Header("Counts (Read Only)")]
    [SerializeField] private List<DiseaseCount> counts = new();

    // Internal lookup, rebuilt from `counts` on Awake for fast access
    private Dictionary<DiseaseSO, int> lookup = new();

    private void Awake()
    {
        RebuildLookup();
    }

    private void RebuildLookup()
    {
        lookup.Clear();
        foreach (var c in counts)
        {
            if (c == null || c.disease == null) continue;
            lookup[c.disease] = c.correctDiagnoses;
        }
    }

    public int GetCount(DiseaseSO disease)
    {
        if (disease == null) return 0;
        return lookup.TryGetValue(disease, out int n) ? n : 0;
    }

    public bool IsExcluded(DiseaseSO disease)
    {
        if (disease == null) return true;
        return excludedDiseases.Contains(disease);
    }

    public void RegisterCorrectDiagnosis(DiseaseSO disease)
    {
        if (disease == null) return;
        if (IsExcluded(disease))
        {
            Debug.Log($"[DiagnosisCounter] Skipped {disease.diseaseName} (excluded from rare unlocks).");
            return;
        }

        if (!lookup.ContainsKey(disease))
            lookup[disease] = 0;

        lookup[disease]++;
        SyncListFromLookup();

        Debug.Log($"[DiagnosisCounter] {disease.diseaseName} count -> {lookup[disease]}.");
    }

    private void SyncListFromLookup()
    {
        // Keep the serialized list in sync so it shows up in the Inspector
        counts.Clear();
        foreach (var kvp in lookup)
        {
            counts.Add(new DiseaseCount
            {
                disease = kvp.Key,
                correctDiagnoses = kvp.Value
            });
        }
    }

    [ContextMenu("Print Counts")]
    public void PrintCounts()
    {
        Debug.Log("[DiagnosisCounter] Current counts:");
        foreach (var kvp in lookup)
            Debug.Log($"  - {kvp.Key.diseaseName}: {kvp.Value}");
    }
}