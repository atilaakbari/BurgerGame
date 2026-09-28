using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CustomerPatience : MonoBehaviour
{
    public enum Mood
    {
        Happy,
        Normal,
        Angry
    }

    [Header("Timing")]
    [SerializeField] private float maxWaitTime = 60f;
    [Tooltip("از این مقدار به بعد ناحیه قرمز (0..1)")]
    [Range(0.2f, 0.95f)]
    [SerializeField] private float redZoneStart = 0.7f;

    [Header("Money Multiplier")]
    [SerializeField] private float maxMultiplier = 1.5f; // خیلی سریع
    [SerializeField] private float midMultiplier = 1.0f;
    [SerializeField] private float minMultiplier = 0.5f; // خیلی دیر

    [Header("Patience Bar")]
    [SerializeField] private GameObject patienceRoot;
    [SerializeField] private Image fillImage;
    [SerializeField] private Color greenColor = new Color(0.2f, 0.85f, 0.3f);
    [SerializeField] private Color redColor = new Color(0.9f, 0.15f, 0.15f);

    [Header("Emoji (Sprites)")]
    [SerializeField] private GameObject emojiRoot;
    [SerializeField] private Image emojiImage;
    [SerializeField] private Sprite happySprite;
    [SerializeField] private Sprite normalSprite;
    [SerializeField] private Sprite angrySprite;
    [SerializeField] private float angryFlashDuration = 1.2f;
    [SerializeField] private float finalEmojiDuration = 1.5f;

    private bool isRunning;
    private bool isFinished;
    private float elapsed;
    private bool angryFlashed;
    private Coroutine emojiRoutine;

    /// <summary>0 = تازه، 1 = پر شده</summary>
    public float Progress
    {
        get
        {
            if (maxWaitTime <= 0.01f)
                return 1f;
            return Mathf.Clamp01(elapsed / maxWaitTime);
        }
    }

    public bool IsInRedZone => Progress >= redZoneStart;
    public bool IsRunning => isRunning && !isFinished;

    private void Awake()
    {
        HideBar();
        HideEmoji();
    }

    private void Update()
    {
        if (!isRunning || isFinished)
            return;

        elapsed += Time.deltaTime;
        if (elapsed > maxWaitTime)
            elapsed = maxWaitTime;

        RefreshBar();

        // ورود به قرمز → ایموجی عصبانی کوتاه
        if (!angryFlashed && IsInRedZone)
        {
            angryFlashed = true;
            PlayEmoji(angrySprite, angryFlashDuration);
        }
    }

    // =========================================================
    // API
    // =========================================================

    /// <summary>
    /// waitTime از BurgerOrder می‌آید. اگر از قبل در حال اجرا باشد ریست نمی‌شود.
    /// </summary>
    public void StartPatience(float waitTime = -1f)
    {
        // قبلاً شروع شده و هنوز تمام نشده → دست نزن
        if (isRunning && !isFinished)
            return;

        if (waitTime > 0.01f)
            maxWaitTime = waitTime;

        isRunning = true;
        isFinished = false;
        elapsed = 0f;
        angryFlashed = false;

        if (emojiRoutine != null)
        {
            StopCoroutine(emojiRoutine);
            emojiRoutine = null;
        }

        HideEmoji();
        ShowBar();
        RefreshBar();
    }

    /// <summary>دیگر اول صف نیست</summary>
    public void StopPatience()
    {
        isRunning = false;
        HideBar();
    }

    /// <summary>
    /// بعد از تحویل موفق سفارش.
    /// نوار را می‌بندد، ایموجی نهایی را نشان می‌دهد، ضریب پول برمی‌گرداند.
    /// </summary>
    public float FinishAndGetMultiplier()
    {
        float mult = GetPayMultiplier();
        Mood mood = GetMood();

        isFinished = true;
        isRunning = false;
        HideBar();

        Sprite sprite =
            mood == Mood.Happy ? happySprite :
            mood == Mood.Angry ? angrySprite :
            normalSprite;

        PlayEmoji(sprite, finalEmojiDuration);

        return mult;
    }

    public float GetPayMultiplier()
    {
        float p = Progress;

        // سریع: 0 → 0.33
        if (p <= 0.33f)
            return Mathf.Lerp(maxMultiplier, midMultiplier, p / 0.33f);

        // معمولی تا قبل قرمز
        if (p < redZoneStart)
            return midMultiplier;

        // قرمز تا آخر
        float t = Mathf.InverseLerp(redZoneStart, 1f, p);
        return Mathf.Lerp(midMultiplier, minMultiplier, t);
    }

    public Mood GetMood()
    {
        float p = Progress;
        if (p <= 0.33f)
            return Mood.Happy;
        if (p < redZoneStart)
            return Mood.Normal;
        return Mood.Angry;
    }

    // =========================================================
    // UI
    // =========================================================

    private void RefreshBar()
    {
        if (fillImage == null)
            return;

        float p = Progress;
        fillImage.fillAmount = p;
        fillImage.color = Color.Lerp(greenColor, redColor, p);
    }

    private void ShowBar()
    {
        if (patienceRoot != null)
            patienceRoot.SetActive(true);
    }

    private void HideBar()
    {
        if (patienceRoot != null)
            patienceRoot.SetActive(false);
    }

    private void HideEmoji()
    {
        if (emojiRoot != null)
            emojiRoot.SetActive(false);
    }

    private void PlayEmoji(Sprite sprite, float duration)
    {
        if (emojiRoot == null || emojiImage == null || sprite == null)
            return;

        if (emojiRoutine != null)
            StopCoroutine(emojiRoutine);

        emojiRoutine = StartCoroutine(EmojiRoutine(sprite, duration));
    }

    private IEnumerator EmojiRoutine(Sprite sprite, float duration)
    {
        emojiImage.sprite = sprite;
        emojiRoot.SetActive(true);
        yield return new WaitForSeconds(duration);
        emojiRoot.SetActive(false);
        emojiRoutine = null;
    }
}