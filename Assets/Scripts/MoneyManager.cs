using System;
using UnityEngine;

public class MoneyManager : MonoBehaviour
{
    public static MoneyManager Instance { get; private set; }
    public static event Action<int> OnMoneyChanged;

    [SerializeField] private int money;

    public int Money => money;

    private void Awake()
    {
        Instance = this;
        OnMoneyChanged?.Invoke(money);
    }

    public void AddMoney(int amount)
    {
        if (amount == 0)
            return;

        money += amount;
        OnMoneyChanged?.Invoke(money);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || money < amount)
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);
        return true;
    }
}
