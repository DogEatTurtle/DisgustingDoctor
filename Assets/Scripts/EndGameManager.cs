using UnityEngine;

public class EndGameManager : MonoBehaviour
{
    public enum EndingType
    {
        VillageExtinct,    // All NPCs dead
        Banished,          // All alive NPCs trust the doctor < threshold
        NobelPrize,        // 30 days from start, no deaths
        Vacation           // Player paid for vacation
    }

    [Header("References")]
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private EndGameUI endGameUI;

    [Header("Banished Ending")]
    [Tooltip("Trust value below which an NPC is considered to distrust the doctor.")]
    [SerializeField, Range(0f, 1f)] private float banishedTrustThreshold = 0.25f;

    [Tooltip("Minimum number of alive NPCs required to trigger the Banished ending. Below this, only Village Extinct can trigger.")]
    [SerializeField, Min(1)] private int banishedMinAliveNPCs = 5;

    [Header("Nobel Prize Ending")]
    [Tooltip("Number of days the player must survive without any death to win the Nobel Prize.")]
    [SerializeField, Min(1)] private int nobelDaysRequired = 30;

    [Header("Vacation Ending")]
    [SerializeField, Min(1)] private int vacationCost = 3000;

    [Header("State (Read Only)")]
    [SerializeField] private bool gameEnded;
    [SerializeField] private EndingType triggeredEnding;

    public bool GameEnded => gameEnded;
    public int VacationCost => vacationCost;
    public bool CanAffordVacation => moneyManager != null && moneyManager.HasEnough(vacationCost);

    // Called by DailySystem at the end of ProcessNewDay
    public void CheckConditionsForDay()
    {
        if (gameEnded) return;
        if (dailySystem == null) return;

        // Check Village Extinct first (most severe, takes priority)
        int aliveCount = 0;
        int totalNPCs = 0;
        bool anyDeath = false;
        float trustSum = 0f;
        int alivePastThreshold = 0;

        foreach (var npc in dailySystem.AllNPCs)
        {
            if (npc == null) continue;
            totalNPCs++;
            if (!npc.isAlive)
            {
                anyDeath = true;
            }
            else
            {
                aliveCount++;
                trustSum += npc.trustInDoctor;
                if (npc.trustInDoctor >= banishedTrustThreshold)
                    alivePastThreshold++;
            }
        }

        if (totalNPCs == 0) return;

        // 1) Village Extinct
        if (aliveCount == 0)
        {
            TriggerEnding(EndingType.VillageExtinct);
            return;
        }

        // 2) Banished — every alive NPC has trust below threshold
        if (aliveCount >= banishedMinAliveNPCs && alivePastThreshold == 0)
        {
            TriggerEnding(EndingType.Banished);
            return;
        }

        // 3) Nobel Prize — N days reached with zero total deaths in the village
        int currentDay = dayManager != null ? dayManager.GetCurrentDay() : 0;
        if (currentDay >= nobelDaysRequired && !anyDeath)
        {
            TriggerEnding(EndingType.NobelPrize);
            return;
        }
    }

    // Called by VacationInteractable (or anywhere external)
    public bool TryTriggerVacation()
    {
        if (gameEnded) return false;
        if (moneyManager == null) return false;
        if (!moneyManager.HasEnough(vacationCost))
        {
            Debug.Log($"[EndGame] Cannot afford vacation: need {vacationCost}.");
            return false;
        }

        moneyManager.SpendMoney(vacationCost);
        TriggerEnding(EndingType.Vacation);
        return true;
    }

    public void TriggerEnding(EndingType type)
    {
        if (gameEnded) return;

        gameEnded = true;
        triggeredEnding = type;

        Debug.Log($"[EndGame] Triggered: {type}");

        if (endGameUI != null)
            endGameUI.ShowEnding(type);
    }

    [ContextMenu("Force: Village Extinct")]
    private void DebugVillageExtinct() => TriggerEnding(EndingType.VillageExtinct);

    [ContextMenu("Force: Banished")]
    private void DebugBanished() => TriggerEnding(EndingType.Banished);

    [ContextMenu("Force: Nobel Prize")]
    private void DebugNobel() => TriggerEnding(EndingType.NobelPrize);

    [ContextMenu("Force: Vacation")]
    private void DebugVacation() => TriggerEnding(EndingType.Vacation);
}