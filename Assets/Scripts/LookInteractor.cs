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

    void Start()
    {
        if (hoverPromptText != null)
            hoverPromptText.gameObject.SetActive(false);
    }

    void Update()
    {
        DetectObject();

        if (currentDoor != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            currentDoor.Interact();
        }
    }

    void DetectObject()
    {
        Vector3 origin = rayOrigin.position;
        Vector3 direction = rayOrigin.forward;

        RaycastHit hit;

        Highlightable foundHighlight = null;
        DoorInteractable foundDoor = null;

        if (Physics.Raycast(origin, direction, out hit, maxDistance, interactLayers))
        {
            foundHighlight = hit.collider.GetComponentInParent<Highlightable>();
            foundDoor = hit.collider.GetComponentInParent<DoorInteractable>();
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

        if (hoverPromptText != null)
        {
            if (currentDoor != null)
            {
                if (hoverPromptText != null)
                {
                    hoverPromptText.gameObject.SetActive(currentDoor != null);
                }
            }
            else
            {
                hoverPromptText.gameObject.SetActive(false);
            }
        }
    }
}