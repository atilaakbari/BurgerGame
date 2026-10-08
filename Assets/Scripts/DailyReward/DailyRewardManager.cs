using System;
using UnityEngine;

[Serializable]
public class DailyRewardEntry
{
    public int money;
    public int gems; // فعلاً 0؛ بعداً پر می‌کنی
}

public class DailyRewardManager : MonoBehaviour
{
    public static DailyRewardManager Instance { get; private set; }

    [Header("7 Days (index 0 = Day 1)")]
    [SerializeField] private DailyRewardEntry[] rewards = new DailyRewardEntry[7]
    {
        new DailyRewardEntry { money = 50,  gems = 0 },
        new DailyRewardEntry { money = 75,  gems = 0 },
        new DailyRewardEntry { money = 100, gems = 0 },
        new DailyRewardEntry { money = 150, gems = 0 },
        new DailyRewardEntry { money = 200, gems = 0 },
        new DailyRewardEntry { money = 300, gems = 0 },
        new DailyRewardEntry { money = 500, gems = 0 },
    };

    [Header("UI")]
    [SerializeField] private DailyRewardUI ui;

    [SerializeField] private bool logDebug = true;

    public int CurrentDayIndex =>
        SaveManager.Instance != null ? SaveManager.Instance.GetDailyRewardDayIndex() : 0;

    public int DayNumber => CurrentDayIndex + 1; // 1..7 برای نمایش

    public bool CanClaimToday
    {
        get
        {
            if (SaveManager.Instance == null) return false;
            string today = TodayKey();
            string last = SaveManager.Instance.GetLastDailyRewardDate();
            return last != today;
        }
    }

    public DailyRewardEntry GetReward(int dayIndex)
    {
        if (rewards == null || dayIndex < 0 || dayIndex >= rewards.Length)
            return null;
        return rewards[dayIndex];
    }

    public DailyRewardEntry GetTodayReward()
    {
        return GetReward(CurrentDayIndex);
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
        if (CanClaimToday && ui != null)
            ui.Open();
        else if (ui != null)
            ui.Refresh();
    }

    public static string TodayKey()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    /// <summary>دکمه Claim</summary>
    public bool TryClaim()
    {
        if (!CanClaimToday)
        {
            if (logDebug)
                Debug.Log("[DailyReward] Already claimed today");
            return false;
        }

        if (SaveManager.Instance == null)
            return false;

        int index = CurrentDayIndex;
        DailyRewardEntry reward = GetReward(index);
        if (reward == null)
            return false;

        // پول
        if (reward.money > 0 && MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(reward.money);

        // جم — وقتی سیستم جم آماده شد:
        // if (reward.gems > 0 && GemManager.Instance != null)
        //     GemManager.Instance.AddGems(reward.gems);

        // امروز ادعا شد
        SaveManager.Instance.SetLastDailyRewardDate(TodayKey());

        // روز بعدی در زنجیره (ریست تقویمی نه؛ فقط ایندکس جلو)
        int next = index + 1;
        if (next >= 7)
            next = 0; // چرخه از اول

        SaveManager.Instance.SetDailyRewardDayIndex(next);

        if (logDebug)
            Debug.Log("[DailyReward] Claimed day " + (index + 1) +
                      " money=" + reward.money + " nextIndex=" + next);

        if (ui != null)
            ui.Refresh();

        return true;
    }

    [ContextMenu("Debug/Clear Claim Today")]
    private void DebugClearToday()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetLastDailyRewardDate("");
        if (ui != null) ui.Refresh();
    }

    [ContextMenu("Debug/Reset Chain To Day 1")]
    private void DebugResetChain()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SetDailyRewardDayIndex(0);
        SaveManager.Instance.SetLastDailyRewardDate("");
        if (ui != null) ui.Refresh();
    }
}