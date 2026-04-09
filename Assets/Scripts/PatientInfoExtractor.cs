using System;
using System.Threading.Tasks;
using UnityEngine;

public class PatientInfoExtractor : MonoBehaviour
{
    [SerializeField] private OllamaClient ollamaClient;

    [Serializable]
    private class ExtractionResult
    {
        public bool age;
        public bool profession;
        public bool personality;
        public bool socialTrait;
        public bool skillTrait;
        public bool lastDisease;
    }

    public async Task ExtractAndUnlockAsync(NPCActor npc, string playerMessage, string npcReply)
    {
        if (npc == null || ollamaClient == null) return;
        if (string.IsNullOrWhiteSpace(npcReply)) return;

        var prompt = PromptBuilder.BuildPatientInfoExtraction(playerMessage, npcReply);
        string response = await ollamaClient.ChatOnceAsync(prompt.system, prompt.user);

        ExtractionResult result = ParseResult(response);
        if (result == null)
        {
            Debug.LogWarning($"[Extractor] Could not parse LLM response: {response}");
            return;
        }

        ApplyUnlocks(npc, result);
    }

    private ExtractionResult ParseResult(string raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        int start = raw.IndexOf('{');
        int end = raw.LastIndexOf('}');
        if (start < 0 || end < 0 || end <= start) return null;

        string json = raw.Substring(start, end - start + 1);

        try
        {
            return JsonUtility.FromJson<ExtractionResult>(json);
        }
        catch
        {
            return null;
        }
    }

    private void ApplyUnlocks(NPCActor npc, ExtractionResult result)
    {
        var record = npc.patientRecord;
        if (record == null) return;

        if (result.age && !record.ageUnlocked)
        {
            record.UnlockAge(npc.age);
            Debug.Log($"[Extractor] Unlocked age for {npc.npcName}");
        }
        if (result.profession && !record.professionUnlocked && npc.profession != null)
        {
            record.UnlockProfession(npc.profession.professionName);
            Debug.Log($"[Extractor] Unlocked profession for {npc.npcName}");
        }
        if (result.personality && !record.personalityUnlocked && npc.basePersonality != null)
        {
            record.UnlockPersonality(npc.basePersonality.profileName);
            Debug.Log($"[Extractor] Unlocked personality for {npc.npcName}");
        }
        if (result.socialTrait && !record.socialTraitUnlocked && npc.socialTrait != null)
        {
            record.UnlockSocialTrait(npc.socialTrait.traitName);
            Debug.Log($"[Extractor] Unlocked social trait for {npc.npcName}");
        }
        if (result.skillTrait && !record.skillTraitUnlocked && npc.skillTrait != null)
        {
            record.UnlockSkillTrait(npc.skillTrait.traitName);
            Debug.Log($"[Extractor] Unlocked skill trait for {npc.npcName}");
        }
        if (result.lastDisease && !record.lastDiseaseUnlocked && npc.currentDisease != null)
        {
            record.UnlockLastDisease(npc.currentDisease.diseaseName);
            Debug.Log($"[Extractor] Unlocked last disease for {npc.npcName}");
        }
    }
}