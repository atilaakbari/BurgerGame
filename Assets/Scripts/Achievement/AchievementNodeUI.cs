using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementNodeUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private Image frameImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Locked look")]
    [SerializeField] private Color unlockedColor = Color.white;
    [SerializeField] private Color lockedColor = new Color(0.35f, 0.35f, 0.35f, 0.85f);
    [SerializeField] private float lockedAlpha = 0.45f;

    private AchievementManager.AchievementInfo data;

    public void Setup(AchievementManager.AchievementInfo info)
    {
        data = info;
        Refresh();
    }

    public void Refresh()
    {
        if (data == null) return;

        bool unlocked = AchievementManager.Instance != null &&
                        AchievementManager.Instance.IsUnlocked(data.id);

        int progress = AchievementManager.Instance != null
            ? AchievementManager.Instance.GetProgress(data.id)
            : 0;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.color = unlocked ? unlockedColor : lockedColor;
            iconImage.enabled = data.icon != null;
        }

        if (frameImage != null)
            frameImage.color = unlocked ? unlockedColor : lockedColor;

        if (canvasGroup != null)
            canvasGroup.alpha = unlocked ? 1f : lockedAlpha;

        // اگر CanvasGroup نداری، فقط رنگ کافی است
        if (canvasGroup == null && iconImage != null && !unlocked)
            iconImage.color = lockedColor;

        if (titleText != null)
            titleText.text = data.title;

        if (progressText != null)
        {
            if (unlocked)
                progressText.text = "✓";
            else
                progressText.text = progress + "/" + data.targetValue;
        }
    }
}