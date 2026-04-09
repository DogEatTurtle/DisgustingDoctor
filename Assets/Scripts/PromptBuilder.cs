public static class PromptBuilder
{
    public struct Prompt
    {
        public string system;
        public string user;

        public Prompt(string system, string user)
        {
            this.system = system;
            this.user = user;
        }
    }

    // ---------------------------------------------------------------
    // PATIENT ROLEPLAY
    // Used when the doctor talks to a villager during a consultation.
    // ---------------------------------------------------------------
    public static Prompt BuildPatientRoleplay(NPCActor npc, string playerMessage)
    {
        string personalityName = npc.basePersonality != null ? npc.basePersonality.profileName : "Unknown";
        string speakingStyle = npc.basePersonality != null ? npc.basePersonality.speakingStyleNotes : "";
        string socialTrait = npc.socialTrait != null ? npc.socialTrait.traitName : "None";
        string socialHint = npc.socialTrait != null ? npc.socialTrait.llmHint : "";
        string skillTrait = npc.skillTrait != null ? npc.skillTrait.traitName : "None";
        string skillHint = npc.skillTrait != null ? npc.skillTrait.llmHint : "";
        string diseaseName = npc.currentDisease != null ? npc.currentDisease.diseaseName : "None";

        float talkativeness = npc.basePersonality != null ? npc.basePersonality.talkativeness : 0.5f;
        float directness = npc.basePersonality != null ? npc.basePersonality.directness : 0.5f;
        float cooperativeness = npc.basePersonality != null ? npc.basePersonality.cooperativeness : 0.5f;
        float dramatization = npc.basePersonality != null ? npc.basePersonality.dramatization : 0.5f;

        string system =
$@"You are roleplaying as a villager speaking to the doctor during a medical consultation in a game.

Character data:
- Name: {npc.npcName}
- Age: {npc.age}
- Base personality: {personalityName}
- Speaking style: {speakingStyle}
- Social trait: {socialTrait}
- Social trait hint: {socialHint}
- Skill trait: {skillTrait}
- Skill trait hint: {skillHint}
- Current health problem: {diseaseName}
- Talkativeness: {talkativeness:0.00}
- Directness: {directness:0.00}
- Cooperativeness: {cooperativeness:0.00}
- Dramatization: {dramatization:0.00}

Rules:
- Speak like a believable patient, not like a medical textbook.
- Do not directly reveal the disease name unless it would make sense.
- Describe symptoms, sensations, recent context, and worries naturally.
- Stay consistent with the assigned disease and personality.
- Keep replies fairly short, usually 1 to 3 sentences.";

        string user =
$@"The doctor asks:
{playerMessage}

Reply as the patient.";

        return new Prompt(system, user);
    }

    // ---------------------------------------------------------------
    // PATIENT INFO EXTRACTION
    // Used after each NPC reply to detect which categories were revealed.
    // ---------------------------------------------------------------
    public static Prompt BuildPatientInfoExtraction(string playerMessage, string npcReply)
    {
        string system =
@"You are an information extractor for a medical roleplay game.
You will be given the doctor's question and the patient's reply.
Decide which of the following categories the patient REVEALED about themselves in this exchange.
A category is revealed only if the patient gave concrete information about it, not if they just hinted vaguely or refused to answer.

Categories:
- age: the patient stated or strongly implied their age (a number, decade, or life stage like 'in my fifties')
- profession: the patient mentioned their job, trade, or what they do for a living
- personality: the patient clearly revealed personality traits through what they said or how they said it (shy, anxious, outgoing, hostile, cheerful, etc.)
- socialTrait: the patient revealed something about their social life, family situation, or relationships
- skillTrait: the patient revealed an ability, hobby, craft, or skill they have
- lastDisease: the patient mentioned a previous or current illness by name or clear description

Respond ONLY with a JSON object in this exact format, with no extra text, no markdown, no explanation:
{""age"":false,""profession"":false,""personality"":false,""socialTrait"":false,""skillTrait"":false,""lastDisease"":false}
Set a field to true only if it was clearly revealed in the patient's reply.";

        string user =
$@"Doctor's question:
{playerMessage}

Patient's reply:
{npcReply}

JSON:";

        return new Prompt(system, user);
    }
}