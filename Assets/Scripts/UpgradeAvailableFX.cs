using System.Collections;
using UnityEngine;

// روی یه آبجکت خالی بالای استیشن بذار - هیچ Sprite ای هم لازم نیست از قبل بدی،
// اگه iconSprite رو خالی بذاری خودش یه فلش رو به بالا می‌سازه.
public class UpgradeAvailableFX : MonoBehaviour
{
    [Header("Icon (اختیاری - اگه خالی بذاری خودش فلش می‌سازه)")]
    [SerializeField] private Sprite iconSprite;
    [SerializeField] private Color iconColor = Color.white;
    [SerializeField] private float iconSize = 0.6f;
    [SerializeField] private bool pointDown = true; // فلش رو به پایین (اشاره به استیشن)

    [Header("یا اگه خودت یه Arrow تو صحنه ساختی، مستقیم اینجا وصلش کن")]
    [SerializeField] private Transform manualArrow; // اگه پر باشه، به‌جای ساختن خودکار از همین استفاده می‌شه

    [Header("Bounce Down (پایین می‌ره و ارتفاعش کم می‌شه، بعد کامل برمی‌گرده حالت عادی)")]
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float squashAmount = 0.3f; // وقتی پایین‌ترین نقطه‌ست، چقدر ارتفاعش کم بشه (0..1)

    [Header("Attention Burst (هر چند ثانیه یه تاکید اضافه)")]
    [SerializeField] private float attentionInterval = 3f;
    [SerializeField] private float attentionScaleBoost = 1.3f;
    [SerializeField] private float attentionDuration = 0.25f;

    private SpriteRenderer iconRenderer;
    private Transform iconTransform;
    private Vector3 basePosition;
    private Camera cam;
    private Coroutine loopRoutine;
    private Coroutine attentionRoutine;
    private float currentAttentionScale = 1f;

    private void Awake()
    {
        cam = Camera.main;
        EnsureIcon();
        basePosition = iconTransform.localPosition;
        Hide(instant: true);
    }

    /*private void LateUpdate()
    {
        if (cam == null)
            cam = Camera.main;

        if (cam != null && iconTransform != null && iconTransform.gameObject.activeInHierarchy)
            iconTransform.forward = cam.transform.forward;
    }*/

    // ==========================================================
    // SELF-BUILD
    // ==========================================================

    private void EnsureIcon()
    {
        // اگه خودت یه Arrow تو صحنه ساختی و وصلش کردی، از همون استفاده کن
        if (manualArrow != null)
        {
            iconTransform = manualArrow;
            iconRenderer = manualArrow.GetComponent<SpriteRenderer>();
            return;
        }

        GameObject iconObj = new GameObject("UpgradeIcon_Auto");
        iconObj.transform.SetParent(transform, false);

        iconRenderer = iconObj.AddComponent<SpriteRenderer>();
        iconRenderer.sprite = iconSprite != null ? iconSprite : GenerateArrowSprite();
        iconRenderer.color = iconColor;
        iconRenderer.sortingOrder = 500;
        iconRenderer.flipY = pointDown; // فلش رو برعکس کن که به پایین اشاره کنه

        iconTransform = iconObj.transform;
        iconTransform.localScale = Vector3.one * iconSize;
    }

    // یه فلش ساده رو به بالا می‌سازه، بدون نیاز به هیچ فایل عکسی
    private Sprite GenerateArrowSprite()
    {
        int size = 128;
        Texture2D tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        Color clear = new Color(0f, 0f, 0f, 0f);

        for (int y = 0; y < size; y++)
            for (int x = 0; x < size; x++)
                tex.SetPixel(x, y, clear);

        Vector2 center = new Vector2(size / 2f, size / 2f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float nx = (x - center.x) / (size * 0.5f);
                float ny = (y - center.y) / (size * 0.5f);

                bool inTriangle =
                    ny > 0.1f && ny < 0.9f &&
                    Mathf.Abs(nx) < (0.9f - ny) * 0.9f;

                bool inStem =
                    ny <= 0.1f && ny > -0.7f &&
                    Mathf.Abs(nx) < 0.22f;

                if (inTriangle || inStem)
                    tex.SetPixel(x, y, Color.white);
            }
        }

        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), 100f);
    }

    // ==========================================================
    // PUBLIC API (همون قبلی - نیازی نیست CookingStationUpgrade عوض بشه)
    // ==========================================================

    public void Show()
    {
        gameObject.SetActive(true);

        if (iconTransform != null)
            iconTransform.gameObject.SetActive(true);

        if (loopRoutine != null) StopCoroutine(loopRoutine);
        if (attentionRoutine != null) StopCoroutine(attentionRoutine);

        loopRoutine = StartCoroutine(BounceAndPulse());
        attentionRoutine = StartCoroutine(AttentionBurstLoop());
    }

    public void Hide(bool instant = false)
    {
        if (loopRoutine != null) { StopCoroutine(loopRoutine); loopRoutine = null; }
        if (attentionRoutine != null) { StopCoroutine(attentionRoutine); attentionRoutine = null; }

        if (instant)
        {
            gameObject.SetActive(false);
            return;
        }

        StartCoroutine(FadeOutAndDisable());
    }

    // ==========================================================
    // ANIMATION
    // ==========================================================

    private IEnumerator BounceAndPulse()
    {
        while (true)
        {
            // downAmount بین 0 (بالا/حالت عادی) و 1 (پایین‌ترین نقطه) - مثل یه دایره کامل رفت‌وبرگشت
            float downAmount = (1f - Mathf.Cos(Time.time * bounceSpeed)) * 0.5f;

            // موقعیت: از حالت عادی می‌ره پایین و برمی‌گرده به همون حالت عادی
            float verticalOffset = -downAmount * bounceHeight;
            iconTransform.localPosition = basePosition + Vector3.up * verticalOffset;

            // ارتفاع (اسکیل Y): وقتی پایین‌تره کم می‌شه (فشرده)، وقتی بالاست کاملاً عادیه (1)
            float heightScale = 1f - downAmount * squashAmount;

            Vector3 finalScale = new Vector3(iconSize, iconSize * heightScale, iconSize) * currentAttentionScale;
            iconTransform.localScale = finalScale;

            yield return null;
        }
    }

    private IEnumerator AttentionBurstLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(attentionInterval);

            float t = 0f;

            while (t < attentionDuration)
            {
                t += Time.deltaTime;
                float p = t / attentionDuration;
                currentAttentionScale = Mathf.Lerp(1f, attentionScaleBoost, Mathf.Sin(p * Mathf.PI));
                yield return null;
            }

            currentAttentionScale = 1f;
        }
    }

    private IEnumerator FadeOutAndDisable()
    {
        if (iconRenderer == null)
        {
            gameObject.SetActive(false);
            yield break;
        }

        Color startColor = iconRenderer.color;
        float t = 0f;
        float fadeTime = 0.2f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t / fadeTime);
            iconRenderer.color = c;
            yield return null;
        }

        iconRenderer.color = startColor;
        gameObject.SetActive(false);
    }
}