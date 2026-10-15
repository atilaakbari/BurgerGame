using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MoneyPackUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button buyButton;
    [SerializeField] private TextMeshProUGUI costText;   // عدد جم
    [SerializeField] private TextMeshProUGUI rewardText; // عدد پول

    [SerializeField] private Image buyButtonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color blockedColor = Color.red;

    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Refresh()
    {
        var m = MoneyPackManager.Instance;
        if (m == null) return;

        if (costText != null)
            costText.text = m.GemCost.ToString("N0");

        if (rewardText != null)
            rewardText.text = m.MoneyAmount.ToString("N0");

        if (buyButton != null)
            buyButton.interactable = m.CanBuyToday;
    }

    public void OnClickBuy()
    {
        if (MoneyPackManager.Instance != null)
            MoneyPackManager.Instance.TryBuy();
    }

    public void OnNotEnoughGems() => Flash();
    public void OnCannotBuy() => Flash();

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