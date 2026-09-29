using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [Header("Visibility — CanvasGroup روی همین آبجکت یا child")]
    [SerializeField] private CanvasGroup canvasGroup;

    [Header("Texts / Bar")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Image timerFill;

    [Tooltip("از این عدد به بالا نشان بده (1 = از اولین تحویل)")]
    [SerializeField] private int showFromCombo = 1;

    [SerializeField] private bool logDebug = true;

    private void Awake()
    {
        if (canvasGroup == null)
            canvasGroup = GetComponent<CanvasGroup>();

        Hide();
    }

    private void OnEnable()
    {
        Subscribe();
        Refresh();
    }

    private void Start()
    {
        Subscribe();
        Refresh();
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Update()
    {
        // اگر Manager دیر لود شد
        if (ComboManager.Instance != null)
            Refresh();
    }

    private void Subscribe()
    {
        if (ComboManager.Instance == null)
            return;

        ComboManager.Instance.OnComboUpdated -= Refresh;
        ComboManager.Instance.OnComboUpdated += Refresh;
    }

    private void Unsubscribe()
    {
        if (ComboManager.Instance == null)
            return;

        ComboManager.Instance.OnComboUpdated -= Refresh;
    }

    private void Refresh()
    {
        if (ComboManager.Instance == null)
        {
            Hide();
            return;
        }

        int combo = ComboManager.Instance.CurrentCombo;
        bool show = combo >= showFromCombo && ComboManager.Instance.IsActive;

        if (show)
            Show(combo);
        else
            Hide();
    }

    private void Show(int combo)
    {
        // متن — همیشه
        if (comboText != null)
        {
            comboText.text = "x" + combo;
            comboText.ForceMeshUpdate();
        }
        else if (logDebug)
        {
            Debug.LogWarning("[ComboUI] comboText is NULL");
        }

        // نوار
        if (timerFill != null)
            timerFill.fillAmount = ComboManager.Instance.TimeLeftNormalized;

        // دیده شدن با alpha — بدون SetActive
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        transform.SetAsLastSibling();

        if (logDebug)
            Debug.Log("[ComboUI] SHOW x" + combo);
    }

    private void Hide()
    {
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }

        if (timerFill != null)
            timerFill.fillAmount = 0f;
    }
}