using System;

[Serializable]
public class SecretaryEvent
{
    public enum EventType
    {
        Death,
        SickNotVisiting
    }

    public EventType type;
    public string npcName;
    public int dayRecorded;

    public SecretaryEvent(EventType type, string npcName, int dayRecorded)
    {
        this.type = type;
        this.npcName = npcName;
        this.dayRecorded = dayRecorded;
    }
}