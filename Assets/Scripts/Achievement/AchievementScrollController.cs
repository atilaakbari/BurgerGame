using UnityEngine;
using UnityEngine.UI;

public class AchievementScrollController : MonoBehaviour
{
    [SerializeField] private ScrollRect scrollRect;

    private void Awake()
    {
        if (scrollRect == null)
            scrollRect = GetComponent<ScrollRect>();

        if (scrollRect == null) return;

        // همان حالتی که خودت گفتی کار می‌کند
        scrollRect.movementType = ScrollRect.MovementType.Unrestricted;
        scrollRect.horizontal = true;
        scrollRect.vertical = false;

        scrollRect.onValueChanged.AddListener(OnScroll);
    }

    private void OnDestroy()
    {
        if (scrollRect != null)
            scrollRect.onValueChanged.RemoveListener(OnScroll);
    }

    private void OnScroll(Vector2 _)
    {
        LimitScroll();
    }

    private void LateUpdate()
    {
        LimitScroll();
    }

    private void LimitScroll()
    {
        if (scrollRect == null) return;

        float v = scrollRect.verticalNormalizedPosition;

        // 1 = بالا ، 0 = پایین
        if (v > 1f)
        {
            scrollRect.verticalNormalizedPosition = 1f;
            scrollRect.velocity = Vector2.zero;
        }
        else if (v < 0f)
        {
            scrollRect.verticalNormalizedPosition = 0f;
            scrollRect.velocity = Vector2.zero;
        }
    }

    public void ResetToTop()
    {
        if (scrollRect == null) return;

        scrollRect.StopMovement();
        scrollRect.velocity = Vector2.zero;
        Canvas.ForceUpdateCanvases();
        scrollRect.verticalNormalizedPosition = 1f;
    }
}