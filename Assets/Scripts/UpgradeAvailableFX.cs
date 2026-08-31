using System.Collections;
using UnityEngine;

// این اسکریپت رو یا مستقیم روی خودِ فلشی که ساختی بذار،
// یا رو یه آبجکت دیگه بذار و فلش رو تو فیلد arrow وصل کن.
// هیچ چیزی خودکار ساخته نمی‌شه - فقط همون آبجکتی که بدی رو کنترل می‌کنه.
public class UpgradeAvailableFX : MonoBehaviour
{
    [Header("Arrow (فلشی که خودت ساختی)")]
    [SerializeField] private Transform arrow; // خالی بذاری = خودِ همین آبجکت در نظر گرفته می‌شه

    [Header("Bounce Down (پایین می‌ره و ارتفاعش کم می‌شه، بعد کامل برمی‌گرده حالت عادی)")]
    [SerializeField] private float bounceHeight = 0.2f;
    [SerializeField] private float bounceSpeed = 2f;
    [SerializeField] private float squashAmount = 0.3f; // وقتی پایین‌ترین نقطه‌ست، چقدر ارتفاعش کم بشه (0..1)

    [Header("Attention Burst (هر چند ثانیه یه تاکید اضافه)")]
    [SerializeField] private float attentionInterval = 3f;
    [SerializeField] private float attentionScaleBoost = 1.3f;
    [SerializeField] private float attentionDuration = 0.25f;

    private SpriteRenderer arrowRenderer; // اختیاریه - اگه نبود فقط SetActive استفاده می‌شه، بدون فید
    private Vector3 basePosition;
    private Vector3 baseScale;
    private Coroutine loopRoutine;
    private Coroutine attentionRoutine;
    private float currentAttentionScale = 1f;
    private bool isShown = true; // true فرض می‌کنیم که اولین Hide() تو Awake واقعاً اجرا بشه

    private void Awake()
    {
        if (arrow == null)
            arrow = transform;

        arrowRenderer = arrow.GetComponent<SpriteRenderer>();

        basePosition = arrow.localPosition;
        baseScale = arrow.localScale;

        Hide(instant: true);
    }

    // ==========================================================
    // PUBLIC API
    // ==========================================================
    // این دوتا متد رو هر چندبار که بخوای می‌تونی صدا بزنی (حتی هر فریم) -
    // خودشون تشخیص می‌دن که آیا الان لازمه کاری بشه یا نه.

    public void Show()
    {
        if (isShown)
            return; // از قبل نشون داده شده، کاری لازم نیست

        isShown = true;

        // نکته‌ی حیاتی: اگه خودِ آبجکتی که این اسکریپت روشه غیرفعال باشه، نمیشه روش Coroutine ران کرد
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (arrow == null)
            arrow = transform;

        arrow.gameObject.SetActive(true);

        if (loopRoutine != null) StopCoroutine(loopRoutine);
        if (attentionRoutine != null) StopCoroutine(attentionRoutine);

        loopRoutine = StartCoroutine(BounceAndPulse());
        attentionRoutine = StartCoroutine(AttentionBurstLoop());
    }

    public void Hide(bool instant = false)
    {
        if (!isShown && !instant)
            return; // از قبل مخفیه، کاری لازم نیست

        isShown = false;

        if (loopRoutine != null) { StopCoroutine(loopRoutine); loopRoutine = null; }
        if (attentionRoutine != null) { StopCoroutine(attentionRoutine); attentionRoutine = null; }

        // اگه رندرر نداره یا خواستی فوری خاموش شه، مستقیم SetActive کن
        if (instant || arrowRenderer == null)
        {
            arrow.gameObject.SetActive(false);
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
            // downAmount بین 0 (بالا/حالت عادی) و 1 (پایین‌ترین نقطه)
            float downAmount = (1f - Mathf.Cos(Time.time * bounceSpeed)) * 0.5f;

            float verticalOffset = -downAmount * bounceHeight;
            arrow.localPosition = basePosition + Vector3.up * verticalOffset;

            float heightScale = 1f - downAmount * squashAmount;

            Vector3 finalScale = new Vector3(baseScale.x, baseScale.y * heightScale, baseScale.z) * currentAttentionScale;
            arrow.localScale = finalScale;

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
        Color startColor = arrowRenderer.color;
        float t = 0f;
        float fadeTime = 0.2f;

        while (t < fadeTime)
        {
            t += Time.deltaTime;
            Color c = startColor;
            c.a = Mathf.Lerp(startColor.a, 0f, t / fadeTime);
            arrowRenderer.color = c;
            yield return null;
        }

        arrowRenderer.color = startColor;
        arrow.gameObject.SetActive(false);
    }
}