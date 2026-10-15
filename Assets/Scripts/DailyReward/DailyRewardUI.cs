using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyRewardUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Button claimButton;

    [Header("7 slots")]
    [SerializeField] private TextMeshProUGUI[] slotMoneyTexts;
    [SerializeField] private GameObject[] slotHighlights;   // فقط روز جاری
    //[SerializeField] private CanvasGroup[] slotCanvasGroups; // برای کم‌رنگ کردن
    // یا به‌جای CanvasGroup:
    [SerializeField] private Image[] slotBackgrounds;

    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color claimedColor = new Color(0.35f, 0.35f, 0.35f, 1f); // خاکستری
    //[SerializeField] private float claimedAlpha = 0.45f;

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

        if (LuckyWheelManager.Instance != null &&
            LuckyWheelManager.Instance.HasFreeSpinToday &&
            LuckyWheelManager.Instance.UI != null)
        {
            LuckyWheelManager.Instance.UI.Open();
            return;
        }

        if (OfflineEarningsManager.Instance != null)
            OfflineEarningsManager.Instance.CheckOfflineEarnings();
    }

    public void Refresh()
    {
        var m = DailyRewardManager.Instance;
        if (m == null) return;

        int current = m.CurrentDayIndex;

        for (int i = 0; i < 7; i++)
        {
            var reward = m.GetReward(i);

            // فقط عدد پول
            if (slotMoneyTexts != null && i < slotMoneyTexts.Length && slotMoneyTexts[i] != null)
            {
                if (reward != null)
                    slotMoneyTexts[i].text = reward.money.ToString();
            }

            bool claimed = i < current;
            bool isToday = i == current;

            // هایلایت فقط روز جاری (اختیاری)
            //if (slotHighlights != null && i < slotHighlights.Length && slotHighlights[i] != null)
            //    slotHighlights[i].SetActive(isToday);

            // فقط رنگ پس‌زمینه کارت — بدون alpha
            if (slotBackgrounds != null && i < slotBackgrounds.Length && slotBackgrounds[i] != null)
            {
                if (claimed)
                    slotBackgrounds[i].color = claimedColor;   // خاکستری / تیره
                else
                    slotBackgrounds[i].color = normalColor;    // عادی
            }
        }

        if (claimButton != null)
            claimButton.interactable = m.CanClaimToday;
    }

    public void OnClickClaim()
    {
        if (DailyRewardManager.Instance == null)
            return;

        if (DailyRewardManager.Instance.TryClaim())
            Refresh();
    }
}