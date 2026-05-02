using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class DiagnosisUI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ConsultationManager consultationManager;
    [SerializeField] private ConversationManager conversationManager;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;
    [SerializeField] private MoneyManager moneyManager;
    [SerializeField] private ActiveVirusManager activeVirusManager;
    [SerializeField] private DiagnosisCounter diagnosisCounter;
    [SerializeField] private VirusLabUI virusLabUI;

    [Header("Gameplay UI")]
    [SerializeField] private GameObject notebookPanel;

    [Header("Optional Feedback")]
    [SerializeField] private TextMeshProUGUI feedbackText;
    [SerializeField] private DiseaseButtonSelectionUI selectionUI;

    [Header("Economy")]
    [SerializeField] private int rewardOnCorrectDiagnosis = 20;
    [SerializeField] private int playerVirusBonus = 30;
    [SerializeField] private float lethalityRewardMultiplier = 400f;

    [Header("Trust")]
    [SerializeField] private float trustGainOnCorrect = 0.10f;
    [SerializeField] private float trustLossOnWrong = -0.15f;

    [Header("Audio")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip correctSfx;
    [SerializeField] private AudioClip wrongSfx;

    [Header("Result Images")]
    [SerializeField] private GameObject correctImage;
    [SerializeField] private GameObject wrongImage;
    [SerializeField] private float resultImageSeconds = 2f;

    private DiseaseSO selectedDisease;
    private Coroutine resultImageRoutine;

    public bool IsOpen => notebookPanel != null && notebookPanel.activeSelf;

    private void Start()
    {
        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        if (correctImage != null)
            correctImage.SetActive(false);

        if (wrongImage != null)
            wrongImage.SetActive(false);
    }

    private void Update()
    {
        if (!IsOpen) return;

        if (Keyboard.current != null && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseDiagnosisUI();
        }
    }

    public void OpenDiagnosisUI()
    {
        if (consultationManager == null || !consultationManager.ConsultationActive)
        {
            SetFeedback("No active consultation.");
            return;
        }

        if (notebookPanel != null)
            notebookPanel.SetActive(true);

        if (fpsController != null)
            fpsController.enabled = false;

        if (lookInteractor != null)
            lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CloseDiagnosisUI()
    {
        if (notebookPanel != null)
            notebookPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        if (lookInteractor != null)
            lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void SelectDisease(DiseaseSO disease)
    {
        selectedDisease = disease;

        if (feedbackText != null && disease != null)
            feedbackText.text = $"Selected: {disease.diseaseName}";
    }

    public void ConfirmDiagnosis()
    {
        if (consultationManager == null || !consultationManager.ConsultationActive)
        {
            SetFeedback("No active consultation.");
            return;
        }

        NPCActor patient = consultationManager.CurrentPatient;
        if (patient == null)
        {
            SetFeedback("No current patient.");
            return;
        }

        if (selectedDisease == null)
        {
            SetFeedback("Select a disease first.");
            return;
        }

        bool correct = selectedDisease == patient.currentDisease;
        bool wasPlayerVirus = patient.infectedByPlayerVirus && !IsExternalVirus(patient);
        bool wasExternalVirus = patient.infectedByPlayerVirus && IsExternalVirus(patient);
        DiseaseSO diagnosedDisease = patient.currentDisease;

        if (patient.patientRecord != null)
        {
            string recordedDiseaseName = correct
                ? selectedDisease.diseaseName
                : $"{selectedDisease.diseaseName} (mistaken)";

            patient.patientRecord.UnlockLastDisease(recordedDiseaseName);
            Debug.Log($"[Diagnosis] Recorded last disease for {patient.npcName} -> {recordedDiseaseName}");
        }

        if (correct)
        {
            patient.AdjustTrust(trustGainOnCorrect);

            int totalReward = ComputeReward(wasPlayerVirus, wasExternalVirus);

            if (moneyManager != null)
                moneyManager.AddMoney(totalReward);

            // Register correct diagnosis for rare upgrade unlocks
            // (skips player virus / external virus because they're in the excluded list)
            if (diagnosisCounter != null && diagnosedDisease != null)
                diagnosisCounter.RegisterCorrectDiagnosis(diagnosedDisease);

            if (wasExternalVirus)
            {
                // External virus diagnosis: trust + money, but patient stays infected.
                // Only the lab cure heals the external virus.
                if (virusLabUI != null)
                    virusLabUI.NotifyUnknownVirusDiagnosed();

                SetFeedback($"Correct: Unknown Virus identified. +{totalReward} coins. Cure must be made in the lab.");
            }
            else if (wasPlayerVirus)
            {
                patient.CurePlayerVirusAndBecomeImmune();
                if (activeVirusManager != null)
                    activeVirusManager.NotifyPlayerCuredInfectedNPC(patient);

                SetFeedback($"Correct. +{totalReward} coins. Trust increased to {patient.trustInDoctor:0.00}");
            }
            else
            {
                patient.CureDisease();
                SetFeedback($"Correct. +{totalReward} coins. Trust increased to {patient.trustInDoctor:0.00}");
            }
        }
        else
        {
            patient.AdjustTrust(trustLossOnWrong);
            SetFeedback($"Incorrect. No money gained. Trust decreased to {patient.trustInDoctor:0.00}");
        }

        if (audioSource != null)
        {
            if (correct && correctSfx != null) audioSource.PlayOneShot(correctSfx);
            if (!correct && wrongSfx != null) audioSource.PlayOneShot(wrongSfx);
        }

        ShowResultImage(correct);

        if (selectionUI != null)
            selectionUI.ClearAll();

        selectedDisease = null;

        CloseDiagnosisUI();

        if (conversationManager != null && conversationManager.IsOpen)
            conversationManager.CloseConversation();

        consultationManager.EndConsultation();
    }

    private bool IsExternalVirus(NPCActor patient)
    {
        if (activeVirusManager == null) return false;
        if (!activeVirusManager.HasExternalVirusActive) return false;
        if (patient == null || !patient.infectedByPlayerVirus) return false;

        // The patient is infected by a virus tracked in ActiveVirusManager.
        // If the active virus is external, this patient is infected by it.
        return true;
    }

    private int ComputeReward(bool wasPlayerVirus, bool wasExternalVirus)
    {
        int reward = rewardOnCorrectDiagnosis;

        // External virus: only base reward (20 coins). The big reward comes from the cure.
        if (wasExternalVirus)
            return reward;

        // Player virus: base + bonus + lethality multiplier
        if (wasPlayerVirus && activeVirusManager != null && activeVirusManager.HasActiveVirus)
        {
            int lethalityBonus = Mathf.RoundToInt(activeVirusManager.CurrentVirus.lethalityPerDay * lethalityRewardMultiplier);
            reward += playerVirusBonus + lethalityBonus;

            Debug.Log(
                $"[Diagnosis] Player virus reward: {rewardOnCorrectDiagnosis} base + {playerVirusBonus} virus bonus + " +
                $"{lethalityBonus} lethality bonus = {reward} coins."
            );
        }

        return reward;
    }

    private void ShowResultImage(bool correct)
    {
        if (resultImageRoutine != null)
        {
            StopCoroutine(resultImageRoutine);
            resultImageRoutine = null;
        }

        resultImageRoutine = StartCoroutine(ResultImageRoutine(correct));
    }

    private IEnumerator ResultImageRoutine(bool correct)
    {
        if (correctImage != null) correctImage.SetActive(false);
        if (wrongImage != null) wrongImage.SetActive(false);

        GameObject go = correct ? correctImage : wrongImage;

        if (go != null)
            go.SetActive(true);

        yield return new WaitForSecondsRealtime(resultImageSeconds);

        if (go != null)
            go.SetActive(false);

        resultImageRoutine = null;
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}