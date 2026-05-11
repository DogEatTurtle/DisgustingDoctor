using UnityEngine;

public class SecretaryInteractable : MonoBehaviour
{
    [SerializeField] private SecretaryActor secretaryActor;
    [SerializeField] private SecretaryUI secretaryUI;

    public void Interact()
    {
        if (secretaryUI == null || secretaryActor == null) return;

        if (secretaryActor.HasLeft)
        {
            Debug.Log("[Secretary] Already gone. Cannot interact.");
            return;
        }

        secretaryUI.OpenUI();
    }
}