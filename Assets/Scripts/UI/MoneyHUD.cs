using UnityEngine;
using UnityEngine.UI;

public class MoneyHUD : MonoBehaviour
{
    [SerializeField] private Text moneyText;

    private void Awake()
    {
        if (moneyText == null)
            moneyText = CreateRuntimeLabel();
    }

    private void OnEnable()
    {
        MoneyManager.OnMoneyChanged += Refresh;
        Refresh(MoneyManager.Instance != null ? MoneyManager.Instance.Money : 0);
    }

    private void OnDisable()
    {
        MoneyManager.OnMoneyChanged -= Refresh;
    }

    private void Refresh(int amount)
    {
        if (moneyText != null)
            moneyText.text = amount.ToString();
    }

    private Text CreateRuntimeLabel()
    {
        Canvas canvas = FindFirstObjectByType<Canvas>();

        if (canvas == null)
        {
            GameObject canvasObject = new GameObject(
                "GameCanvas",
                typeof(Canvas),
                typeof(CanvasScaler),
                typeof(GraphicRaycaster)
            );

            canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            CanvasScaler scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
        }

        GameObject labelObject = new GameObject("MoneyText", typeof(RectTransform), typeof(Text));
        labelObject.transform.SetParent(canvas.transform, false);

        RectTransform rect = labelObject.GetComponent<RectTransform>();
        rect.anchorMin = new Vector2(0f, 1f);
        rect.anchorMax = new Vector2(0f, 1f);
        rect.pivot = new Vector2(0f, 1f);
        rect.anchoredPosition = new Vector2(36f, -28f);
        rect.sizeDelta = new Vector2(420f, 80f);

        Text text = labelObject.GetComponent<Text>();
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        if (text.font == null)
            text.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
        text.fontSize = 48;
        text.fontStyle = FontStyle.Bold;
        text.alignment = TextAnchor.UpperLeft;
        text.color = Color.white;
        text.raycastTarget = false;
        text.text = "0";

        Outline outline = labelObject.AddComponent<Outline>();
        outline.effectColor = new Color(0f, 0f, 0f, 0.7f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return text;
    }
}
