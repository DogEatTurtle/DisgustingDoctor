using UnityEngine;

public enum TraitCategory
{
    Social,
    Skill
}

[CreateAssetMenu(fileName = "PersonalityTrait", menuName = "Scriptable Objects/PersonalityTrait")]
public class PersonalityTraitSO : ScriptableObject
{
    public string traitName;
    public TraitCategory category;

    [TextArea(2, 6)] public string description;
    [TextArea(2, 6)] public string llmHint;

    [Header("Gameplay Modifiers")]
    public float infectionRiskMultiplier = 1f;
    public float doctorVisitChanceMultiplier = 1f;
}