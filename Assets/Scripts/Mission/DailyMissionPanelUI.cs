using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DailyMissionPanelUI : MonoBehaviour
{
    [SerializeField] private GameObject panelRoot;

    [Header("5 جا برای کارت‌ها")]
    [SerializeField] private Transform[] slots; // دقیقاً 5 تا

    [SerializeField] private DailyMissionCardUI cardPrefab;

    private readonly List<DailyMissionCardUI> cards = new List<DailyMissionCardUI>();
    private bool rebuildQueued;

    private void OnEnable()
    {
        if (DailyMissionManager.Instance != null)
            DailyMissionManager.Instance.OnMissionsChanged += QueueRebuild;
    }

    private void OnDisable()
    {
        if (DailyMissionManager.Instance != null)
            DailyMissionManager.Instance.OnMissionsChanged -= QueueRebuild;
    }

    public void Open()
    {
        if (DailyMissionManager.Instance != null)
            DailyMissionManager.Instance.EnsureDailyMissions();

        if (panelRoot != null)
            panelRoot.SetActive(true);

        QueueRebuild();
    }

    public void Close()
    {
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

    private void QueueRebuild()
    {
        if (!isActiveAndEnabled) return;
        if (rebuildQueued) return;
        rebuildQueued = true;
        StartCoroutine(RebuildNextFrame());
    }

    private IEnumerator RebuildNextFrame()
    {
        yield return null;
        rebuildQueued = false;
        RebuildImmediate();
    }

    private void RebuildImmediate()
    {
        // پاک کردن کارت‌های قبلی
        for (int i = 0; i < cards.Count; i++)
        {
            if (cards[i] != null)
                Destroy(cards[i].gameObject);
        }
        cards.Clear();

        if (slots != null)
        {
            for (int s = 0; s < slots.Length; s++)
            {
                if (slots[s] == null) continue;
                for (int c = slots[s].childCount - 1; c >= 0; c--)
                    Destroy(slots[s].GetChild(c).gameObject);
            }
        }

        if (DailyMissionManager.Instance == null || cardPrefab == null || slots == null)
            return;

        var list = DailyMissionManager.Instance.ActiveMissions;
        int count = Mathf.Min(list.Count, slots.Length);

        for (int i = 0; i < count; i++)
        {
            if (slots[i] == null) continue;

            DailyMissionCardUI card = Instantiate(cardPrefab, slots[i]);
            card.gameObject.SetActive(true);

            RectTransform rt = card.transform as RectTransform;
            if (rt != null)
            {
                rt.anchorMin = Vector2.zero;
                rt.anchorMax = Vector2.one;
                rt.offsetMin = Vector2.zero;
                rt.offsetMax = Vector2.zero;
                rt.localScale = Vector3.one;
            }

            card.Setup(list[i]);
            cards.Add(card);
        }
    }
}