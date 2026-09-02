using System;
using System.Collections.Generic;
using UnityEngine;

public class CuttingStationUpgrade : MonoBehaviour
{
    [Header("Models (Level 1, Level 2)")]
    [SerializeField] private GameObject[] levelModels;

    [Header("Stars")]
    [SerializeField] private GameObject[] filledStars;
    [SerializeField] private GameObject[] emptyStars;

    [Header("Upgrade Effect")]
    [SerializeField] private ParticleSystem upgradeEffect;
    [SerializeField] private AudioSource upgradeSound;

    [Header("Available Indicator")]
    [SerializeField] private UpgradeAvailableFX availableIndicator;

    [Header("Upgrade Button")]
    [SerializeField] private GameObject upgradeButton;

    [Header("Current Level")]
    [SerializeField] private int currentLevel = 1;

    [Header("Cutting Station")]
    [SerializeField] private CuttingStation cuttingStation;

    public int CurrentLevel => currentLevel;
    public int MaxSlots => currentLevel;

    private bool playerInside;
    private bool forceUpgradeOff;

    public bool IsUpgradeAvailable => CanUpgrade() && !forceUpgradeOff;

    public static event Action<CuttingStationUpgrade> OnStationUpgraded;

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
        return levelModels != null && currentLevel < levelModels.Length;
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

        List<CuttingSlot.SlotState> savedStates = null;
        if (cuttingStation != null)
            savedStates = cuttingStation.CaptureAllActiveStates();

        currentLevel++;
        ApplyLevelVisuals(true);
        OnStationUpgraded?.Invoke(this);

        if (cuttingStation != null && savedStates != null)
            cuttingStation.RestoreStates(savedStates);

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
        int starCount = 2;

        for (int i = 0; i < starCount; i++)
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
}