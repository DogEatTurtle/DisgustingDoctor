using UnityEngine;

public enum VirusUpgradeRarity
{
    Common,
    Rare
}

[CreateAssetMenu(fileName = "VirusUpgrade", menuName = "Scriptable Objects/VirusUpgrade")]
public class VirusUpgradeSO : ScriptableObject
{
    [Header("Identity")]
    [Tooltip("Short label shown in the lab UI (e.g. 'Fever').")]
    public string shortName;

    [TextArea(2, 4)]
    [Tooltip("Natural sentence fed to the LLM as a symptom the patient feels (e.g. 'I'm burning up and my skin feels hot').")]
    public string llmSymptomSentence;

    [Header("Rarity & Economy")]
    public VirusUpgradeRarity rarity = VirusUpgradeRarity.Common;
    [Min(0)] public int basePrice = 10;

    [Header("Virus Modifiers")]
    [Tooltip("Daily chance added to the virus lethality, as a fraction (0.05 = +5% per day).")]
    [Range(-0.2f, 0.5f)] public float lethalityPerDay = 0f;

    [Tooltip("How many extra people this virus can infect per day.")]
    public int dailyInfectionsCap = 0;

    [Tooltip("How many extra people this virus can infect in total (including patient zero).")]
    public int totalInfectionsCap = 0;

    [Header("Rare Unlock Condition (optional)")]
    [Tooltip("If set, this upgrade only enters the black market pool after the player correctly diagnoses this disease N times.")]
    public DiseaseSO requiredDiseaseToCure;

    [Min(0)] public int curesNeededToUnlock = 3;

    public bool IsRareLocked => rarity == VirusUpgradeRarity.Rare && requiredDiseaseToCure != null;
}