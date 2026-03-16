using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class NPCProfileData
{
    public string fullName;
    public int age;

    public PersonalitySO basePersonality;
    public List<PersonalityTraitSO> socialTraits;
    public List<PersonalityTraitSO> skillTraits;
}