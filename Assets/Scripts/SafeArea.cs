using UnityEngine;

// این رو روی یه Panel خالی (فقط RectTransform) بذار که مستقیم زیر Canvas اصلیه.
// همه چیز دیگه (HUD, StationUI, Popups, PauseMenu) باید زیر همین پنل بره.
[RequireComponent(typeof(RectTransform))]
public class SafeArea : MonoBehaviour
{
    private RectTransform panel;
    private Rect lastSafeArea = new Rect(0, 0, 0, 0);
    private Vector2Int lastScreenSize = new Vector2Int(0, 0);
    private ScreenOrientation lastOrientation = ScreenOrientation.AutoRotation;

    private void Awake()
    {
        panel = GetComponent<RectTransform>();
        Refresh();
    }

    private void Update()
    {
        // هر فریم چک می‌کنیم چون موقع چرخش گوشی (Orientation) این مقادیر عوض می‌شن
        Refresh();
    }

    private void Refresh()
    {
        Rect safeArea = Screen.safeArea;

        bool changed = safeArea != lastSafeArea
            || Screen.width != lastScreenSize.x
            || Screen.height != lastScreenSize.y
            || Screen.orientation != lastOrientation;

        if (!changed)
            return;

        lastSafeArea = safeArea;
        lastScreenSize = new Vector2Int(Screen.width, Screen.height);
        lastOrientation = Screen.orientation;

        ApplySafeArea(safeArea);
    }

    private void ApplySafeArea(Rect safeArea)
    {
        Vector2 anchorMin = safeArea.position;
        Vector2 anchorMax = safeArea.position + safeArea.size;

        anchorMin.x /= Screen.width;
        anchorMin.y /= Screen.height;
        anchorMax.x /= Screen.width;
        anchorMax.y /= Screen.height;

        panel.anchorMin = anchorMin;
        panel.anchorMax = anchorMax;
    }
}
