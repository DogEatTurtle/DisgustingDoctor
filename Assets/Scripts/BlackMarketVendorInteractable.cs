using UnityEngine;

public class BlackMarketVendorInteractable : MonoBehaviour
{
    [SerializeField] private BlackMarketShopUI shopUI;

    public void Interact()
    {
        if (shopUI != null)
            shopUI.OpenShop();
    }
}