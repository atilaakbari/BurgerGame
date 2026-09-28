using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MissionToastUI : MonoBehaviour
{
    public static MissionToastUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private CanvasGroup canvasGroup;
    [SerializeField] private RectTransform toastRect;
    [SerializeField] private Image iconImage;
    [SerializeField] private TextMeshProUGUI headerText;
    [SerializeField] private TextMeshProUGUI deltaText;
    [SerializeField] private TextMeshProUGUI remainText;

    [Header("Timing")]
    [SerializeField] private float showDuration = 1.8f;
    [SerializeField] private float slideSeconds = 0.2f;
    [SerializeField] private float gapBetween = 0.15f;
    [SerializeField] private Vector2 hiddenPos = new Vector2(450f, 0f);
    [SerializeField] private Vector2 shownPos = Vector2.zero;

    private class Req
    {
        public Sprite icon;
        public int delta;
        public int remaining;
    }

    private readonly Queue<Req> queue = new Queue<Req>();
    private bool playing;

    private void Awake()
    {
        Instance = this;

        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        HideImmediate();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    public void Enqueue(Sprite icon, int delta, int remaining)
    {
        if (delta <= 0) return;

        queue.Enqueue(new Req
        {
            icon = icon,
            delta = delta,
            remaining = remaining
        });

        Debug.Log("[Toast] queue=" + queue.Count + " playing=" + playing);

        if (!playing)
            StartCoroutine(PlayQueue());
    }

    private IEnumerator PlayQueue()
    {
        playing = true;

        while (queue.Count > 0)
        {
            Req r = queue.Dequeue();
            yield return ShowOne(r);
            if (gapBetween > 0f)
                yield return new WaitForSecondsRealtime(gapBetween);
        }

        playing = false;
    }

    private IEnumerator ShowOne(Req r)
    {
        if (headerText != null) headerText.text = "مأموریت";
        if (deltaText != null) deltaText.text = "+" + r.delta;
        if (remainText != null) remainText.text = "باقی‌مانده: " + r.remaining;

        if (iconImage != null)
        {
            iconImage.sprite = r.icon;
            iconImage.enabled = r.icon != null;
        }

        transform.SetAsLastSibling();

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (toastRect != null)
        {
            toastRect.anchoredPosition = hiddenPos;
            float t = 0f;
            while (t < slideSeconds)
            {
                t += Time.unscaledDeltaTime;
                toastRect.anchoredPosition = Vector2.Lerp(hiddenPos, shownPos, Mathf.Clamp01(t / slideSeconds));
                yield return null;
            }
            toastRect.anchoredPosition = shownPos;
        }

        yield return new WaitForSecondsRealtime(showDuration);

        if (toastRect != null)
        {
            float t = 0f;
            while (t < slideSeconds)
            {
                t += Time.unscaledDeltaTime;
                toastRect.anchoredPosition = Vector2.Lerp(shownPos, hiddenPos, Mathf.Clamp01(t / slideSeconds));
                yield return null;
            }
            toastRect.anchoredPosition = hiddenPos;
        }

        HideImmediate();
    }

    private void HideImmediate()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (toastRect != null)
            toastRect.anchoredPosition = hiddenPos;
    }
}