using UnityEngine;
using UnityEngine.UI;

public class HorizontalContentAligner : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;
    [SerializeField] private RectTransform content;
    [SerializeField] private RectTransform viewport;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponentInParent<ScrollRect>();

        if (scrollRect != null)
        {
            if (content == null) content = scrollRect.content;
            if (viewport == null) viewport = scrollRect.viewport;
        }
    }

    /// <summary>
    /// بعد از Build ردیف صدا بزن
    /// </summary>
    public void RefreshAlign()
    {
        if (content == null || viewport == null)
            return;

        Canvas.ForceUpdateCanvases();
        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        float contentW = content.rect.width;
        float viewW = viewport.rect.width;

        if (contentW > viewW + 1f)
        {
            // پهن‌تر از صفحه → بچسب راست، بیرون‌زدن از چپ
            SetRightAnchored();
            content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);

            if (scrollRect != null)
            {
                scrollRect.StopMovement();
                scrollRect.horizontalNormalizedPosition = 1f; // سمت راست = شروع
            }
        }
        else
        {
            // کوچک‌تر یا مساوی → بچسب چپ
            SetRightAnchored();
            content.anchoredPosition = new Vector2(0f, content.anchoredPosition.y);

            if (scrollRect != null)
            {
                scrollRect.StopMovement();
                scrollRect.horizontalNormalizedPosition = 0f;
            }
        }
    }

    private void SetRightAnchored()
    {
        content.anchorMin = new Vector2(1f, 0.5f);
        content.anchorMax = new Vector2(1f, 0.5f);
        content.pivot = new Vector2(1f, 0.5f);
    }

    private void SetLeftAnchored()
    {
        content.anchorMin = new Vector2(0f, 0.5f);
        content.anchorMax = new Vector2(0f, 0.5f);
        content.pivot = new Vector2(0f, 0.5f);
    }
}