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

    [Header("Available Professions")]
    [SerializeField] private List<ProfessionSO> professions = new();

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

        if (basePersonalities.Count == 0 || socialTraits.Count == 0 || skillTraits.Count == 0 || professions.Count == 0)
        {
            Debug.LogWarning("One or more data lists are empty.");
            return;
        }

        foreach (var npc in sceneNPCs)
        {
            if (npc == null) continue;

            int randomAge = Random.Range(minAge, maxAge + 1);

            PersonalitySO randomPersonality = basePersonalities[Random.Range(0, basePersonalities.Count)];
            PersonalityTraitSO randomSocialTrait = socialTraits[Random.Range(0, socialTraits.Count)];
            PersonalityTraitSO randomSkillTrait = skillTraits[Random.Range(0, skillTraits.Count)];
            ProfessionSO randomProfession = professions[Random.Range(0, professions.Count)];

            npc.age = randomAge;
            npc.basePersonality = randomPersonality;
            npc.socialTrait = randomSocialTrait;
            npc.skillTrait = randomSkillTrait;
            npc.profession = randomProfession;

            npc.isAlive = true;
            npc.isSick = false;
            npc.currentDisease = null;
            npc.daysSick = 0;
            npc.daysImmune = 0;
            npc.currentVisibleSymptoms.Clear();
            npc.willVisitClinic = false;
            npc.trustInDoctor = 0.5f;

            npc.patientRecord.InitializeWithNameOnly(npc.npcName);

            Debug.Log(
                $"NPC Initialized -> Name: {npc.npcName} | Age: {npc.age} | " +
                $"Profession: {(npc.profession ? npc.profession.professionName : "None")} | " +
                $"Base Personality: {(npc.basePersonality ? npc.basePersonality.profileName : "None")} | " +
                $"Social Trait: {(npc.socialTrait ? npc.socialTrait.traitName : "None")} | " +
                $"Skill Trait: {(npc.skillTrait ? npc.skillTrait.traitName : "None")}"
            );
        }
    }
}