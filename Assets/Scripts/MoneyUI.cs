using TMPro;
using UnityEngine;

// این رو روی همون آبجکتی بذار که TextMeshProUGUI روشه (یا یه پرنت خالی بالاش)
public class MoneyUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI moneyText;

    private void OnEnable()
    {
        MoneyManager.OnMoneyChanged += UpdateText;

        // همون لحظه‌ی فعال شدن، مقدار فعلی رو نشون بده (نه این‌که منتظر اولین تغییر بمونه)
        if (MoneyManager.Instance != null)
            UpdateText(MoneyManager.Instance.Money);
    }

    private void OnDisable()
    {
        MoneyManager.OnMoneyChanged -= UpdateText;
    }

    private void UpdateText(int amount)
    {
        if (moneyText == null)
            return;

        // N0 = جداکننده‌ی هزارگان (مثلاً 12,500 به‌جای 12500)
        moneyText.text = amount.ToString("N0");
    }
}