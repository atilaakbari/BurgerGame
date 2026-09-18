using System;
using System.Collections.Generic;
using UnityEngine;

public class AchievementManager : MonoBehaviour
{
    public static AchievementManager Instance { get; private set; }

    public event Action<AchievementDefinition> OnAchievementUnlocked;

    [Header("All Achievements")]
    [SerializeField] private List<AchievementDefinition> achievements = new List<AchievementDefinition>();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // همگام‌سازی progress از سیو
        SyncProgressFromSave();
    }

    private void SyncProgressFromSave()
    {
        if (SaveManager.Instance == null) return;

        int burgers = SaveManager.Instance.GetTotalBurgersDelivered();

        foreach (var def in achievements)
        {
            if (def == null) continue;

            string key = def.id.ToString();
            int progress = Mathf.Min(burgers, def.targetValue);
            bool unlocked = SaveManager.Instance.IsAchievementUnlocked(key);

            // اگر قبلاً آنلاک نشده ولی الان به هدف رسیده
            if (!unlocked && burgers >= def.targetValue)
            {
                Unlock(def, progress);
            }
            else
            {
                SaveManager.Instance.SetAchievementState(key, unlocked, progress);
            }
        }
    }

    /// <summary>
    /// از DeliveryStation بعد از تحویل موفق صدا بزن
    /// </summary>
    public void ReportBurgerDelivered()
    {
        if (SaveManager.Instance == null) return;

        int total = SaveManager.Instance.GetTotalBurgersDelivered() + 1;
        SaveManager.Instance.SetTotalBurgersDelivered(total);

        foreach (var def in achievements)
        {
            if (def == null) continue;

            // فقط دستاوردهای شمارش برگر
            if (!IsBurgerCountAchievement(def.id))
                continue;

            string key = def.id.ToString();

            if (SaveManager.Instance.IsAchievementUnlocked(key))
            {
                // فقط progress را تا سقف هدف نگه دار
                SaveManager.Instance.SetAchievementState(key, true, def.targetValue);
                continue;
            }

            int progress = Mathf.Min(total, def.targetValue);
            SaveManager.Instance.SetAchievementState(key, false, progress);

            if (total >= def.targetValue)
                Unlock(def, progress);
        }
    }

    private bool IsBurgerCountAchievement(AchievementId id)
    {
        return id == AchievementId.FirstBurger ||
               id == AchievementId.Burgers10 ||
               id == AchievementId.Burgers50 ||
               id == AchievementId.Burgers100;
    }

    private void Unlock(AchievementDefinition def, int progress)
    {
        if (def == null) return;

        string key = def.id.ToString();

        if (SaveManager.Instance != null)
        {
            if (SaveManager.Instance.IsAchievementUnlocked(key))
                return;

            SaveManager.Instance.SetAchievementState(key, true, progress);
        }

        // پاداش
        if (def.rewardMoney > 0 && MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(def.rewardMoney);

        if (def.rewardXP > 0 && XPManager.Instance != null)
            XPManager.Instance.AddXP(def.rewardXP);

        Debug.Log("Achievement Unlocked: " + def.title);
        OnAchievementUnlocked?.Invoke(def);
    }

    public AchievementDefinition GetDefinition(AchievementId id)
    {
        foreach (var def in achievements)
        {
            if (def != null && def.id == id)
                return def;
        }
        return null;
    }

    public IReadOnlyList<AchievementDefinition> GetAll()
    {
        return achievements;
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
}