using UnityEngine;

public class BlackMarketSpreaderInteractable : MonoBehaviour
{
    [SerializeField] private BlackMarketSpreaderUI spreaderUI;

    public void Interact()
    {
        if (spreaderUI != null)
            spreaderUI.OpenSpreaderUI();
    }
}