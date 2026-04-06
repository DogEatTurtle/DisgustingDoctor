using UnityEngine;

[CreateAssetMenu(fileName = "ProfessionSO", menuName = "Scriptable Objects/ProfessionSO")]
public class ProfessionSO : ScriptableObject
{
    public string professionName;

    [TextArea(2, 5)]
    public string description;

    [Header("Virus Transmission")]
    [Range(0.1f, 3f)] public float sameProfessionTransmissionMultiplier = 1.25f;
    [Range(0.1f, 3f)] public float generalTransmissionMultiplier = 1f;
}