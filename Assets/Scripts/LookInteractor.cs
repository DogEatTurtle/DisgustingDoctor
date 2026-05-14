using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class LookInteractor : MonoBehaviour
{
    [Header("Raycast")]
    [SerializeField] private Transform rayOrigin;
    [SerializeField] private float maxDistance = 3f;
    [SerializeField] private LayerMask interactLayers;

    [Header("UI")]
    [SerializeField] private TMP_Text hoverPromptText;

    private Highlightable currentHighlightable;
    private DoorInteractable currentDoor;
    private NPCInteractable currentNPC;
    private DoctorSeatInteraction currentSeat;
    private DiagnosisInteractable currentDiagnosis;
    private ComputerInteractable currentComputer;
    private PatientRecordsComputerInteractable currentPatientRecordsComputer;
    private BlackMarketVendorInteractable currentVendor;
    private BlackMarketSpreaderInteractable currentSpreader;
    private LabBenchInteractable currentLabBench;
    private SecretaryInteractable currentSecretary;
    private SecretaryFarewellLetterInteractable currentSecretaryLetter;
    private VacationInteractable currentVacation;
    private JukeboxInteractable currentJukebox;

    private void Start()
    {
        if (hoverPromptText != null)
            hoverPromptText.gameObject.SetActive(false);
    }

    private void Update()
    {
        DetectObject();

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            if (currentDoor != null)
                currentDoor.Interact();
            else if (currentNPC != null)
                currentNPC.Interact();
            else if (currentSeat != null)
                currentSeat.Interact();
            else if (currentDiagnosis != null)
                currentDiagnosis.Interact();
            else if (currentComputer != null)
                currentComputer.Interact();
            else if (currentPatientRecordsComputer != null)
                currentPatientRecordsComputer.Interact();
            else if (currentVendor != null)
                currentVendor.Interact();
            else if (currentSpreader != null)
                currentSpreader.Interact();
            else if (currentLabBench != null)
                currentLabBench.Interact();
            else if (currentSecretary != null)
                currentSecretary.Interact();
            else if (currentSecretaryLetter != null)
                currentSecretaryLetter.Interact();
            else if (currentVacation != null)
                currentVacation.Interact();
            else if (currentJukebox != null)
                currentJukebox.Interact();
        }
    }

    private void DetectObject()
    {
        Vector3 origin = rayOrigin.position;
        Vector3 direction = rayOrigin.forward;

        Highlightable foundHighlight = null;
        DoorInteractable foundDoor = null;
        NPCInteractable foundNPC = null;
        DoctorSeatInteraction foundSeat = null;
        DiagnosisInteractable foundDiagnosis = null;
        ComputerInteractable foundComputer = null;
        PatientRecordsComputerInteractable foundPatientRecordsComputer = null;
        BlackMarketVendorInteractable foundVendor = null;
        BlackMarketSpreaderInteractable foundSpreader = null;
        LabBenchInteractable foundLabBench = null;
        SecretaryInteractable foundSecretary = null;
        SecretaryFarewellLetterInteractable foundSecretaryLetter = null;
        VacationInteractable foundVacation = null;
        JukeboxInteractable foundJukebox = null;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance, interactLayers))
        {
            foundHighlight = hit.collider.GetComponentInParent<Highlightable>();
            foundDoor = hit.collider.GetComponentInParent<DoorInteractable>();
            foundNPC = hit.collider.GetComponentInParent<NPCInteractable>();
            foundSeat = hit.collider.GetComponentInParent<DoctorSeatInteraction>();
            foundDiagnosis = hit.collider.GetComponentInParent<DiagnosisInteractable>();
            foundComputer = hit.collider.GetComponentInParent<ComputerInteractable>();
            foundPatientRecordsComputer = hit.collider.GetComponentInParent<PatientRecordsComputerInteractable>();
            foundVendor = hit.collider.GetComponentInParent<BlackMarketVendorInteractable>();
            foundSpreader = hit.collider.GetComponentInParent<BlackMarketSpreaderInteractable>();
            foundLabBench = hit.collider.GetComponentInParent<LabBenchInteractable>();
            foundSecretary = hit.collider.GetComponentInParent<SecretaryInteractable>();
            foundSecretaryLetter = hit.collider.GetComponentInParent<SecretaryFarewellLetterInteractable>();
            foundVacation = hit.collider.GetComponentInParent<VacationInteractable>();
            foundJukebox = hit.collider.GetComponentInParent<JukeboxInteractable>();
        }

        if (foundHighlight != currentHighlightable)
        {
            if (currentHighlightable != null)
                currentHighlightable.SetHighlighted(false);

            currentHighlightable = foundHighlight;

            if (currentHighlightable != null)
                currentHighlightable.SetHighlighted(true);
        }

        currentDoor = foundDoor;
        currentNPC = foundNPC;
        currentSeat = foundSeat;
        currentDiagnosis = foundDiagnosis;
        currentComputer = foundComputer;
        currentPatientRecordsComputer = foundPatientRecordsComputer;
        currentVendor = foundVendor;
        currentSpreader = foundSpreader;
        currentLabBench = foundLabBench;
        currentSecretary = foundSecretary;
        currentSecretaryLetter = foundSecretaryLetter;
        currentVacation = foundVacation;
        currentJukebox = foundJukebox;

        if (hoverPromptText != null)
        {
            bool canShowPrompt =
                currentDoor != null ||
                currentNPC != null ||
                currentSeat != null ||
                currentDiagnosis != null ||
                currentComputer != null ||
                currentPatientRecordsComputer != null ||
                currentVendor != null ||
                currentSpreader != null ||
                currentLabBench != null ||
                currentSecretary != null ||
                currentSecretaryLetter != null ||
                currentVacation != null ||
                currentJukebox != null;

            hoverPromptText.gameObject.SetActive(canShowPrompt);
        }
    }

    private void OnDisable()
    {
        if (currentHighlightable != null)
        {
            currentHighlightable.SetHighlighted(false);
            currentHighlightable = null;
        }

        currentDoor = null;
        currentNPC = null;
        currentSeat = null;
        currentDiagnosis = null;
        currentComputer = null;
        currentPatientRecordsComputer = null;
        currentVendor = null;
        currentSpreader = null;
        currentLabBench = null;
        currentSecretary = null;
        currentSecretaryLetter = null;
        currentVacation = null;
        currentJukebox = null;

        if (hoverPromptText != null)
            hoverPromptText.gameObject.SetActive(false);
    }
}