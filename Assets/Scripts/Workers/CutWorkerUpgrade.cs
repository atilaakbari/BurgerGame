using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CutWorkerUpgrade : MonoBehaviour
{
    [Header("Worker")]
    [SerializeField] private CutWorker worker;

    [Header("Capacity Levels")]
    [SerializeField] private int[] capacityLevels = { 1, 2, 3 };

    [Header("Speed Levels")]
    [SerializeField] private float[] speedLevels = { 2.5f, 3.5f, 4.5f };

    [Header("Costs - Capacity")]
    [SerializeField] private int[] capacityCosts = { 50, 100 };

    [Header("Costs - Speed")]
    [SerializeField] private int[] speedCosts = { 40, 80 };

    [Header("UI - پنل (با ورود به تریگر روشن می‌شود)")]
    [SerializeField] private GameObject upgradePanel;

    [Header("UI - Capacity")]
    [SerializeField] private GameObject capacityButton;
    [SerializeField] private TextMeshProUGUI capacityCostText;
    [SerializeField] private TextMeshProUGUI capacityLevelText;
    [SerializeField] private Image capacityButtonImage;

    [Header("UI - Speed")]
    [SerializeField] private GameObject speedButton;
    [SerializeField] private TextMeshProUGUI speedCostText;
    [SerializeField] private TextMeshProUGUI speedLevelText;
    [SerializeField] private Image speedButtonImage;

    [Header("Flash")]
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color notEnoughColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("Save")]
    [SerializeField] private string workerId = "CutWorker_1";

    private int capacityLevelIndex;
    private int speedLevelIndex;
    private bool playerInside;
    private Coroutine flashRoutine;

    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            capacityLevelIndex = Mathf.Clamp(
                SaveManager.Instance.GetStationLevel(workerId + "_Cap", 1) - 1,
                0, capacityLevels.Length - 1);

            speedLevelIndex = Mathf.Clamp(
                SaveManager.Instance.GetStationLevel(workerId + "_Spd", 1) - 1,
                0, speedLevels.Length - 1);
        }

        ApplyCapacity();
        ApplySpeed();

        playerInside = false;
        if (upgradePanel != null)
            upgradePanel.SetActive(false);

        RefreshUI();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = true;
        RefreshUI();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;
        playerInside = false;
        RefreshUI();
    }

    public void OnCapacityUpgradePressed()
    {
        if (!playerInside || !CanUpgradeCapacity()) return;

        int cost = GetCapacityCost();
        if (MoneyManager.Instance == null || !MoneyManager.Instance.TrySpend(cost))
        {
            FlashButton(capacityButtonImage);
            return;
        }

        capacityLevelIndex++;
        ApplyCapacity();
        SaveProgress();
        RefreshUI();
    }

    public void OnSpeedUpgradePressed()
    {
        if (!playerInside || !CanUpgradeSpeed()) return;

        int cost = GetSpeedCost();
        if (MoneyManager.Instance == null || !MoneyManager.Instance.TrySpend(cost))
        {
            FlashButton(speedButtonImage);
            return;
        }

        speedLevelIndex++;
        ApplySpeed();
        SaveProgress();
        RefreshUI();
    }

    private void ApplyCapacity()
    {
        if (worker == null) return;
        int value = capacityLevels[Mathf.Clamp(capacityLevelIndex, 0, capacityLevels.Length - 1)];
        worker.UpgradeCapacity(value);
    }

    private void ApplySpeed()
    {
        if (worker == null) return;
        float value = speedLevels[Mathf.Clamp(speedLevelIndex, 0, speedLevels.Length - 1)];
        worker.UpgradeSpeed(value);
    }

    private bool CanUpgradeCapacity() =>
        capacityLevels != null && capacityLevelIndex < capacityLevels.Length - 1;

    private bool CanUpgradeSpeed() =>
        speedLevels != null && speedLevelIndex < speedLevels.Length - 1;

    private int GetCapacityCost() =>
        capacityCosts != null && capacityLevelIndex < capacityCosts.Length
            ? capacityCosts[capacityLevelIndex] : 0;

    private int GetSpeedCost() =>
        speedCosts != null && speedLevelIndex < speedCosts.Length
            ? speedCosts[speedLevelIndex] : 0;

    private void RefreshUI()
    {
        if (upgradePanel != null)
            upgradePanel.SetActive(playerInside);

        if (!playerInside) return;

        bool canCap = CanUpgradeCapacity();
        if (capacityButton != null) capacityButton.SetActive(canCap);
        if (capacityCostText != null)
            capacityCostText.text = canCap ? GetCapacityCost().ToString("N0") : "MAX";
        if (capacityLevelText != null)
            capacityLevelText.text = "Lv " + (capacityLevelIndex + 1);

        bool canSpd = CanUpgradeSpeed();
        if (speedButton != null) speedButton.SetActive(canSpd);
        if (speedCostText != null)
            speedCostText.text = canSpd ? GetSpeedCost().ToString("N0") : "MAX";
        if (speedLevelText != null)
            speedLevelText.text = "Lv " + (speedLevelIndex + 1);
    }

    private void SaveProgress()
    {
        if (SaveManager.Instance == null) return;
        SaveManager.Instance.SetStationLevel(workerId + "_Cap", capacityLevelIndex + 1);
        SaveManager.Instance.SetStationLevel(workerId + "_Spd", speedLevelIndex + 1);
    }

    private void FlashButton(Image img)
    {
        if (img == null) return;
        if (flashRoutine != null) StopCoroutine(flashRoutine);
        flashRoutine = StartCoroutine(FlashRoutine(img));
    }

    private IEnumerator FlashRoutine(Image img)
    {
        img.color = notEnoughColor;
        yield return new WaitForSeconds(flashDuration);
        img.color = normalColor;
        flashRoutine = null;
    }
}