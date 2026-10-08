using System.Collections;
using TMPro;
using UnityEngine;

public class OfflineEarningsPopup : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI valueHours;
    [SerializeField] private TextMeshProUGUI valueProfit;
    [SerializeField] private TextMeshProUGUI valueMoney;
    [SerializeField] private float autoCloseSeconds = 4f;

    private Coroutine closeRoutine;

    private void Awake()
    {
        if (root != null)
            root.SetActive(false);
    }

    public void ShowReward(int money, double hours, int perHour)
    {
        if (money <= 0) return;

        if (root != null)
            root.SetActive(true);

        if (valueHours != null)
            valueHours.text = hours.ToString("0.0");

        if (valueProfit != null)
            valueProfit.text = perHour.ToString();

        if (valueMoney != null)
            valueMoney.text = "+" + money;

        RestartAutoClose();
    }

    /// <summary>اگر کمتر از حداقل زمان گذشته — هیچ پاپ‌آپی نشان نده</summary>
    public void ShowTooSoon(double hoursRemaining)
    {
        // عمداً خالی: پاپ‌آپ نیاید
        Close();
    }

    public void Close()
    {
        if (closeRoutine != null)
        {
            StopCoroutine(closeRoutine);
            closeRoutine = null;
        }

        if (root != null)
            root.SetActive(false);
    }

    private void RestartAutoClose()
    {
        if (closeRoutine != null)
            StopCoroutine(closeRoutine);

        closeRoutine = StartCoroutine(AutoClose());
    }

    private IEnumerator AutoClose()
    {
        yield return new WaitForSecondsRealtime(autoCloseSeconds);
        closeRoutine = null;
        Close();
    }
}