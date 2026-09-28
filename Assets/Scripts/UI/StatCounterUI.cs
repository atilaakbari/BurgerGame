using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatCounterUI : MonoBehaviour
{
    public enum StatType
    {
        BurgersDelivered,
        SodasDelivered,
        PattiesCooked,
        MoneyEarned
        // بعداً اینجا اضافه کن
    }

    [Header("Which stat")]
    [SerializeField] private StatType statType = StatType.BurgersDelivered;

    [Header("UI (یکی کافی است)")]
    [SerializeField] private TextMeshProUGUI tmpText;
    [SerializeField] private Text uiText;

    [Header("Format — {0} = عدد")]
    [SerializeField] private string format = "{0}";

    [Header("Settings")]
    [SerializeField] private bool refreshEveryFrame = true;
    [SerializeField] private float refreshInterval = 0.25f;

    private float timer;

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        if (refreshEveryFrame)
        {
            Refresh();
            return;
        }

        timer += Time.deltaTime;
        if (timer >= refreshInterval)
        {
            timer = 0f;
            Refresh();
        }
    }

    public void Refresh()
    {
        int value = GetValue(statType);
        string msg = string.Format(format, value);

        if (tmpText != null)
            tmpText.text = msg;

        if (uiText != null)
            uiText.text = msg;
    }

    private int GetValue(StatType type)
    {
        if (SaveManager.Instance == null)
            return 0;

        switch (type)
        {
            case StatType.BurgersDelivered:
                return SaveManager.Instance.GetTotalBurgersDelivered();

            case StatType.SodasDelivered:
                return SaveManager.Instance.GetTotalSodasDelivered();

            case StatType.PattiesCooked:
                return SaveManager.Instance.GetTotalPattiesCooked();

            case StatType.MoneyEarned:
                return SaveManager.Instance.GetTotalMoneyEarned();

            default:
                return 0;
        }
    }

    /// <summary>از کد دیگر می‌توانی نوع را عوض کنی</summary>
    public void SetStatType(StatType type)
    {
        statType = type;
        Refresh();
    }
}