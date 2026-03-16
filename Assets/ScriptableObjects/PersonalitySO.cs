using UnityEngine;

[CreateAssetMenu(fileName = "PersonalitySO", menuName = "Scriptable Objects/PersonalitySO")]
public class PersonalitySO : ScriptableObject
{
    public string profileName;

    [Range(0f, 1f)] public float openness;
    [Range(0f, 1f)] public float conscientiousness;
    [Range(0f, 1f)] public float extraversion;
    [Range(0f, 1f)] public float agreeableness;
    [Range(0f, 1f)] public float neuroticism;

    [Header("Dialogue knobs (simple controls)")]
    [Range(0f, 1f)] public float talkativeness;   // curto vs longo
    [Range(0f, 1f)] public float directness;      // vago vs direto
    [Range(0f, 1f)] public float cooperativeness; // colaborante vs resistente
    [Range(0f, 1f)] public float dramatization;   // exagera desconforto

    [TextArea(2, 6)]
    public string speakingStyleNotes; // instruções para o LLM
}
