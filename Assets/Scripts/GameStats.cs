using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

public class GameStats : MonoBehaviour
{
    [Serializable]
    public class DailyEntry
    {
        public int dayNumber;
        public List<string> events = new();
    }

    [Header("Settings")]
    [SerializeField] private string runTitle = "Disgusting Doctor — Run Report";

    [Header("State (Read Only)")]
    [SerializeField] private List<DailyEntry> dailyEntries = new();

    // ---------------- Aggregated counters ----------------
    [Header("Counters (Read Only)")]
    [SerializeField] private int correctDiagnoses;
    [SerializeField] private int wrongDiagnoses;
    [SerializeField] private int deathsFromNaturalDisease;
    [SerializeField] private int deathsFromPlayerVirus;
    [SerializeField] private int deathsFromExternalVirus;
    [SerializeField] private int virusesCreatedByPlayer;
    [SerializeField] private int totalInfectedByPlayerVirus;
    [SerializeField] private int externalVirusesCured;
    [SerializeField] private int externalVirusesExtinguishedWithoutCure;
    [SerializeField] private int totalMoneyEarned;
    [SerializeField] private int totalMoneySpent;

    [Header("References")]
    [SerializeField] private DayManager dayManager;

    private DailyEntry CurrentDayEntry()
    {
        int day = dayManager != null ? dayManager.GetCurrentDay() : 0;
        var existing = dailyEntries.Find(e => e.dayNumber == day);
        if (existing != null) return existing;

        var entry = new DailyEntry { dayNumber = day };
        dailyEntries.Add(entry);
        return entry;
    }

    private void AddEvent(string text)
    {
        var entry = CurrentDayEntry();
        entry.events.Add(text);
    }

    // ---------------- Public recording methods ----------------

    public void RecordDiagnosis(string patientName, string diagnosedAs, bool correct, string actualDisease)
    {
        if (correct)
        {
            correctDiagnoses++;
            AddEvent($"Correctly diagnosed {patientName} with {diagnosedAs}.");
        }
        else
        {
            wrongDiagnoses++;
            AddEvent($"Misdiagnosed {patientName} as {diagnosedAs} (actual: {actualDisease}).");
        }
    }

    public void RecordInfoDiscovered(string patientName, List<string> categories)
    {
        if (categories == null || categories.Count == 0) return;
        string list = string.Join(", ", categories);
        AddEvent($"Discovered about {patientName}: {list}.");
    }

    public void RecordDeath(string npcName, string cause, int villagersAliveAfter)
    {
        if (cause == "PlayerVirus")
            deathsFromPlayerVirus++;
        else if (cause == "ExternalVirus")
            deathsFromExternalVirus++;
        else
            deathsFromNaturalDisease++;

        string causeText;
        switch (cause)
        {
            case "PlayerVirus": causeText = "your virus"; break;
            case "ExternalVirus": causeText = "the unknown virus"; break;
            default: causeText = cause; break;
        }

        AddEvent($"{npcName} died of {causeText}. The village now has {villagersAliveAfter} villagers.");
    }

    public void RecordPatientUntreated(int count)
    {
        if (count <= 0) return;
        AddEvent(count == 1
            ? "1 patient was left untreated."
            : $"{count} patients were left untreated.");
    }

    public void RecordPlayerVirusReleased(string patientZero, float lethalityPerDay, int dailyCap, int totalCap, List<string> symptomNames)
    {
        virusesCreatedByPlayer++;
        string symptoms = symptomNames != null && symptomNames.Count > 0
            ? string.Join(", ", symptomNames)
            : "(no symptoms)";

        AddEvent(
            $"Your Virus started spreading. " +
            $"Patient zero: {patientZero}. " +
            $"Symptoms: {symptoms}. " +
            $"Lethality/day: {lethalityPerDay * 100f:0}%, " +
            $"daily spread cap: {dailyCap}, " +
            $"total spread cap: {totalCap}."
        );
    }

    public void RecordPlayerVirusInfection(string npcName)
    {
        totalInfectedByPlayerVirus++;
        AddEvent($"{npcName} was infected by your virus.");
    }

    public void RecordExternalVirusReleased(string patientZero, float lethalityPerDay, int dailyCap, int totalCap, List<string> symptomNames)
    {
        string symptoms = symptomNames != null && symptomNames.Count > 0
            ? string.Join(", ", symptomNames)
            : "(no symptoms)";

        AddEvent(
            $"An Unknown Virus started spreading. " +
            $"Patient zero: {patientZero}. " +
            $"Symptoms: {symptoms}. " +
            $"Lethality/day: {lethalityPerDay * 100f:0}%, " +
            $"daily spread cap: {dailyCap}, " +
            $"total spread cap: {totalCap}."
        );
    }

