using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class XPUI : MonoBehaviour
{
    [Header("Level Text")]
    [SerializeField] private TextMeshProUGUI levelText;

    [Header("Progress Bar")]
    [SerializeField] private Image progressFill;

    [Header("Smooth Settings")]
    [SerializeField] private float smoothSpeed = 5f;   // هر چی بیشتر = سریع‌تر نرم می‌ره

    private float targetFill;
    private int displayedLevel = -1;

    private void OnEnable()
    {
        XPManager.OnXPChanged += OnXPChanged;

        if (XPManager.Instance != null)
        {
            OnXPChanged(
                XPManager.Instance.CurrentXP,
                XPManager.Instance.XPNeededForNextLevel,
                XPManager.Instance.Level
            );
        }
    }

    private void OnDisable()
    {
        XPManager.OnXPChanged -= OnXPChanged;
    }

    private void Update()
    {
        if (progressFill == null)
            return;

        // نرم پر می‌شه
        progressFill.fillAmount = Mathf.Lerp(
            progressFill.fillAmount,
            targetFill,
            Time.deltaTime * smoothSpeed
        );
    }

    private void OnXPChanged(int currentXP, int neededXP, int level)
    {
        // عدد لول
        if (levelText != null && level != displayedLevel)
        {
            levelText.text = level.ToString();
            displayedLevel = level;

            // وقتی لول رفت بالا، نوار رو فوری صفر کن بعد دوباره نرم پر بشه
            if (progressFill != null)
                progressFill.fillAmount = 0f;
        }

        // هدف جدید برای پر شدن
        targetFill = neededXP > 0 ? (float)currentXP / neededXP : 0f;
    }
}