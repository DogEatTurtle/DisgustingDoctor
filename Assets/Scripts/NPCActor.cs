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
        currentVisibleSymptoms.Clear();
        willVisitClinic = false;
        trustInDoctor = 0.5f;

        patientRecord.InitializeWithNameOnly(npcName);
    }

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
        currentVisibleSymptoms.Clear();
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
        daysImmune = 0;
        willVisitClinic = false;
        patientRecord.status = PatientRecordData.HealthStatus.Deceased;
    }

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