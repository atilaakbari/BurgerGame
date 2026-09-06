using System;
using UnityEngine;

public class XPManager : MonoBehaviour
{
    public static XPManager Instance { get; private set; }

    // وقتی XP عوض شد (برای آپدیت UI)
    public static event Action<int, int, int> OnXPChanged; // currentXP, neededXP, level

    // وقتی لول رفت بالا
    public static event Action<int> OnLevelUp; // newLevel

    [Header("Level Settings")]
    [SerializeField] private int startingLevel = 1;

    [Header("XP Curve (قابل تنظیم)")]
    [Tooltip("XP لازم برای رفتن از لول ۱ به ۲")]
    [SerializeField] private int baseXPForLevel2 = 50;

    [Tooltip("هر لول چقدر بیشتر از لول قبلی XP می‌خواد")]
    [SerializeField] private int xpIncreasePerLevel = 25;

    // مثال: لول ۱→۲ = 50 | ۲→۳ = 75 | ۳→۴ = 100 | ...

    private int level;
    private int currentXP;

    public int Level => level;
    public int CurrentXP => currentXP;
    public int XPNeededForNextLevel => GetXPNeededForLevel(level);

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
        if (SaveManager.Instance != null)
        {
            level = Mathf.Max(1, SaveManager.Instance.Data.playerLevel);
            currentXP = Mathf.Max(0, SaveManager.Instance.Data.currentXP);
        }
        else
        {
            level = startingLevel;
            currentXP = 0;
        }

        NotifyUI();
    }

    // =========================================================
    // API اصلی
    // =========================================================

    public void AddXP(int amount)
    {
        if (amount <= 0) return;

        currentXP += amount;
        CheckLevelUp();
        SyncToSave();
        NotifyUI();
    }

    private void CheckLevelUp()
    {
        while (currentXP >= XPNeededForNextLevel)
        {
            currentXP -= XPNeededForNextLevel;
            level++;

            Debug.Log($"Level Up! Now Level {level}");
            OnLevelUp?.Invoke(level);
        }
    }

    // فرمول: base + (level - 1) * increase
    public int GetXPNeededForLevel(int forLevel)
    {
        // forLevel = لول فعلی (مثلاً وقتی لول ۱ هستی، برای رفتن به ۲)
        return baseXPForLevel2 + (forLevel - 1) * xpIncreasePerLevel;
    }

    private void NotifyUI()
    {
        OnXPChanged?.Invoke(currentXP, XPNeededForNextLevel, level);
    }

    private void SyncToSave()
    {
        if (SaveManager.Instance == null) return;

        SaveManager.Instance.Data.playerLevel = level;
        SaveManager.Instance.Data.currentXP = currentXP;
        SaveManager.Instance.RequestSave();
    }

    // برای تست از Inspector
    [ContextMenu("Add 10 XP")]
    private void DebugAdd10XP() => AddXP(10);

    [ContextMenu("Add 100 XP")]
    private void DebugAdd100XP() => AddXP(100);
}