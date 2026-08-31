using UnityEngine;
using System;

public class CookingStationUpgrade : MonoBehaviour
{
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

    public int CurrentLevel => currentLevel;
    public int MaxSlots => currentLevel;

    private bool playerInside = false;

    // --- اضافه شده ---
    private bool forceUpgradeOff = false;   // با این می‌تونی اجباری false کنی

    public bool IsUpgradeAvailable => CanUpgrade() && !forceUpgradeOff;

    public static event Action<CookingStationUpgrade> OnStationUpgraded;

    private void Start()
    {
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

    // ====================== متدهای جدید ======================

    /// <summary>
    /// آپگرید رو اجباری خاموش می‌کنه (IsUpgradeAvailable = false)
    /// </summary>
    public void ForceDisableUpgrade()
    {
        forceUpgradeOff = true;
        RefreshUpgradeState();
    }

    /// <summary>
    /// دوباره اجازه آپگرید می‌ده (به شرطی که لول هنوز جا داشته باشه)
    /// </summary>
    public void EnableUpgradeAgain()
    {
        forceUpgradeOff = false;
        RefreshUpgradeState();
    }

    // ========================================================

    public void Upgrade()
    {
        if (!CanUpgrade() || forceUpgradeOff) return;

        currentLevel++;
        ApplyLevelVisuals(true);
        OnStationUpgraded?.Invoke(this);

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
        RefreshUpgradeState();
    }

    public void nooo() 
    {
        ForceDisableUpgrade();
    }
}