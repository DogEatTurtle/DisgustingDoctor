using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private int currentMoney = 0;

    [Header("Spend Sound")]
    [SerializeField] private AudioSource audioSource;
    [SerializeField] private AudioClip spendSfx;

    public int CurrentMoney => currentMoney;

    private void Start()
    {
        UpdateMoneyUI();
    }

    public void AddMoney(int amount)
    {
        currentMoney += amount;
        UpdateMoneyUI();
        Debug.Log($"Money gained: {amount} | Total money: {currentMoney}");
    }

    public bool HasEnough(int amount)
    {
        return currentMoney >= amount;
    }

    public bool SpendMoney(int amount)
    {
        if (!HasEnough(amount))
        {
            Debug.LogWarning($"Not enough money. Have: {currentMoney}, need: {amount}");
            return false;
        }

        currentMoney -= amount;
        UpdateMoneyUI();
        Debug.Log($"Money spent: {amount} | Total money: {currentMoney}");

        // Play the purchase sound on any successful spend
        if (audioSource != null && spendSfx != null)
            audioSource.PlayOneShot(spendSfx);

        return true;
    }

    public int GetCurrentMoney()
    {
        return currentMoney;
    }

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"{currentMoney}";
    }
}