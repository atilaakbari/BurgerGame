using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    // id, title, desc, icon, progress, target
    public event Action<string, string, string, Sprite, int, int> OnUnlocked;

    [Serializable]
    public class AchievementInfo
    {
        public AchievementId id;
        public AchievementCategory category;
        public string title = "Achievement";
        [TextArea] public string description;
        public int targetValue = 1;
        public int rewardMoney = 0;
        public Sprite icon;
        [Tooltip("ترتیب در ردیف — کوچکتر = زودتر")]
        public int chainOrder = 0;
    }

    [SerializeField] private List<AchievementInfo> achievements = new List<AchievementInfo>();
    [SerializeField] private bool logDebug = true;

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
        EnsureDefaultAchievements();
        SyncFromSave();
    }

    private void EnsureDefaultAchievements()
    {
        if (achievements != null && achievements.Count > 0)
            return;

        achievements = new List<AchievementInfo>
        {
            // Burger
            new AchievementInfo { id = AchievementId.FirstBurger, title = "اولین برگر", description = "اولین سفارش را تحویل بده", targetValue = 1 },
            new AchievementInfo { id = AchievementId.Burgers10, title = "۱۰ برگر", description = "۱۰ سفارش تحویل بده", targetValue = 10 },
            new AchievementInfo { id = AchievementId.Burgers50, title = "۵۰ برگر", description = "۵۰ سفارش تحویل بده", targetValue = 50 },
            new AchievementInfo { id = AchievementId.Burgers100, title = "۱۰۰ برگر", description = "۱۰۰ سفارش تحویل بده", targetValue = 100 },

            // Soda
            new AchievementInfo { id = AchievementId.FirstSoda, title = "اولین نوشابه", description = "اولین نوشابه را تحویل بده", targetValue = 1 },
            new AchievementInfo { id = AchievementId.Sodas10, title = "۱۰ نوشابه", description = "۱۰ نوشابه تحویل بده", targetValue = 10 },
            new AchievementInfo { id = AchievementId.Sodas50, title = "۵۰ نوشابه", description = "۵۰ نوشابه تحویل بده", targetValue = 50 },
            new AchievementInfo { id = AchievementId.Sodas100, title = "۱۰۰ نوشابه", description = "۱۰۰ نوشابه تحویل بده", targetValue = 100 },

            // Level
            new AchievementInfo { id = AchievementId.Level5, title = "لول ۵", description = "به لول ۵ برس", targetValue = 5 },
            new AchievementInfo { id = AchievementId.Level10, title = "لول ۱۰", description = "به لول ۱۰ برس", targetValue = 10 },
            new AchievementInfo { id = AchievementId.Level20, title = "لول ۲۰", description = "به لول ۲۰ برس", targetValue = 20 },
            new AchievementInfo { id = AchievementId.Level50, title = "لول ۵۰", description = "به لول ۵۰ برس", targetValue = 50 },
            new AchievementInfo { id = AchievementId.Level75, title = "لول ۷۵", description = "به لول ۷۵ برس", targetValue = 75 },
            new AchievementInfo { id = AchievementId.Level100, title = "لول ۱۰۰", description = "به لول ۱۰۰ برس", targetValue = 100 },

            // Money earned
            new AchievementInfo { id = AchievementId.Money1000, title = "۱۰۰۰ پول", description = "در مجموع ۱۰۰۰ پول دربیاور", targetValue = 1000 },
            new AchievementInfo { id = AchievementId.Money2500, title = "۲۵۰۰ پول", description = "در مجموع ۲۵۰۰ پول دربیاور", targetValue = 2500 },
            new AchievementInfo { id = AchievementId.Money5000, title = "۵۰۰۰ پول", description = "در مجموع ۵۰۰۰ پول دربیاور", targetValue = 5000 },
            new AchievementInfo { id = AchievementId.Money10000, title = "۱۰۰۰۰ پول", description = "در مجموع ۱۰۰۰۰ پول دربیاور", targetValue = 10000 },

            // Patties
            new AchievementInfo { id = AchievementId.Patties5, title = "۵ پتی", description = "۵ پتی بپز", targetValue = 5 },
            new AchievementInfo { id = AchievementId.Patties10, title = "۱۰ پتی", description = "۱۰ پتی بپز", targetValue = 10 },
            new AchievementInfo { id = AchievementId.Patties25, title = "۲۵ پتی", description = "۲۵ پتی بپز", targetValue = 25 },
            new AchievementInfo { id = AchievementId.Patties50, title = "۵۰ پتی", description = "۵۰ پتی بپز", targetValue = 50 },
            new AchievementInfo { id = AchievementId.Patties100, title = "۱۰۰ پتی", description = "۱۰۰ پتی بپز", targetValue = 100 },
        };
    }

    private void SyncFromSave()
    {
        if (SaveManager.Instance == null) return;

        int burgers = SaveManager.Instance.GetTotalBurgersDelivered();
        int sodas = SaveManager.Instance.GetTotalSodasDelivered();
        int money = SaveManager.Instance.GetTotalMoneyEarned();
        int patties = SaveManager.Instance.GetTotalPattiesCooked();
        int level = SaveManager.Instance.Data != null ? SaveManager.Instance.Data.playerLevel : 1;

        foreach (var a in achievements)
        {
            if (a == null) continue;
            int current = GetCounterFor(a.id, burgers, sodas, money, patties, level);
            TryProgressAndUnlock(a, current, false);
        }
    }

    private int GetCounterFor(AchievementId id, int burgers, int sodas, int money, int patties, int level)
    {
        if (IsBurger(id)) return burgers;
        if (IsSoda(id)) return sodas;
        if (IsMoney(id)) return money;
        if (IsPatty(id)) return patties;
        if (IsLevel(id)) return level;
        return 0;
    }

    // ---------- Reports ----------

    public void ReportBurgerDelivered()
    {
        if (SaveManager.Instance == null) return;
        EnsureDefaultAchievements();
        int total = SaveManager.Instance.GetTotalBurgersDelivered() + 1;
        SaveManager.Instance.SetTotalBurgersDelivered(total);
        if (logDebug) Debug.Log("Burgers = " + total);
        ApplyForCategory(IsBurger, total);
    }

    public void ReportSodaDelivered()
    {
        if (SaveManager.Instance == null) return;
        EnsureDefaultAchievements();
        int total = SaveManager.Instance.GetTotalSodasDelivered() + 1;
        SaveManager.Instance.SetTotalSodasDelivered(total);
        if (logDebug) Debug.Log("Sodas = " + total);
        ApplyForCategory(IsSoda, total);
    }

    public void ReportPlayerLevel(int level)
    {
        if (SaveManager.Instance == null) return;
        EnsureDefaultAchievements();
        if (logDebug) Debug.Log("Level = " + level);
        ApplyForCategory(IsLevel, level);
    }

    /// <summary>هر بار که پول به بازیکن اضافه می‌شود (نه موجودی فعلی)</summary>
    public void ReportMoneyEarned(int amount)
    {
        if (amount <= 0 || SaveManager.Instance == null) return;
        EnsureDefaultAchievements();
        int total = SaveManager.Instance.GetTotalMoneyEarned() + amount;
        SaveManager.Instance.SetTotalMoneyEarned(total);
        if (logDebug) Debug.Log("Money earned total = " + total);
        ApplyForCategory(IsMoney, total);
    }

    public void ReportPattyCooked()
    {
        if (SaveManager.Instance == null) return;
        EnsureDefaultAchievements();
        int total = SaveManager.Instance.GetTotalPattiesCooked() + 1;
        SaveManager.Instance.SetTotalPattiesCooked(total);
        if (logDebug) Debug.Log("Patties cooked = " + total);
        ApplyForCategory(IsPatty, total);
    }

    private void ApplyForCategory(Func<AchievementId, bool> filter, int currentValue)
    {
        foreach (var a in achievements)
        {
            if (a == null || !filter(a.id)) continue;
            TryProgressAndUnlock(a, currentValue, true);
        }
    }

    private void TryProgressAndUnlock(AchievementInfo a, int currentValue, bool canUnlockNow)
    {
        string key = a.id.ToString();
        bool already = SaveManager.Instance.IsAchievementUnlocked(key);
        int progress = Mathf.Min(currentValue, a.targetValue);

        if (already)
        {
            SaveManager.Instance.SetAchievementState(key, true, a.targetValue);
            return;
        }

        SaveManager.Instance.SetAchievementState(key, false, progress);

        if (canUnlockNow && currentValue >= a.targetValue)
            Unlock(a);
        else if (!canUnlockNow && currentValue >= a.targetValue)
        {
            // retro unlock silent or with popup — اینجا با پاپ‌آپ:
            Unlock(a);
        }
    }

    private void Unlock(AchievementInfo a)
    {
        string key = a.id.ToString();
        if (SaveManager.Instance.IsAchievementUnlocked(key))
            return;

        SaveManager.Instance.SetAchievementState(key, true, a.targetValue);

        if (a.rewardMoney > 0 && MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(a.rewardMoney);

        if (logDebug) Debug.Log("ACHIEVEMENT UNLOCKED: " + a.title);

        OnUnlocked?.Invoke(a.id.ToString(), a.title, a.description, a.icon, a.targetValue, a.targetValue);
    }

    private bool IsBurger(AchievementId id) =>
        id == AchievementId.FirstBurger || id == AchievementId.Burgers10 ||
        id == AchievementId.Burgers50 || id == AchievementId.Burgers100;

    private bool IsSoda(AchievementId id) =>
        id == AchievementId.FirstSoda || id == AchievementId.Sodas10 ||
        id == AchievementId.Sodas50 || id == AchievementId.Sodas100;

    private bool IsLevel(AchievementId id) =>
        id == AchievementId.Level5 || id == AchievementId.Level10 ||
        id == AchievementId.Level20 || id == AchievementId.Level50 ||
        id == AchievementId.Level75 || id == AchievementId.Level100;

    private bool IsMoney(AchievementId id) =>
        id == AchievementId.Money1000 || id == AchievementId.Money2500 ||
        id == AchievementId.Money5000 || id == AchievementId.Money10000;

    private bool IsPatty(AchievementId id) =>
        id == AchievementId.Patties5 || id == AchievementId.Patties10 ||
        id == AchievementId.Patties25 || id == AchievementId.Patties50 ||
        id == AchievementId.Patties100;

    public int GetTotalBurgers() =>
        SaveManager.Instance != null ? SaveManager.Instance.GetTotalBurgersDelivered() : 0;


    // =========================================================
    // DEBUG — راست‌کلیک روی کامپوننت AchievementManager
    // =========================================================

    [ContextMenu("Debug/Burger +1")]
    private void DebugBurgerPlus1()
    {
        ReportBurgerDelivered();
    }

    [ContextMenu("Debug/Soda +1")]
    private void DebugSodaPlus1()
    {
        ReportSodaDelivered();
    }

    [ContextMenu("Debug/Patty +1")]
    private void DebugPattyPlus1()
    {
        ReportPattyCooked();
    }

    [ContextMenu("Debug/Money +100")]
    private void DebugMoneyPlus100()
    {
        ReportMoneyEarned(100);
    }

    [ContextMenu("Debug/Money +1000")]
    private void DebugMoneyPlus1000()
    {
        ReportMoneyEarned(1000);
    }

    [ContextMenu("Debug/Level Report Current")]
    private void DebugLevelReportCurrent()
    {
        int level = 1;
        if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
            level = SaveManager.Instance.Data.playerLevel;

        ReportPlayerLevel(level);
    }

    [ContextMenu("Debug/Level Set 5 + Report")]
    private void DebugLevel5()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
        {
            SaveManager.Instance.Data.playerLevel = 5;
            SaveManager.Instance.RequestSave();
        }
        ReportPlayerLevel(5);
    }

    [ContextMenu("Debug/Level Set 10 + Report")]
    private void DebugLevel10()
    {
        if (SaveManager.Instance != null && SaveManager.Instance.Data != null)
        {
            SaveManager.Instance.Data.playerLevel = 10;
            SaveManager.Instance.RequestSave();
        }
        ReportPlayerLevel(10);
    }

    [ContextMenu("Debug/Reset ALL Counters + Achievements")]
    private void DebugResetAll()
    {
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.SetTotalBurgersDelivered(0);
        SaveManager.Instance.SetTotalSodasDelivered(0);
        SaveManager.Instance.SetTotalPattiesCooked(0);
        SaveManager.Instance.SetTotalMoneyEarned(0);

        EnsureDefaultAchievements();

        if (achievements != null)
        {
            foreach (var a in achievements)
            {
                if (a == null) continue;
                SaveManager.Instance.SetAchievementState(a.id.ToString(), false, 0);
            }
        }

        Debug.Log("All achievement counters + unlocks reset");
    }

    [ContextMenu("Debug/Reset Burgers Only")]
    private void DebugResetBurgers()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SetTotalBurgersDelivered(0);
        ResetCategory(IsBurger);
        Debug.Log("Burgers reset");
    }

    [ContextMenu("Debug/Reset Sodas Only")]
    private void DebugResetSodas()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SetTotalSodasDelivered(0);
        ResetCategory(IsSoda);
        Debug.Log("Sodas reset");
    }

    [ContextMenu("Debug/Reset Patties Only")]
    private void DebugResetPatties()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SetTotalPattiesCooked(0);
        ResetCategory(IsPatty);
        Debug.Log("Patties reset");
    }

    [ContextMenu("Debug/Reset Money Only")]
    private void DebugResetMoney()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SetTotalMoneyEarned(0);
        ResetCategory(IsMoney);
        Debug.Log("Money earned reset");
    }

    [ContextMenu("Debug/Reset Level Achievements Only")]
    private void DebugResetLevelAchievements()
    {
        ResetCategory(IsLevel);
        Debug.Log("Level achievements reset (playerLevel unchanged)");
    }

    private void ResetCategory(System.Func<AchievementId, bool> filter)
    {
        if (SaveManager.Instance == null) return;
        EnsureDefaultAchievements();

        if (achievements == null) return;

        foreach (var a in achievements)
        {
            if (a == null || !filter(a.id)) continue;
            SaveManager.Instance.SetAchievementState(a.id.ToString(), false, 0);
        }
    }

    public List<AchievementInfo> GetByCategory(AchievementCategory category)
    {
        EnsureDefaultAchievements();
        List<AchievementInfo> list = new List<AchievementInfo>();

        foreach (var a in achievements)
        {
            if (a != null && a.category == category)
                list.Add(a);
        }

        list.Sort((x, y) => x.chainOrder.CompareTo(y.chainOrder));
        return list;
    }

    public bool IsUnlocked(AchievementId id)
    {
        if (SaveManager.Instance == null) return false;
        return SaveManager.Instance.IsAchievementUnlocked(id.ToString());
    }

    public int GetProgress(AchievementId id)
    {
        if (SaveManager.Instance == null) return 0;
        return SaveManager.Instance.GetAchievementProgress(id.ToString());
    }

    public AchievementInfo GetInfo(AchievementId id)
    {
        EnsureDefaultAchievements();
        foreach (var a in achievements)
            if (a != null && a.id == id) return a;
        return null;
    }
    
}