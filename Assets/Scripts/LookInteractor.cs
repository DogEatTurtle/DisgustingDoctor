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
                currentSpreader != null;

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

        if (hoverPromptText != null)
            hoverPromptText.gameObject.SetActive(false);
    }
}