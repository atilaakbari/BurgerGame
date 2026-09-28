using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopupUI : MonoBehaviour
{
    [Header("همین Panel که باید روشن شود")]
    [SerializeField] private GameObject panelRoot;

    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image iconImage;

    [SerializeField] private float showDuration = 3f;

    private Coroutine routine;
    private bool subscribed;

    private void Awake()
    {
        // اگر panelRoot خالی بود، خود این آبجکت
        if (panelRoot == null)
            panelRoot = gameObject;

        SetPanelVisible(false);
    }

    private void Update()
    {
        if (subscribed) return;
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.OnUnlocked += HandleUnlocked;
        subscribed = true;
        Debug.Log("AchievementPopupUI subscribed");
    }

    private void OnDestroy()
    {
        if (subscribed && AchievementManager.Instance != null)
            AchievementManager.Instance.OnUnlocked -= HandleUnlocked;
    }

    private void HandleUnlocked(
        string id,
        string title,
        string desc,
        Sprite icon,
        int progress,
        int target)
    {
        Debug.Log("Popup HandleUnlocked: " + title);

        if (titleText != null)
            titleText.text = title;

        if (descText != null)
            descText.text = string.IsNullOrEmpty(desc) ? "" : desc;

        // آیکون
        if (iconImage != null)
        {
            if (icon != null)
            {
                iconImage.sprite = icon;
                iconImage.enabled = true;
                iconImage.color = Color.white;
                Debug.Log("Icon set: " + icon.name);
            }
            else
            {
                iconImage.enabled = false;
                Debug.LogWarning("Achievement icon is null — در Manager روی آن دستاورد Sprite بگذار");
            }
        }
        else
        {
            Debug.LogWarning("iconImage reference is null in Popup");
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        SetPanelVisible(true);
        Debug.Log("Popup panel ON");

        yield return new WaitForSeconds(showDuration);

        SetPanelVisible(false);
        Debug.Log("Popup panel OFF");
        routine = null;
    }

    private void SetPanelVisible(bool visible)
    {
        if (panelRoot == null)
        {
            Debug.LogError("panelRoot is null");
            return;
        }

        // اگر والد خاموش باشد، فرزند دیده نمی‌شود
        if (visible)
        {
            Transform t = panelRoot.transform;
            while (t != null)
            {
                if (!t.gameObject.activeSelf)
                    t.gameObject.SetActive(true);
                t = t.parent;
            }
        }

        panelRoot.SetActive(visible);

        CanvasGroup cg = panelRoot.GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.alpha = visible ? 1f : 0f;
            cg.interactable = visible;
            cg.blocksRaycasts = visible;
        }

        if (visible)
            panelRoot.transform.SetAsLastSibling();
    }

    [ContextMenu("Test Show Popup")]
    private void TestShow()
    {
        HandleUnlocked("test", "تست", "توضیح تست", null, 1, 1);
    }
}