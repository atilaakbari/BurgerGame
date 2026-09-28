using System.Collections;
using UnityEngine;

public class AchievementPanelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Rows در پنل (از قبل در Hierarchy چیدی)")]
    [SerializeField] private AchievementRowUI[] rows;
    
    [Space]
    [SerializeField] private AchievementScrollController scrollController;

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    // به دکمه کنار صفحه وصل کن (OnClick)
    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        BuildAll();

        // بعد از چیده شدن محتوا، برو بالا
        if (scrollController != null)
        {
            // یک فریم بعد مطمئن‌تر است
            StartCoroutine(AlignNextFrame());
        }
    }

    public void Close()
    {
        if (scrollController != null)
            scrollController.ResetToTop();

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Toggle()
    {
        if (panelRoot != null && panelRoot.activeSelf)
            Close();
        else
            Open();
    }

    private System.Collections.IEnumerator ResetScrollNextFrame()
    {
        yield return null;
        Canvas.ForceUpdateCanvases();
        if (scrollController != null)
            scrollController.ResetToTop();
    }

    private void BuildAll()
    {
        if (rows == null) return;

        foreach (var row in rows)
        {
            if (row == null) continue;
            row.Build();
        }
    }

    // اگر پنل باز است و اچیومنت جدید گرفتی
    private void OnEnable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnUnlocked += OnAnyUnlock;
    }

    private void OnDisable()
    {
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnUnlocked -= OnAnyUnlock;
    }

    private void OnAnyUnlock(string id, string title, string desc, Sprite icon, int p, int t)
    {
        if (panelRoot != null && panelRoot.activeSelf)
        {
            foreach (var row in rows)
                if (row != null) row.Refresh();
        }
    }

    IEnumerator AlignNextFrame()
    {
        yield return null;
        foreach (var a in FindObjectsOfType<HorizontalContentAligner>())
            a.RefreshAlign();
    }
}