using UnityEngine;

public class ExpandOutsideSafeArea : MonoBehaviour
{
    private RectTransform rt;

    private void LateUpdate()
    {
        if (rt == null)
            rt = transform as RectTransform;

        Rect safe = Screen.safeArea;
        float sw = Screen.width;
        float sh = Screen.height;

        // فاصله Safe Area تا لبه صفحه (پیکسل)
        float left = safe.x;
        float bottom = safe.y;
        float right = sw - safe.xMax;
        float top = sh - safe.yMax;

        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        // منفی = برو بیرون از والد (والد = SafeArea)
        rt.offsetMin = new Vector2(-left, -bottom);
        rt.offsetMax = new Vector2(right, top);
    }
}