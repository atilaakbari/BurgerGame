using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ComboUI : MonoBehaviour
{
    [Header("Hierarchy")]
    [Tooltip("فرزند: ComboRoot — فقط این خاموش/روشن می‌شود")]
    [SerializeField] private GameObject comboRoot;

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI comboText;
    [SerializeField] private Image timerFill;

    [Header("Show")]
    [Tooltip("از چه کمبویی Root روشن شود")]
    [SerializeField] private int showFromCombo = 1;

    [SerializeField] private bool logDebug = false;

    private int lastShownCombo = -1;
    private bool lastVisible;

    private void Awake()
    {
        // Panel همیشه روشن می‌ماند
        if (comboRoot != null)
            comboRoot.SetActive(false);

        lastVisible = false;
        lastShownCombo = -1;

        // اگر اشتباهی اسکریپت روی خود Root باشد
        if (comboRoot != null && comboRoot == gameObject)
        {
            Debug.LogError(
                "[ComboUI] اسکریپت را روی ComboPanel بگذار، نه روی ComboRoot. " +
                "Root فقط visual است."
            );
        }
    }

    private void Update()
    {
        ComboManager manager = ComboManager.Instance;

        if (manager == null)
        {
            SetVisible(false);
            return;
        }

        int combo = manager.Combo;
        bool shouldShow = manager.IsActive && combo >= showFromCombo;

        if (!shouldShow)
        {
            SetVisible(false);
            return;
        }

        SetVisible(true);
        ApplyComboVisual(combo, manager.TimerNormalized);
    }

    private void ApplyComboVisual(int combo, float timer01)
    {
        if (comboText != null)
        {
            // فقط وقتی عدد عوض شد بنویس (کمتر پرش)
            if (combo != lastShownCombo)
            {
                comboText.text = "x" + combo;
                comboText.ForceMeshUpdate(true);
                lastShownCombo = combo;

                if (logDebug)
                    Debug.Log("[ComboUI] text → x" + combo);
            }
        }
        else if (logDebug)
        {
            Debug.LogWarning("[ComboUI] comboText is null");
        }

        if (timerFill != null)
            timerFill.fillAmount = timer01;
    }

    private void SetVisible(bool visible)
    {
        if (visible == lastVisible)
            return;

        lastVisible = visible;

        if (comboRoot != null)
            comboRoot.SetActive(visible);

        if (!visible)
            lastShownCombo = -1;

        if (logDebug)
            Debug.Log("[ComboUI] visible=" + visible);
    }
}