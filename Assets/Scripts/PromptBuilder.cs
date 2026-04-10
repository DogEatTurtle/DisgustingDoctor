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
        string speakingStyle = npc.basePersonality != null ? npc.basePersonality.speakingStyleNotes : "";
        string socialHint = npc.socialTrait != null ? npc.socialTrait.llmHint : "";
        string skillHint = npc.skillTrait != null ? npc.skillTrait.llmHint : "";

        float talkativeness = npc.basePersonality != null ? npc.basePersonality.talkativeness : 0.5f;
        float directness = npc.basePersonality != null ? npc.basePersonality.directness : 0.5f;
        float cooperativeness = npc.basePersonality != null ? npc.basePersonality.cooperativeness : 0.5f;
        float dramatization = npc.basePersonality != null ? npc.basePersonality.dramatization : 0.5f;

        string symptomsBlock;
        if (npc.currentVisibleSymptoms != null && npc.currentVisibleSymptoms.Count > 0)
            symptomsBlock = "- " + string.Join("\n- ", npc.currentVisibleSymptoms);
        else
            symptomsBlock = "(none)";

        int daysSick = npc.daysSick;
        string dayDescription;
        if (daysSick <= 1)
            dayDescription = "Day 1 of the illness. The symptoms are still mild and not all of them have appeared. If asked, the patient describes them vaguely and is unsure what is wrong.";
        else if (daysSick == 2)
            dayDescription = "Day 2 of the illness. All symptoms have appeared and the patient feels them clearly. If asked, the patient describes them openly.";
        else
            dayDescription = "Day 3 of the illness. The symptoms are severe and undeniable. If asked, the patient describes them strongly and worriedly.";

        string profession = npc.profession != null ? npc.profession.professionName : "a villager";

        string system =
    $@"You are roleplaying as a villager visiting the doctor in a small village game.

About you:
- Your name is {npc.npcName}.
- You are {npc.age} years old.
- You work as a {profession}.

How you behave and speak (this is who you are, never mention these notes literally):
{speakingStyle}

Your social tendencies (never mention this label literally, just behave this way):
{socialHint}

Your personal habits and abilities (never mention this label literally, just behave this way):
{skillHint}

Your speech tendencies (do not mention these numbers, just let them shape how you talk):
- Talkativeness: {talkativeness:0.00} (0 = very brief, 1 = very wordy)
- Directness: {directness:0.00} (0 = vague and rambling, 1 = blunt and to the point)
- Cooperativeness: {cooperativeness:0.00} (0 = resistant and short, 1 = helpful and open)
- Dramatization: {dramatization:0.00} (0 = downplays things, 1 = exaggerates how bad things feel)

Current illness state (private knowledge, do not reveal unless asked about how you feel):
{dayDescription}

Symptoms you currently feel (only mention these if the doctor asks about your health, how you feel, what is wrong, or similar):
{symptomsBlock}

Conversation rules:
- ANSWER THE DOCTOR'S QUESTION DIRECTLY. Stay on the topic the doctor brought up.
- If the doctor greets you, greet back briefly. Do not start listing symptoms.
- If the doctor asks about your family, talk about family. If they ask about your work, talk about work. If they ask about your hobbies, talk about hobbies.
- Only mention symptoms if the doctor explicitly asks about your health, how you are feeling, what brought you here, or similar.
- Never volunteer symptoms in response to small talk or unrelated topics.
- Never mention any disease name. You do not know what you have.
- Only describe symptoms from the list above. Do not invent additional symptoms.
- NEVER use words like 'methodical', 'introverted', 'curious', 'hostile', 'lazy', 'reliable reporter', 'self-diagnoser', or any other personality label to describe yourself. Show who you are through HOW you speak and WHAT you talk about, never by naming traits.
- Speak like a real person from a small village, not like a character sheet.
- Keep replies short and natural, usually 1 to 3 sentences.";

        string user =
    $@"The doctor says:
{playerMessage}

Reply as the patient, staying on the topic the doctor brought up.";

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
For each category below, decide whether the patient revealed information about it in this exchange.

Categories and rules:

- age: TRUE if the patient stated or strongly implied their age (a number, decade, or life stage like 'in my fifties', 'I'm getting old'). FALSE otherwise.

- profession: TRUE if the patient mentioned their job, trade, what they do for a living, or their workplace. FALSE otherwise.

- personality: TRUE if the patient gave ANY substantive reply at all (more than just 'hi' or 'yes'). Personality is revealed simply by HOW the patient speaks, regardless of WHAT they say. Even a complaint, a greeting back with attitude, or a short emotional reaction counts. Only set FALSE if the patient said nothing meaningful (empty reply, single-word acknowledgement).

- socialTrait: TRUE if the patient mentioned anything about their family, friends, neighbours, romantic life, household, loneliness, or social activities. FALSE if they only talked about themselves alone, work, or symptoms.

- skillTrait: TRUE if the patient mentioned a hobby, craft, ability, sport, art, instrument, manual skill, or anything they like to do or are good at. FALSE if they only talked about work duties, symptoms, or social life.

- lastDisease: TRUE if the patient mentioned a previous or current illness by name or clear description (NOT vague symptoms, but an actual condition). FALSE otherwise.

Respond ONLY with a JSON object in this exact format, with no extra text, no markdown, no explanation:
{""age"":false,""profession"":false,""personality"":false,""socialTrait"":false,""skillTrait"":false,""lastDisease"":false}";

        string user =
    $@"Doctor's question:
{playerMessage}

Patient's reply:
{npcReply}

JSON:";

        return new Prompt(system, user);
    }
}