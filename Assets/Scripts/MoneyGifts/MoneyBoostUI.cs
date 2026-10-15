using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyBoostUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI costText;      // فقط عدد جم
    [SerializeField] private TextMeshProUGUI timerText;     // فقط عدد ثانیه وقتی فعال است
    [SerializeField] private GameObject activeIndicator;   // اختیاری

    [SerializeField] private Image buyButtonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color blockedColor = Color.red;

    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        Refresh();
        RefreshActive();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        var m = MoneyBoostManager.Instance;
        if (m == null) return;

        if (costText != null)
            costText.text = m.GemCost.ToString("N0");

        bool can = m.CanBuyToday && !m.IsActive;

        if (buyButton != null)
            buyButton.interactable = can;
    }

    public void RefreshActive()
    {
        var m = MoneyBoostManager.Instance;
        if (m == null) return;

        if (timerText != null)
        {
            if (m.IsActive)
                timerText.text = Mathf.CeilToInt(m.TimeLeft).ToString();
            else
                timerText.text = "";
        }

        if (activeIndicator != null)
            activeIndicator.SetActive(m.IsActive);

        // وقتی فعال شد دکمه خاموش بماند
        if (buyButton != null && m.IsActive)
            buyButton.interactable = false;

        if (buyButton != null && !m.IsActive)
            buyButton.interactable = m.CanBuyToday;
    }

    public void OnClickBuy()
    {
        if (MoneyBoostManager.Instance != null)
            MoneyBoostManager.Instance.TryActivate();
    }

    public void OnNotEnoughGems()
    {
        Flash();
    }

    public void OnCannotBuy()
    {
        Flash();
    }

    private void Flash()
    {
        if (buyButtonImage == null) return;
        StopAllCoroutines();
        StartCoroutine(FlashRoutine());
    }

    private System.Collections.IEnumerator FlashRoutine()
    {
        buyButtonImage.color = blockedColor;
        yield return new WaitForSeconds(0.3f);
        buyButtonImage.color = normalColor;
    }
}