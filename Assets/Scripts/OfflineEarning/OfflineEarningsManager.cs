using System;
using UnityEngine;

public class OfflineEarningsManager : MonoBehaviour
{
    public static OfflineEarningsManager Instance { get; private set; }

    [Header("Time rules")]
    [Tooltip("حداقل چند ساعت باید گذشته باشد تا پول حساب شود")]
    [SerializeField] private float minHours = 1f;

    [Tooltip("سقف ساعت‌های مؤثر")]
    [SerializeField] private float maxHours = 6f;

    [Header("Profit per level (سکه در ساعت)")]
    [SerializeField] private int[] profitPerHourByLevel =
    {
        50,   // level 1
        80,   // 2
        120,  // 3
        180,  // 4
        250,  // 5
        350,  // 6
        500   // 7
    };

    [Header("Upgrade cost (ایندکس 0 = هزینه رفتن از 1 به 2)")]
    [SerializeField] private int[] upgradeCosts =
    {
        100, 250, 500, 1000, 2000, 4000
    };

    [Header("UI")]
    [SerializeField] private OfflineEarningsPopup popup;

    [SerializeField] private bool logDebug = true;

    private bool checkedThisSession;

    public int CurrentLevel
    {
        get
        {
            if (SaveManager.Instance == null) return 1;
            return SaveManager.Instance.GetOfflineProfitLevel();
        }
    }

    public int MaxLevel =>
        profitPerHourByLevel == null ? 1 : profitPerHourByLevel.Length;

    public int ProfitPerHour
    {
        get
        {
            int level = CurrentLevel;
            if (profitPerHourByLevel == null || profitPerHourByLevel.Length == 0)
                return 0;

            int index = Mathf.Clamp(level - 1, 0, profitPerHourByLevel.Length - 1);
            return profitPerHourByLevel[index];
        }
    }

    public bool CanUpgrade => CurrentLevel < MaxLevel;

    public int NextUpgradeCost
    {
        get
        {
            if (!CanUpgrade || upgradeCosts == null || upgradeCosts.Length == 0)
                return 0;

            int index = CurrentLevel - 1; // از 1→2 = ایندکس 0
            if (index < 0 || index >= upgradeCosts.Length)
                return 0;

            return upgradeCosts[index];
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
        // اگر Daily امروز باز می‌شود، Offline را خود Daily بعد از Close صدا می‌زند
        if (DailyRewardManager.Instance != null && DailyRewardManager.Instance.CanClaimToday)
            return;

        Invoke(nameof(CheckOfflineEarnings), 0.2f);
    }

    private void OnApplicationPause(bool pause)
    {
        if (pause)
            SaveLogoutTime();
        else
            CheckOfflineEarnings();
    }

    private void OnApplicationQuit()
    {
        SaveLogoutTime();
    }

    // بعضی ادیتورها Quit را درست نمی‌فرستند
    private void OnDestroy()
    {
        if (Instance == this)
            SaveLogoutTime();
    }

    // =========================================================
    // LOGOUT
    // =========================================================

    public void SaveLogoutTime()
    {
        if (SaveManager.Instance == null)
            return;

        string utc = DateTime.UtcNow.ToString("o");
        SaveManager.Instance.SetLastLogoutUtc(utc);

        if (logDebug)
            Debug.Log("[Offline] saved logout: " + utc);
    }

    // =========================================================
    // CHECK ON ENTER
    // =========================================================

    public void CheckOfflineEarnings()
    {
        if (checkedThisSession)
            return;

        if (SaveManager.Instance == null)
            return;

        string raw = SaveManager.Instance.GetLastLogoutUtc();
        if (string.IsNullOrEmpty(raw))
        {
            // اولین اجرا — فقط زمان را بنویس
            SaveLogoutTime();
            checkedThisSession = true;
            return;
        }

        if (!DateTime.TryParse(raw, null,
            System.Globalization.DateTimeStyles.RoundtripKind,
            out DateTime logoutUtc))
        {
            SaveLogoutTime();
            checkedThisSession = true;
            return;
        }

        if (logoutUtc.Kind == DateTimeKind.Unspecified)
            logoutUtc = DateTime.SpecifyKind(logoutUtc, DateTimeKind.Utc);

        TimeSpan delta = DateTime.UtcNow - logoutUtc;
        double hoursPassed = delta.TotalHours;

        if (logDebug)
            Debug.Log("[Offline] hours away=" + hoursPassed.ToString("0.00"));

        // کمتر از حداقل → بدون پول
        if (hoursPassed < minHours)
        {
            //if (popup != null)
            //    popup.ShowTooSoon(minHours - hoursPassed);

            // زمان خروج را به الان آپدیت نکن تا اگر زود برگشت باز همان مبدأ بماند؟
            // معمولاً برای سادگی الان را ذخیره می‌کنیم تا تایمر از ورود جدید شروع شود:
            SaveLogoutTime();
            checkedThisSession = true;
            return;
        }

        double effectiveHours = Math.Min(hoursPassed, maxHours);
        int perHour = ProfitPerHour;
        int reward = Mathf.Max(0, (int)Math.Floor(effectiveHours * perHour));

        checkedThisSession = true;

        // بعد از حساب، زمان را ریست کن تا دوباره همان بازه دوبار پول ندهد
        SaveLogoutTime();

        if (reward <= 0)
        {
            if (popup != null)
                popup.ShowTooSoon(0);
            return;
        }

        if (MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(reward);

        // اگر AddMoney مأموریت را هم جلو می‌برد و نمی‌خواهی:
        // از همان قفل پاداش مأموریت استفاده کن یا متد بدون mission

        if (popup != null)
            popup.ShowReward(reward, effectiveHours, perHour);
        else if (logDebug)
            Debug.Log("[Offline] reward=" + reward);
    }

    // =========================================================
    // UPGRADE
    // =========================================================

    /// <summary>بعد از اینکه UI پول را Spend کرد</summary>
    public bool TryUpgradePaid()
    {
        if (!CanUpgrade)
            return false;

        if (SaveManager.Instance == null)
            return false;

        int newLevel = CurrentLevel + 1;
        SaveManager.Instance.SetOfflineProfitLevel(newLevel);

        if (logDebug)
            Debug.Log("[Offline] level → " + newLevel + " profit/h=" + ProfitPerHour);

        return true;
    }

    [ContextMenu("Debug/Fake 2 Hours Away")]
    private void DebugFake()
    {
        if (SaveManager.Instance == null) return;
        string utc = DateTime.UtcNow.AddHours(-2).ToString("o");
        SaveManager.Instance.SetLastLogoutUtc(utc);
        checkedThisSession = false;
        CheckOfflineEarnings();
    }
}