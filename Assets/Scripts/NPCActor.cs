using System.Collections.Generic;
using UnityEngine;

public class NPCActor : MonoBehaviour
{
    [Header("Identity")]
    public string npcName;
    public int age;

    [Header("Assigned Data")]
    public PersonalitySO basePersonality;
    public PersonalityTraitSO socialTrait;
    public PersonalityTraitSO skillTrait;

    [Header("Health")]
    public bool isAlive = true;
    public bool isSick;
    public DiseaseSO currentDisease;
    public int daysSick;
    public int daysImmune;
    public List<string> currentVisibleSymptoms = new();

    [Header("Player Virus State")]
    public bool infectedByPlayerVirus;
    public bool immuneToCurrentPlayerVirus;

    // The full combined symptom list of the active virus this NPC has,
    // stored so we can reconstruct currentVisibleSymptoms each day based on
    // revealedSymptomIndices.
    [SerializeField] private List<string> virusFullSymptomList = new();

    // Indices into virusFullSymptomList that this NPC currently reveals via LLM.
    [SerializeField] private List<int> revealedSymptomIndices = new();

    [Header("Background")]
    public ProfessionSO profession;

    [Header("Daily State")]
    public bool willVisitClinic;

    [Header("Original Position")]
    public Vector3 originalPosition;
    public Quaternion originalRotation;

    [Header("Trust")]
    [Range(0f, 1f)] public float trustInDoctor = 0.5f;

    [Header("Patient Record")]
    public PatientRecordData patientRecord = new PatientRecordData();

    private const int symptomsOnDayOne = 3;

    private void Awake()
    {
        originalPosition = transform.position;
        originalRotation = transform.rotation;
    }

    public void ReturnToOriginalPosition()
    {
        transform.position = originalPosition;
        transform.rotation = originalRotation;
    }

    public void AssignRandomData(
        string newName,
        int newAge,
        PersonalitySO personality,
        PersonalityTraitSO social,
        PersonalityTraitSO skill)
    {
        npcName = newName;
        age = newAge;
        basePersonality = personality;
        socialTrait = social;
        skillTrait = skill;

        isAlive = true;
        isSick = false;
        currentDisease = null;
        daysSick = 0;
        daysImmune = 0;
        infectedByPlayerVirus = false;
        immuneToCurrentPlayerVirus = false;
        currentVisibleSymptoms.Clear();
        virusFullSymptomList.Clear();
        revealedSymptomIndices.Clear();
        willVisitClinic = false;
        trustInDoctor = 0.5f;

        patientRecord.InitializeWithNameOnly(npcName);
    }

    // ---------------- Normal disease ----------------

    public void CatchDisease(DiseaseSO disease)
    {
        if (!isAlive || disease == null) return;

        currentDisease = disease;
        isSick = true;
        daysSick = 1;
        patientRecord.status = PatientRecordData.HealthStatus.Sick;

        BuildVisibleSymptomsForDayOne();
    }

    public void CureDisease()
    {
        currentDisease = null;
        isSick = false;
        daysSick = 0;
        infectedByPlayerVirus = false;
        currentVisibleSymptoms.Clear();
        virusFullSymptomList.Clear();
        revealedSymptomIndices.Clear();
        daysImmune = 1;

        if (isAlive)
            patientRecord.status = PatientRecordData.HealthStatus.Healthy;
    }

    public void AdvanceDayWithDisease()
    {
        if (!isAlive || !isSick) return;

        daysSick++;

        if (daysSick >= 2)
            BuildVisibleSymptomsAll();
    }

    public void Die()
    {
        isAlive = false;
        isSick = false;
        currentDisease = null;
        daysSick = 0;
        currentVisibleSymptoms.Clear();
        virusFullSymptomList.Clear();
        revealedSymptomIndices.Clear();
        daysImmune = 0;
        infectedByPlayerVirus = false;
        willVisitClinic = false;
        patientRecord.status = PatientRecordData.HealthStatus.Deceased;
    }

    // ---------------- Player / External virus ----------------

    // Called by ActiveVirusManager.InfectNPC. Receives the full symptom list
    // and the initial 2 indices to reveal. The NPC stores both, and rebuilds
    // currentVisibleSymptoms accordingly.
    public void CatchPlayerVirus(DiseaseSO virusDiseaseSO, List<string> virusSymptoms, List<int> initialRevealedIndices)
    {
        if (!isAlive || virusDiseaseSO == null) return;

        currentDisease = virusDiseaseSO;
        isSick = true;
        daysSick = 1;
        infectedByPlayerVirus = true;
        patientRecord.status = PatientRecordData.HealthStatus.Sick;

        virusFullSymptomList.Clear();
        if (virusSymptoms != null)
            virusFullSymptomList.AddRange(virusSymptoms);

        revealedSymptomIndices.Clear();
        if (initialRevealedIndices != null)
            revealedSymptomIndices.AddRange(initialRevealedIndices);

        RebuildVisibleSymptomsFromIndices();
    }

