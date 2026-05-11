using UnityEngine;

public class SecretaryFarewellLetterInteractable : MonoBehaviour
{
    [SerializeField] private SecretaryFarewellLetterUI letterUI;

    public void Interact()
    {
        if (letterUI == null) return;
        letterUI.OpenUI();
    }
}