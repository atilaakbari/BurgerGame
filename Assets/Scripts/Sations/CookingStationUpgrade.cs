using System;
using System.Collections.Generic;
using UnityEngine;

public class CookingStationUpgrade : MonoBehaviour
{
    [Header("شناسه‌ی یکتا (حتماً برای هر استیشن فرق کنه - مثلاً \"CookingStation_1\")")]
    [SerializeField] private string stationId;

    [Header("Models (1 Flame, 2 Flame, 3 Flame)")]
    [SerializeField] private GameObject[] levelModels;

    [Header("Stars")]
    [SerializeField] private GameObject[] filledStars;
    [SerializeField] private GameObject[] emptyStars;

    [Header("Upgrade Effect")]
    [SerializeField] private ParticleSystem upgradeEffect;
    [SerializeField] private AudioSource upgradeSound;

    [Header("Available Indicator (نور + پارتیکل هشدار)")]
    [SerializeField] private UpgradeAvailableFX availableIndicator;

    [Header("Upgrade Button (وقتی پلیر داخل کلایدره و آپگرید موجوده فعال می‌شه)")]
    [SerializeField] private GameObject upgradeButton;

    [Header("Current Level")]
    [SerializeField] private int currentLevel = 1;

    [Header("Cooking Station (برای انتقال پتی‌ها)")]
    [SerializeField] private CookingStation cookingStation;

    public int CurrentLevel => currentLevel;
    public int MaxSlots => currentLevel;

    private bool playerInside = false;
    private bool forceUpgradeOff = false;

    public bool IsUpgradeAvailable => CanUpgrade() && !forceUpgradeOff;

    public static event Action<CookingStationUpgrade> OnStationUpgraded;

    private void Start()
    {
        // تو Start می‌خونیمش نه Awake - چون باید مطمئن باشیم SaveManager قبلش آماده شده
        if (SaveManager.Instance != null)
            currentLevel = SaveManager.Instance.GetStationLevel(stationId, currentLevel);

        ApplyLevelVisuals(false);
        RefreshUpgradeState();
    }

    private void Update()
    {
        RefreshUpgradeState();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
        RefreshUpgradeButton();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        RefreshUpgradeButton();
    }

    private void RefreshUpgradeState()
    {
        if (availableIndicator != null)
        {
            if (IsUpgradeAvailable)
                availableIndicator.Show();
            else
                availableIndicator.Hide();
        }

        RefreshUpgradeButton();
    }

    private void RefreshUpgradeButton()
    {
        if (upgradeButton != null)
            upgradeButton.SetActive(playerInside && IsUpgradeAvailable);
    }

    public bool CanUpgrade()
    {
        return currentLevel < levelModels.Length;
    }

    public void ForceDisableUpgrade()
    {
        forceUpgradeOff = true;
        RefreshUpgradeState();
    }

    public void EnableUpgradeAgain()
    {
        forceUpgradeOff = false;
        RefreshUpgradeState();
    }

    public void Upgrade()
    {
        if (!CanUpgrade() || forceUpgradeOff)
            return;

        List<CookingSlot.SlotState> savedStates = null;
        if (cookingStation != null)
            savedStates = cookingStation.CaptureAllActiveStates();

        currentLevel++;
        ApplyLevelVisuals(true);
        OnStationUpgraded?.Invoke(this);

        if (cookingStation != null && savedStates != null)
            cookingStation.RestoreStates(savedStates);

        // سیو کردن لول جدید
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetStationLevel(stationId, currentLevel);

        RefreshUpgradeState();
    }

    private void ApplyLevelVisuals(bool playEffect)
    {
        for (int i = 0; i < levelModels.Length; i++)
        {
            if (levelModels[i] != null)
                levelModels[i].SetActive(i == currentLevel - 1);
        }

        UpdateStars();

        if (playEffect)
        {
            if (upgradeEffect != null) upgradeEffect.Play();
            if (upgradeSound != null) upgradeSound.Play();
        }
    }

    private void UpdateStars()
    {
        for (int i = 0; i < 3; i++)
        {
            bool isFilled = i < currentLevel;

            if (filledStars != null && i < filledStars.Length && filledStars[i] != null)
                filledStars[i].SetActive(isFilled);

            if (emptyStars != null && i < emptyStars.Length && emptyStars[i] != null)
                emptyStars[i].SetActive(!isFilled);
        }
    }

    public void SetLevel(int level, bool playEffect = false)
    {
        currentLevel = Mathf.Clamp(level, 1, levelModels.Length);
        ApplyLevelVisuals(playEffect);

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetStationLevel(stationId, currentLevel);

        RefreshUpgradeState();
    }
}