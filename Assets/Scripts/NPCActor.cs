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
    public bool isSick;
    public DiseaseSO currentDisease;

    [Header("Daily State")]
    public bool willVisitClinic;

    [Header("Original Position")]
    public Vector3 originalPosition;
    public Quaternion originalRotation;

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

        // A doença passa a ser controlada apenas pelo sistema diário
        isSick = false;
        currentDisease = null;
    }

    public void SetDisease(DiseaseSO disease)
    {
        currentDisease = disease;
        isSick = disease != null;
    }

    [ContextMenu("Print NPC Info")]
    public void PrintInfo()
    {
        Debug.Log(
            $"NPC: {npcName} | Age: {age} | Personality: {(basePersonality ? basePersonality.name : "None")} | " +
            $"Social Trait: {(socialTrait ? socialTrait.traitName : "None")} | " +
            $"Skill Trait: {(skillTrait ? skillTrait.traitName : "None")} | " +
            $"Is Sick: {isSick} | Disease: {(currentDisease ? currentDisease.name : "None")}",
            this
        );
    }
}