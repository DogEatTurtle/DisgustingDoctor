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
    public DiseaseSO currentDisease;

    public void AssignRandomData(
        string newName,
        int newAge,
        PersonalitySO personality,
        PersonalityTraitSO social,
        PersonalityTraitSO skill,
        DiseaseSO disease)
    {
        npcName = newName;
        age = newAge;
        basePersonality = personality;
        socialTrait = social;
        skillTrait = skill;
        currentDisease = disease;
    }

    [ContextMenu("Print NPC Info")]
    public void PrintInfo()
    {
        Debug.Log(
            $"NPC: {npcName} | Age: {age} | Personality: {(basePersonality ? basePersonality.name : "None")} | " +
            $"Social Trait: {(socialTrait ? socialTrait.traitName : "None")} | " +
            $"Skill Trait: {(skillTrait ? skillTrait.traitName : "None")} | " +
            $"Disease: {(currentDisease ? currentDisease.name : "None")}",
            this
        );
    }
}