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
    }

    private void Start()
    {
        // تو Start می‌خونیمش، نه Awake - چون باید مطمئن باشیم SaveManager
        // (که تو Awake خودش فایل رو از دیسک می‌خونه) قبلش کامل آماده شده
        if (SaveManager.Instance != null)
            money = SaveManager.Instance.Data.money;

        OnMoneyChanged?.Invoke(money);
    }

    public void AddMoney(int amount)
    {
        if (amount == 0)
            return;

        money += amount;
        OnMoneyChanged?.Invoke(money);

        SyncToSave();
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || money < amount)
            return false;

        money -= amount;
        OnMoneyChanged?.Invoke(money);

        SyncToSave();

        return true;
    }

    private void SyncToSave()
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.Data.money = money;
        SaveManager.Instance.RequestSave();
    }
}