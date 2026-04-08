using System;
using UnityEngine;

[Serializable]
public class PatientRecordData
{
    [Header("Always Visible")]
    public string patientName;

    [Header("Unlocked Fields")]
    public bool ageUnlocked;
    public int age;

    public bool professionUnlocked;
    public string professionName;

    public bool personalityUnlocked;
    public string personalityName;

    public bool socialTraitUnlocked;
    public string socialTraitName;

    public bool skillTraitUnlocked;
    public string skillTraitName;

    public bool lastDiseaseUnlocked;
    public string lastDiseaseName;

    public void InitializeWithNameOnly(string name)
    {
        patientName = name;

        ageUnlocked = false;
        age = 0;

        professionUnlocked = false;
        professionName = "";

        personalityUnlocked = false;
        personalityName = "";

        socialTraitUnlocked = false;
        socialTraitName = "";

        skillTraitUnlocked = false;
        skillTraitName = "";

        lastDiseaseUnlocked = false;
        lastDiseaseName = "";
    }

    public void UnlockAge(int value)
    {
        ageUnlocked = true;
        age = value;
    }

    public void UnlockProfession(string value)
    {
        professionUnlocked = true;
        professionName = value;
    }

    public void UnlockPersonality(string value)
    {
        personalityUnlocked = true;
        personalityName = value;
    }

    public void UnlockSocialTrait(string value)
    {
        socialTraitUnlocked = true;
        socialTraitName = value;
    }

    public void UnlockSkillTrait(string value)
    {
        skillTraitUnlocked = true;
        skillTraitName = value;
    }

    public void UnlockLastDisease(string value)
    {
        lastDiseaseUnlocked = true;
        lastDiseaseName = value;
    }
}