    // Called by the daily virus update (ActiveVirusManager) for each
    // currently infected NPC, after advancing daysSick. Adds one new
    // symptom index if appropriate, capped per the curve:
    //   Player virus:   day 1 -> 2, day 2 -> 3, day 3 -> 3, day 4 -> 4
    //   External virus: day 1 -> 2, day 2 -> 2, day 3 -> 3, day 4 -> 3
    public void AdvanceVirusDay(bool isExternalVirus)
    {
        if (!isAlive || !infectedByPlayerVirus) return;
        if (virusFullSymptomList == null || virusFullSymptomList.Count == 0) return;

        int targetCount = ComputeTargetRevealedCount(daysSick, isExternalVirus, virusFullSymptomList.Count);

        // Add new indices until we reach target
        while (revealedSymptomIndices.Count < targetCount)
        {
            int newIndex = PickUnusedSymptomIndex();
            if (newIndex < 0) break; // no more unused indices
            revealedSymptomIndices.Add(newIndex);
        }

        RebuildVisibleSymptomsFromIndices();
    }

    private static int ComputeTargetRevealedCount(int day, bool isExternal, int totalSymptoms)
    {
        // Curve: which day reveals how many symptoms
        // Player:   2, 3, 3, 4
        // External: 2, 2, 3, 3
        int target;
        if (isExternal)
        {
            if (day <= 2) target = 2;
            else target = 3;
        }
        else
        {
            if (day <= 1) target = 2;
            else if (day <= 3) target = 3;
            else target = 4;
        }
        return Mathf.Min(target, totalSymptoms);
    }

    private int PickUnusedSymptomIndex()
    {
        var used = new HashSet<int>(revealedSymptomIndices);
        var unused = new List<int>();
        for (int i = 0; i < virusFullSymptomList.Count; i++)
        {
            if (!used.Contains(i)) unused.Add(i);
        }
        if (unused.Count == 0) return -1;
        return unused[Random.Range(0, unused.Count)];
    }

    private void RebuildVisibleSymptomsFromIndices()
    {
        currentVisibleSymptoms.Clear();
        if (virusFullSymptomList == null) return;
        foreach (var idx in revealedSymptomIndices)
        {
            if (idx >= 0 && idx < virusFullSymptomList.Count)
                currentVisibleSymptoms.Add(virusFullSymptomList[idx]);
        }
    }

    public IReadOnlyList<int> GetRevealedSymptomIndices() => revealedSymptomIndices;

    public void CurePlayerVirusAndBecomeImmune()
    {
        currentDisease = null;
        isSick = false;
        daysSick = 0;
        infectedByPlayerVirus = false;
        immuneToCurrentPlayerVirus = true;
        currentVisibleSymptoms.Clear();
        virusFullSymptomList.Clear();
        revealedSymptomIndices.Clear();
        daysImmune = 1;

        if (isAlive)
            patientRecord.status = PatientRecordData.HealthStatus.Healthy;
    }

    public void ResetPlayerVirusImmunity()
    {
        immuneToCurrentPlayerVirus = false;
    }

    // ---------------- Normal disease symptoms ----------------

    private void BuildVisibleSymptomsForDayOne()
    {
        currentVisibleSymptoms.Clear();
        if (currentDisease == null || currentDisease.patientFriendlyFacts == null) return;

        var pool = new List<string>(currentDisease.patientFriendlyFacts);
        int count = Mathf.Min(symptomsOnDayOne, pool.Count);

        for (int i = 0; i < count; i++)
        {
            int idx = Random.Range(0, pool.Count);
            currentVisibleSymptoms.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
    }

    private void BuildVisibleSymptomsAll()
    {
        currentVisibleSymptoms.Clear();
        if (currentDisease == null || currentDisease.patientFriendlyFacts == null) return;

        currentVisibleSymptoms.AddRange(currentDisease.patientFriendlyFacts);
    }

    public void AdjustTrust(float delta)
    {
        trustInDoctor = Mathf.Clamp01(trustInDoctor + delta);
    }
}