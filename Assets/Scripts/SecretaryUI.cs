using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class SecretaryUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private SecretaryActor secretaryActor;
    [SerializeField] private SecretaryInfo secretaryInfo;
    [SerializeField] private DailySystem dailySystem;
    [SerializeField] private DayManager dayManager;
    [SerializeField] private OllamaClient ollamaClient;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("Panel")]
    [SerializeField] private GameObject secretaryPanel;

    [Header("Question Buttons (Active mode)")]
    [SerializeField] private Button buttonRecentDeaths;
    [SerializeField] private Button buttonSickNotVisiting;
    [SerializeField] private Button buttonVillageStatus;
    [SerializeField] private Button buttonRumors;

    [Header("Farewell Day Button")]
    [Tooltip("Only visible on farewell day (replaces the normal questions).")]
    [SerializeField] private Button buttonFarewell;

    [Header("Response Area")]
    [SerializeField] private TMP_Text responseText;

    [Header("Close")]
    [SerializeField] private Button closeButton;

    private bool isOpen;
    private bool isBusy;

    private void Start()
    {
        if (secretaryPanel != null)
            secretaryPanel.SetActive(false);

        WireButtons();
    }

    private void Update()
    {
        if (!isOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
            CloseUI();
    }

    private void WireButtons()
    {
        if (buttonRecentDeaths != null)
        {
            buttonRecentDeaths.onClick.RemoveAllListeners();
            buttonRecentDeaths.onClick.AddListener(OnAskRecentDeaths);
        }
        if (buttonSickNotVisiting != null)
        {
            buttonSickNotVisiting.onClick.RemoveAllListeners();
            buttonSickNotVisiting.onClick.AddListener(OnAskSickNotVisiting);
        }
        if (buttonVillageStatus != null)
        {
            buttonVillageStatus.onClick.RemoveAllListeners();
            buttonVillageStatus.onClick.AddListener(OnAskVillageStatus);
        }
        if (buttonRumors != null)
        {
            buttonRumors.onClick.RemoveAllListeners();
            buttonRumors.onClick.AddListener(OnAskRumors);
        }
        if (buttonFarewell != null)
        {
            buttonFarewell.onClick.RemoveAllListeners();
            buttonFarewell.onClick.AddListener(OnAskFarewell);
        }
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(CloseUI);
        }
    }

    public void OpenUI()
    {
        if (secretaryActor == null) return;

        isOpen = true;

        if (secretaryPanel != null)
            secretaryPanel.SetActive(true);

        if (fpsController != null) fpsController.enabled = false;
        if (lookInteractor != null) lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // If she's on her farewell day, register the visit
        if (secretaryActor.IsOnFarewellDay)
            secretaryActor.RegisterPlayerVisitedDuringFarewell();

        ShowProperButtons();

        if (responseText != null)
            responseText.text = "";
    }

    public void CloseUI()
    {
        isOpen = false;

        if (secretaryPanel != null)
            secretaryPanel.SetActive(false);

        if (fpsController != null) fpsController.enabled = true;
        if (lookInteractor != null) lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void ShowProperButtons()
    {
        bool activeMode = secretaryActor != null && secretaryActor.IsActive;
        bool farewellMode = secretaryActor != null && secretaryActor.IsOnFarewellDay;

        if (buttonRecentDeaths != null) buttonRecentDeaths.gameObject.SetActive(activeMode);
        if (buttonSickNotVisiting != null) buttonSickNotVisiting.gameObject.SetActive(activeMode);
        if (buttonVillageStatus != null) buttonVillageStatus.gameObject.SetActive(activeMode);
        if (buttonRumors != null) buttonRumors.gameObject.SetActive(activeMode);

        if (buttonFarewell != null) buttonFarewell.gameObject.SetActive(farewellMode);
    }

    // ---------------- Question handlers ----------------

    private async void OnAskRecentDeaths()
    {
        if (!CanAsk()) return;
        SetBusy(true);

        int currentDay = dayManager != null ? dayManager.GetCurrentDay() : 0;
        var deaths = secretaryInfo != null ? secretaryInfo.GetRecentDeaths(currentDay) : new List<SecretaryEvent>();

        var prompt = PromptBuilder.BuildSecretaryAnswer_RecentDeaths(secretaryActor, deaths, currentDay);
        await SendPromptAndDisplay(prompt);

        SetBusy(false);
    }

    private async void OnAskSickNotVisiting()
    {
        if (!CanAsk()) return;
        SetBusy(true);

        int currentDay = dayManager != null ? dayManager.GetCurrentDay() : 0;
        var entries = secretaryInfo != null ? secretaryInfo.GetRecentSickNotVisiting(currentDay) : new List<SecretaryEvent>();

        var prompt = PromptBuilder.BuildSecretaryAnswer_SickNotVisiting(secretaryActor, entries, currentDay);
        await SendPromptAndDisplay(prompt);

        SetBusy(false);
    }

    private async void OnAskVillageStatus()
    {
        if (!CanAsk()) return;
        SetBusy(true);

        int alive = 0, dead = 0, sick = 0;
        if (dailySystem != null)
        {
            foreach (var npc in dailySystem.AllNPCs)
            {
                if (npc == null) continue;
                if (!npc.isAlive) dead++;
                else
                {
                    alive++;
                    if (npc.isSick) sick++;
                }
            }
        }

        var prompt = PromptBuilder.BuildSecretaryAnswer_VillageStatus(secretaryActor, alive, sick, dead);
        await SendPromptAndDisplay(prompt);

        SetBusy(false);
    }

    private async void OnAskRumors()
    {
        if (!CanAsk()) return;
        SetBusy(true);

        var praising = new List<string>();
        var complaining = new List<string>();

        if (dailySystem != null && secretaryInfo != null)
        {
            float high = secretaryInfo.TrustHighThreshold;
            float low = secretaryInfo.TrustLowThreshold;

            foreach (var npc in dailySystem.AllNPCs)
            {
                if (npc == null) continue;
                if (!npc.isAlive) continue;
                if (npc.trustInDoctor >= high) praising.Add(npc.npcName);
                else if (npc.trustInDoctor <= low) complaining.Add(npc.npcName);
            }
        }

        var prompt = PromptBuilder.BuildSecretaryAnswer_Rumors(secretaryActor, praising, complaining);
        await SendPromptAndDisplay(prompt);

        SetBusy(false);
    }

    private async void OnAskFarewell()
    {
        if (!CanAsk()) return;
        SetBusy(true);

        var prompt = PromptBuilder.BuildSecretaryAnswer_Farewell(secretaryActor);
        await SendPromptAndDisplay(prompt);

        SetBusy(false);
    }

    private bool CanAsk()
    {
        if (isBusy) return false;
        if (ollamaClient == null) return false;
        if (secretaryActor == null) return false;
        return true;
    }

    private async System.Threading.Tasks.Task SendPromptAndDisplay(PromptBuilder.Prompt prompt)
    {
        if (responseText != null) responseText.text = "...";
        string reply = await ollamaClient.ChatOnceAsync(prompt.system, prompt.user);
        if (responseText != null) responseText.text = reply;
    }

    private void SetBusy(bool b)
    {
        isBusy = b;
        if (buttonRecentDeaths != null) buttonRecentDeaths.interactable = !b;
        if (buttonSickNotVisiting != null) buttonSickNotVisiting.interactable = !b;
        if (buttonVillageStatus != null) buttonVillageStatus.interactable = !b;
        if (buttonRumors != null) buttonRumors.interactable = !b;
        if (buttonFarewell != null) buttonFarewell.interactable = !b;
    }
}