using TMPro;
using UnityEngine;

public class DayManager : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text dayText;
    [SerializeField] private TransitionUIManager transitionUIManager;

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private CharacterController characterController;
    [SerializeField] private FPSController fpsController;

    [Header("Spawn")]
    [SerializeField] private Transform consultorioSpawn;

    [Header("Settings")]
    [SerializeField] private int currentDay = 1;

    private void Start()
    {
        UpdateDayUI();
    }

    public void AdvanceDay()
    {
        currentDay++;
        UpdateDayUI();

        TeleportPlayerToConsultorio();

        if (transitionUIManager != null)
            transitionUIManager.CloseTransitionUI();

        Debug.Log($"Advanced to Day {currentDay}");
    }

    private void TeleportPlayerToConsultorio()
    {
        if (player == null || consultorioSpawn == null) return;

        if (characterController != null)
            characterController.enabled = false;

        player.position = consultorioSpawn.position;
        player.rotation = consultorioSpawn.rotation;

        if (characterController != null)
            characterController.enabled = true;

        if (fpsController != null)
            fpsController.enabled = true;
    }

    private void UpdateDayUI()
    {
        if (dayText != null)
            dayText.text = $"Day {currentDay}";
    }

    public int GetCurrentDay()
    {
        return currentDay;
    }
}