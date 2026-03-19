using System.Collections.Generic;
using UnityEngine;

public class NPCInitializer : MonoBehaviour
{
    [Header("NPCs in Scene")]
    [SerializeField] private List<NPCActor> sceneNPCs = new();

    [Header("Available Base Personalities")]
    [SerializeField] private List<PersonalitySO> basePersonalities = new();

    [Header("Available Social Traits")]
    [SerializeField] private List<PersonalityTraitSO> socialTraits = new();

    [Header("Available Skill Traits")]
    [SerializeField] private List<PersonalityTraitSO> skillTraits = new();

    [Header("Temporary Name Pool")]
    [SerializeField]
    private List<string> possibleNames = new()
    {
        "Maria",
        "João",
        "Helena",
        "Carlos",
        "Ana",
        "Rui",
        "Teresa",
        "Miguel",
        "Sofia",
        "Manuel"
    };

    [Header("Age Range")]
    [SerializeField] private int minAge = 18;
    [SerializeField] private int maxAge = 80;

    private void Awake()
    {
        AssignRandomDataToAllNPCs();
    }

    [ContextMenu("Assign Random Data To All NPCs")]
    public void AssignRandomDataToAllNPCs()
    {
        if (sceneNPCs.Count == 0)
        {
            Debug.LogWarning("No NPCs assigned in sceneNPCs.");
            return;
        }

        if (basePersonalities.Count == 0 || socialTraits.Count == 0 || skillTraits.Count == 0)
        {
            Debug.LogWarning("One or more personality/trait lists are empty.");
            return;
        }

        List<string> availableNames = new(possibleNames);

        foreach (var npc in sceneNPCs)
        {
            if (npc == null) continue;

            string randomName = GetRandomName(availableNames);
            int randomAge = Random.Range(minAge, maxAge + 1);

            PersonalitySO randomPersonality = basePersonalities[Random.Range(0, basePersonalities.Count)];
            PersonalityTraitSO randomSocialTrait = socialTraits[Random.Range(0, socialTraits.Count)];
            PersonalityTraitSO randomSkillTrait = skillTraits[Random.Range(0, skillTraits.Count)];

            npc.AssignRandomData(
                randomName,
                randomAge,
                randomPersonality,
                randomSocialTrait,
                randomSkillTrait
            );

            Debug.Log(
                $"NPC Initialized -> Name: {npc.npcName} | Age: {npc.age} | " +
                $"Base Personality: {(npc.basePersonality ? npc.basePersonality.name : "None")} | " +
                $"Social Trait: {(npc.socialTrait ? npc.socialTrait.traitName : "None")} | " +
                $"Skill Trait: {(npc.skillTrait ? npc.skillTrait.traitName : "None")}"
            );
        }
    }

    private string GetRandomName(List<string> availableNames)
    {
        if (availableNames.Count == 0)
            return $"NPC_{Random.Range(1000, 9999)}";

        int index = Random.Range(0, availableNames.Count);
        string chosenName = availableNames[index];
        availableNames.RemoveAt(index);
        return chosenName;
    }
}