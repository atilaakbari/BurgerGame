using System;
using UnityEngine;

public class MoneyPackManager : MonoBehaviour
{
    public static MoneyPackManager Instance { get; private set; }

    [Header("Pack")]
    [SerializeField] private int moneyAmount = 500;
    [SerializeField] private int gemCost = 3;

    [Header("UI")]
    [SerializeField] private MoneyPackUI ui;

    public int MoneyAmount => moneyAmount;
    public int GemCost => gemCost;

    public bool CanBuyToday
    {
        get
        {
            if (SaveManager.Instance == null) return false;
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            return SaveManager.Instance.GetLastMoneyPackDate() != today;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (ui != null)
            ui.Refresh();
    }

    public void TryBuy()
    {
        if (!CanBuyToday)
        {
            if (ui != null) ui.OnCannotBuy();
            return;
        }

        if (GemManager.Instance == null || !GemManager.Instance.TrySpend(gemCost))
        {
            if (ui != null) ui.OnNotEnoughGems();
            return;
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetLastMoneyPackDate(DateTime.Now.ToString("yyyy-MM-dd"));

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(moneyAmount);

        if (ui != null)
            ui.Refresh();
    }

    [ContextMenu("Debug/Reset Daily Pack")]
    private void DebugReset()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetLastMoneyPackDate("");
        if (ui != null) ui.Refresh();
    }
}