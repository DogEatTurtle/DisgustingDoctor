using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BlackMarketShopUI : MonoBehaviour
{
    [Header("Dependencies")]
    [SerializeField] private BlackMarketShopManager shopManager;
    [SerializeField] private FPSController fpsController;
    [SerializeField] private LookInteractor lookInteractor;

    [Header("Panels")]
    [SerializeField] private GameObject shopPanel;

    [Header("Card Slots (3)")]
    [SerializeField] private List<BlackMarketCardSlotUI> cardSlots = new();

    [Header("Feedback")]
    [SerializeField] private TMP_Text feedbackText;

    private bool isOpen;

    private void Start()
    {
        if (shopPanel != null)
            shopPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isOpen) return;

        if (UnityEngine.InputSystem.Keyboard.current != null &&
            UnityEngine.InputSystem.Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseShop();
        }
    }

    public void OpenShop()
    {
        if (shopManager == null) return;

        isOpen = true;

        if (shopPanel != null)
            shopPanel.SetActive(true);

        if (fpsController != null)
            fpsController.enabled = false;

        if (lookInteractor != null)
            lookInteractor.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        RefreshCards();
    }

    public void CloseShop()
    {
        isOpen = false;

        if (shopPanel != null)
            shopPanel.SetActive(false);

        if (fpsController != null)
            fpsController.enabled = true;

        if (lookInteractor != null)
            lookInteractor.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnCardBuyClicked(int cardIndex)
    {
        if (shopManager == null) return;

        var offers = shopManager.TodaysOffers;
        if (cardIndex < 0 || cardIndex >= offers.Count) return;

        var upgrade = offers[cardIndex];
        bool success = shopManager.TryBuyUpgrade(upgrade);

        if (success)
        {
            SetFeedback($"Bought {upgrade.shortName}!");
            RefreshCards();
        }
        else
        {
            SetFeedback("Cannot buy this upgrade.");
        }
    }

    private void RefreshCards()
    {
        var offers = shopManager.TodaysOffers;

        for (int i = 0; i < cardSlots.Count; i++)
        {
            if (i < offers.Count)
                cardSlots[i].Setup(offers[i], i, this);
            else
                cardSlots[i].SetEmpty();
        }
    }

    private void SetFeedback(string msg)
    {
        if (feedbackText != null)
            feedbackText.text = msg;
    }
}