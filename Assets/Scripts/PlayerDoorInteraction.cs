using UnityEngine;

public class PlayerDoorInteraction : MonoBehaviour
{
    [Header("Raycast")]
    public Camera playerCamera;
    public float interactDistance = 3f;
    public LayerMask interactLayer;

    [Header("UI")]
    public TransitionUIManager transitionUIManager;

    private DoorInteractable currentDoor;

    void Update()
    {
        CheckDoorInFront();

        if (currentDoor != null && Input.GetKeyDown(KeyCode.E))
        {
            transitionUIManager.OpenTransitionUI(currentDoor.currentLocation);
        }
    }

    void CheckDoorInFront()
    {
        currentDoor = null;

        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactDistance, interactLayer))
        {
            currentDoor = hit.collider.GetComponent<DoorInteractable>();
        }
    }
}