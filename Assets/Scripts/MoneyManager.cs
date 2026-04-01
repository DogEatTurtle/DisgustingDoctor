using TMPro;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private int currentMoney = 0;

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

    private void UpdateMoneyUI()
    {
        if (moneyText != null)
            moneyText.text = $"{currentMoney}";
    }
}