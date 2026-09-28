using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DailyMissionCardUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private TextMeshProUGUI progressText;
    [SerializeField] private Image progressFill;
    [SerializeField] private TextMeshProUGUI rewardText;

    [Header("Completed look")]
    [SerializeField] private GameObject completedOverlay; // سبز کمرنگ
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private float completedAlpha = 0.75f;

    private DailyMissionManager.ActiveMission data;

    public void Setup(DailyMissionManager.ActiveMission mission)
    {
        data = mission;
        Refresh();
    }

    public void Refresh()
    {
        if (data == null || data.def == null)
            return;

        MissionDefinition d = data.def;

        if (iconImage != null)
        {
            iconImage.sprite = d.icon;
            iconImage.enabled = d.icon != null;
        }

        if (titleText != null)
            titleText.text = d.title;

        if (descText != null)
            descText.text = d.description;

        if (progressText != null)
            progressText.text = data.progress + "/" + d.targetAmount;

        if (progressFill != null)
            progressFill.fillAmount =
                d.targetAmount <= 0 ? 0f :
                Mathf.Clamp01((float)data.progress / d.targetAmount);

        if (rewardText != null)
            rewardText.text = "$" + d.rewardMoney + "  XP " + d.rewardXP;

        bool done = data.completed;

        if (completedOverlay != null)
            completedOverlay.SetActive(done);

        if (canvasGroup != null)
            canvasGroup.alpha = done ? completedAlpha : 1f;
    }
}