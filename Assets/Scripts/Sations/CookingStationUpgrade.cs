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

    [Header("Available Indicator (??? + ??????? ?????)")]
    [SerializeField] private UpgradeAvailableFX availableIndicator;

    [Header("Current Level")]
    [SerializeField] private int currentLevel = 1;

    public int CurrentLevel => currentLevel;
    public int MaxSlots => currentLevel;

    public static event Action<CookingStationUpgrade> OnStationUpgraded;

    private void Start()
    {
        ApplyLevelVisuals(false);
        RefreshAvailableIndicator();
    }

    // ??? ???? ???? ?????? ???? ??? ?? ???? ??? ????? ?????? ??
    private void RefreshAvailableIndicator()
    {
        if (availableIndicator == null)
            return;

        if (CanUpgrade())
            availableIndicator.Show();
        else
            availableIndicator.Hide();
    }

    public bool CanUpgrade()
    {
        return currentLevel < levelModels.Length;
    }

    public void Upgrade()
    {
        if (!CanUpgrade()) return;

        currentLevel++;
        ApplyLevelVisuals(true);
        OnStationUpgraded?.Invoke(this);

        // ??? ??? ???? ?? ???? ????? ?????? ??? ?? ???? ?? ?? ?????? ????? ???? ???? ?????? ???
        RefreshAvailableIndicator();
    }

    private void ApplyLevelVisuals(bool playEffect)
    {
        // ??? ??? ????? ?? ??? ???? ?? ???? ??
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
    }
}