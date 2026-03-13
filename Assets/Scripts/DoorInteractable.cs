using UnityEngine;

public class DoorInteractable : MonoBehaviour
{
    public LocationType currentLocation;
    public TransitionUIManager transitionUIManager;

    public void Interact()
    {
        if (transitionUIManager != null)
        {
            transitionUIManager.OpenTransitionUI(currentLocation);
        }
    }
}