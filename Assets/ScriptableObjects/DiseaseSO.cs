using UnityEngine;

[CreateAssetMenu(fileName = "DiseaseSO", menuName = "Scriptable Objects/DiseaseSO")]
public class DiseaseSO : ScriptableObject
{
    public string diseaseName;
    public string type; // Viral/Bacterial/Psychological/Non-infectious

    [TextArea(3, 8)]
    public string description; // texto do manual (inglês)

    [Tooltip("Max 4")]
    public string[] mainSymptoms; // até 4

    public string typicalProgression;
    public string recommendedTreatment;

    [Header("Internal (for LLM facts)")]
    [Tooltip("Leigo: frases curtas do tipo 'I feel feverish'")]
    public string[] patientFriendlyFacts; // 4-8 frases curtas para o LLM
}

