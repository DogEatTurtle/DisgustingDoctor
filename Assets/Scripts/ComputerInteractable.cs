using UnityEngine;

public class ComputerInteractable : MonoBehaviour
{
    [SerializeField] private ComputerUI computerUI;

    public void Interact()
    {
        if (computerUI == null)
            return;

        computerUI.OpenComputerUI();
    }
}