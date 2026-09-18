using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AchievementPopupUI : MonoBehaviour
{
    [SerializeField] private GameObject root;
    [SerializeField] private TextMeshProUGUI titleText;
    [SerializeField] private TextMeshProUGUI descText;
    [SerializeField] private Image iconImage;
    [SerializeField] private float showDuration = 3f;

    private Coroutine routine;
    private bool subscribed;

    private void Start()
    {
        if (root != null)
            root.SetActive(false);

        TrySubscribe();
    }

    private void Update()
    {
        // اگر Manager بعداً ساخته شد، وصل شو
        if (!subscribed)
            TrySubscribe();
    }

    private void OnDestroy()
    {
        Unsubscribe();
    }

    private void TrySubscribe()
    {
        if (subscribed) return;
        if (AchievementManager.Instance == null) return;

        AchievementManager.Instance.OnAchievementUnlocked += Show;
        subscribed = true;
        Debug.Log("AchievementPopupUI subscribed.");
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= Show;
        subscribed = false;
    }

    private void Show(AchievementDefinition def)
    {
        if (def == null) return;

        Debug.Log("Popup Show: " + def.title);

        if (titleText != null) titleText.text = def.title;
        if (descText != null) descText.text = def.description;

        if (iconImage != null)
        {
            if (def.icon != null)
            {
                iconImage.sprite = def.icon;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }

        if (routine != null)
            StopCoroutine(routine);

        routine = StartCoroutine(ShowRoutine());
    }

    private IEnumerator ShowRoutine()
    {
        if (root != null)
            root.SetActive(true);
        else
            Debug.LogWarning("AchievementPopupUI: Root is null!");

        yield return new WaitForSeconds(showDuration);

        if (root != null)
            root.SetActive(false);

        routine = null;
    }
}