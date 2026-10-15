using System;
using System.Collections;
using UnityEngine;

public class MoneyBoostManager : MonoBehaviour
{
    public static MoneyBoostManager Instance { get; private set; }

    [Header("Boost")]
    [SerializeField] private float multiplier = 2f;      // یا 1.5
    [SerializeField] private float durationSeconds = 60f;
    [SerializeField] private int gemCost = 5;

    [Header("UI")]
    [SerializeField] private MoneyBoostUI ui;

    private float timeLeft;
    private bool active;

    public bool IsActive => active && timeLeft > 0f;
    public float Multiplier => IsActive ? multiplier : 1f;
    public float TimeLeft => Mathf.Max(0f, timeLeft);
    public int GemCost => gemCost;
    public float BoostMultiplierValue => multiplier;
    public float Duration => durationSeconds;

    public bool CanBuyToday
    {
        get
        {
            if (SaveManager.Instance == null) return false;
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            return SaveManager.Instance.GetLastMoneyBoostDate() != today;
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

    private void Update()
    {
        if (!active) return;

        timeLeft -= Time.deltaTime;
        if (ui != null)
            ui.RefreshActive();

        if (timeLeft <= 0f)
        {
            active = false;
            timeLeft = 0f;
            if (ui != null)
                ui.RefreshActive();
        }
    }

    public void TryActivate()
    {
        if (!CanBuyToday)
        {
            if (ui != null) ui.OnCannotBuy();
            return;
        }

        if (active)
            return;

        if (GemManager.Instance == null || !GemManager.Instance.TrySpend(gemCost))
        {
            if (ui != null) ui.OnNotEnoughGems();
            return;
        }

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetLastMoneyBoostDate(DateTime.Now.ToString("yyyy-MM-dd"));

        active = true;
        timeLeft = durationSeconds;

        if (ui != null)
        {
            ui.Refresh();
            ui.RefreshActive();
        }
    }

    public float GetPayMultiplier()
    {
        return IsActive ? multiplier : 1f;
    }

    [ContextMenu("Debug/Reset Daily Boost")]
    private void DebugReset()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetLastMoneyBoostDate("");
        if (ui != null) ui.Refresh();
    }
}