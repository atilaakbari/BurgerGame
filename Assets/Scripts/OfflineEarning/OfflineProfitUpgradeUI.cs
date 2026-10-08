using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OfflineProfitUpgradeUI : MonoBehaviour
{
    [Header("Button")]
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color notEnoughMoneyColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("Progress squares")]
    [SerializeField] private GameObject[] emptySquares;
    [SerializeField] private GameObject[] filledSquares;

    private bool playerInside;
    private Coroutine flashRoutine;

    private void Start()
    {
        if (upgradeButton != null)
            upgradeButton.SetActive(false);

        RefreshSquares();
        RefreshCost();
    }

    private void Update()
    {
        RefreshButtonVisibility();
        RefreshCost();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }

    private void RefreshButtonVisibility()
    {
        if (upgradeButton == null)
            return;

        bool canUpgrade = OfflineEarningsManager.Instance != null
                          && OfflineEarningsManager.Instance.CanUpgrade;

        // مثل Carry: فقط داخل استیشن + هنوز جا برای آپگرید
        upgradeButton.SetActive(playerInside && canUpgrade);
    }

    private void RefreshCost()
    {
        if (costText == null)
            return;

        if (OfflineEarningsManager.Instance == null)
        {
            costText.text = "";
            return;
        }

        if (!OfflineEarningsManager.Instance.CanUpgrade)
        {
            costText.text = "MAX";
            return;
        }

        // فقط عدد — برای دوزبانه بعدی
        costText.text = OfflineEarningsManager.Instance.NextUpgradeCost.ToString("N0");
    }

    private void RefreshSquares()
    {
        int filled = 0;

        if (OfflineEarningsManager.Instance != null)
            filled = OfflineEarningsManager.Instance.CurrentLevel - 1;

        int max = 0;
        if (emptySquares != null) max = emptySquares.Length;
        if (filledSquares != null) max = Mathf.Min(max, filledSquares.Length);

        filled = Mathf.Clamp(filled, 0, max);

        for (int i = 0; i < max; i++)
        {
            bool isFilled = i < filled;

            if (filledSquares != null && i < filledSquares.Length && filledSquares[i] != null)
                filledSquares[i].SetActive(isFilled);

            if (emptySquares != null && i < emptySquares.Length && emptySquares[i] != null)
                emptySquares[i].SetActive(!isFilled);
        }
    }

    /// <summary>OnClick دکمه</summary>
    public void OnClickUpgrade()
    {
        if (!playerInside)
            return;

        if (OfflineEarningsManager.Instance == null)
            return;

        if (!OfflineEarningsManager.Instance.CanUpgrade)
            return;

        int cost = OfflineEarningsManager.Instance.NextUpgradeCost;

        if (MoneyManager.Instance == null || !MoneyManager.Instance.TrySpend(cost))
        {
            FlashNotEnoughMoney();
            return;
        }

        // پول را خودمان کم کردیم؛ Manager فقط سطح را بالا ببرد
        if (!OfflineEarningsManager.Instance.TryUpgradePaid())
            return;

        RefreshSquares();
        RefreshCost();
        RefreshButtonVisibility();
    }

    private void FlashNotEnoughMoney()
    {
        if (upgradeButtonImage == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        upgradeButtonImage.color = notEnoughMoneyColor;
        yield return new WaitForSeconds(flashDuration);
        upgradeButtonImage.color = normalButtonColor;
        flashRoutine = null;
    }
}