    public void RecordExternalVirusCured(int curedCount)
    {
        externalVirusesCured++;
        AddEvent($"The Unknown Virus has been extinguished by Player Cure. Curing {curedCount} patient(s).");
    }

    public void RecordVirusExtinct(bool wasExternal)
    {
        if (wasExternal)
        {
            externalVirusesExtinguishedWithoutCure++;
            AddEvent("The Unknown Virus went extinct on its own (without being cured by the player).");
        }
        else
        {
            AddEvent("Your Virus has gone extinct.");
        }
    }

    public void RecordSecretaryEnteredFarewellDay()
    {
        AddEvent("The Secretary announced she will leave the village tomorrow.");
    }

    public void RecordSecretaryLeft(bool leftLetter)
    {
        AddEvent(leftLetter
            ? "The Secretary left the village, leaving a farewell letter on the desk."
            : "The Secretary left the village after saying goodbye in person.");
    }

    public void RecordMoneyEarned(int amount)
    {
        if (amount > 0) totalMoneyEarned += amount;
    }

    public void RecordMoneySpent(int amount)
    {
        if (amount > 0) totalMoneySpent += amount;
    }

    // ---------------- Report generation ----------------

    public string BuildReport(int aliveCount, int deadCount, float averageTrust, int currentDay, EndGameManager.EndingType ending, int finalMoney)
    {
        var sb = new StringBuilder();

        sb.AppendLine(runTitle);
        sb.AppendLine($"Generated: {DateTime.Now:yyyy-MM-dd HH:mm:ss}");
        sb.AppendLine();
        sb.AppendLine($"Ending: {EndingToReadable(ending)}");
        sb.AppendLine($"Days survived: {currentDay}");
        sb.AppendLine();

        sb.AppendLine("=== SUMMARY ===");
        sb.AppendLine($"Villagers alive: {aliveCount}");
        sb.AppendLine($"Villagers dead: {deadCount}");
        sb.AppendLine($"  - Died from natural diseases: {deathsFromNaturalDisease}");
        sb.AppendLine($"  - Died from your virus: {deathsFromPlayerVirus}");
        sb.AppendLine($"  - Died from external virus: {deathsFromExternalVirus}");
        sb.AppendLine();
        sb.AppendLine($"Correct diagnoses: {correctDiagnoses}");
        sb.AppendLine($"Wrong diagnoses: {wrongDiagnoses}");
        sb.AppendLine();
        sb.AppendLine($"Your viruses created: {virusesCreatedByPlayer}");
        sb.AppendLine($"  - Total patients infected by your viruses: {totalInfectedByPlayerVirus}");
        sb.AppendLine($"Unknown viruses cured by you: {externalVirusesCured}");
        sb.AppendLine($"Unknown viruses that went extinct without cure: {externalVirusesExtinguishedWithoutCure}");
        sb.AppendLine();
        sb.AppendLine($"Final money: {finalMoney} coins");
        sb.AppendLine($"Total money earned: {totalMoneyEarned} coins");
        sb.AppendLine($"Total money spent: {totalMoneySpent} coins");
        sb.AppendLine($"Final average trust: {averageTrust:0.00}");
        sb.AppendLine();
        sb.AppendLine("=== TIMELINE ===");

        // Sort entries by day
        dailyEntries.Sort((a, b) => a.dayNumber.CompareTo(b.dayNumber));

        foreach (var entry in dailyEntries)
        {
            if (entry.events.Count == 0) continue;
            sb.AppendLine();
            sb.AppendLine($"Day {entry.dayNumber}");
            foreach (var ev in entry.events)
                sb.AppendLine($"  - {ev}");
        }

        return sb.ToString();
    }

    private string EndingToReadable(EndGameManager.EndingType ending)
    {
        switch (ending)
        {
            case EndGameManager.EndingType.VillageExtinct: return "Village Extinct";
            case EndGameManager.EndingType.Banished: return "Banished from the Village";
            case EndGameManager.EndingType.NobelPrize: return "Nobel Prize in Medicine";
            case EndGameManager.EndingType.Vacation: return "Took a Vacation";
            default: return ending.ToString();
        }
    }

    public string ExportToFile(string content)
    {
        string folder = Application.persistentDataPath;
        string timestamp = DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss");
        string filename = $"disgusting_doctor_run_{timestamp}.txt";
        string fullPath = Path.Combine(folder, filename);

        try
        {
            File.WriteAllText(fullPath, content);
            Debug.Log($"[GameStats] Report exported to: {fullPath}");
            return fullPath;
        }
        catch (Exception ex)
        {
            Debug.LogError($"[GameStats] Failed to export report: {ex.Message}");
            return null;
        }
    }
}
