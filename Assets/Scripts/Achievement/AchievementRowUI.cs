using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class AchievementRowUI : MonoBehaviour
{
    [Header("Category")]
    [SerializeField] private AchievementCategory category;
    [SerializeField] private string rowTitle = "Burgers";

    [Header("UI")]
    [SerializeField] private TextMeshProUGUI rowTitleText;
    [SerializeField] private AchievementNodeUI nodePrefab;
    [SerializeField] private GameObject arrowPrefab;

    [SerializeField] private Transform contentParent;
    private readonly List<AchievementNodeUI> nodes = new List<AchievementNodeUI>();

    public AchievementCategory Category => category;

    public void Build()
    {
        /*contentParent = transform.Find("ContentParent");

        if (contentParent == null)
        {
            Debug.LogError(name + ": فرزند ContentParent پیدا نشد.", this);
            return;
        }

        if (contentParent.parent != transform)
        {
            Debug.LogError(name + ": ContentParent باید فرزند مستقیم این Row باشد.", this);
            return;
        }*/

        Clear();

        if (rowTitleText != null)
            rowTitleText.text = rowTitle;

        if (AchievementManager.Instance == null)
        {
            Debug.LogError("AchievementManager.Instance is null");
            return;
        }

        if (nodePrefab == null)
        {
            Debug.LogError(name + ": nodePrefab is null");
            return;
        }

        List<AchievementManager.AchievementInfo> list =
            AchievementManager.Instance.GetByCategory(category);

        // آخری‌ها سمت چپ، اولی‌ها سمت راست
        list.Sort((a, b) => b.chainOrder.CompareTo(a.chainOrder));

        for (int i = 0; i < list.Count; i++)
        {
            AchievementNodeUI node = Instantiate(nodePrefab, contentParent);
            node.transform.SetParent(contentParent, false);
            node.gameObject.SetActive(true);
            node.Setup(list[i]);
            nodes.Add(node);

            if (i < list.Count - 1 && arrowPrefab != null)
            {
                GameObject arrow = Instantiate(arrowPrefab, contentParent);
                arrow.transform.SetParent(contentParent, false);
                arrow.SetActive(true);
            }
        }

        // ... بعد از ساخت همه نود و فلش ...

        Canvas.ForceUpdateCanvases();

        HorizontalContentAligner aligner =
            contentParent.GetComponent<HorizontalContentAligner>();

        if (aligner == null)
            aligner = contentParent.GetComponentInParent<HorizontalContentAligner>();

        if (aligner != null)
            aligner.RefreshAlign();
    }

    public void Refresh()
    {
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i] != null)
                nodes[i].Refresh();
        }
    }

    private void Clear()
    {
        nodes.Clear();

        if (contentParent == null)
            return;

        for (int i = contentParent.childCount - 1; i >= 0; i--)
            Destroy(contentParent.GetChild(i).gameObject);
    }
}