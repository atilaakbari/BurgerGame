using UnityEngine;

public class CustomerAppearance : MonoBehaviour
{
    [System.Serializable]
    public class Outfit
    {
        [Tooltip("اسم ست برای خودت")]
        public string outfitName = "Worker";

        [Tooltip("همه قطعه‌هایی که با این ست روشن می‌شوند (لباس + کلاه + کفش + ...)")]
        public GameObject[] enableParts;

        [Tooltip("وزن شانس انتخاب این ست (بزرگ‌تر = احتمال بیشتر)")]
        [Min(0f)]
        public float weight = 1f;
    }

    [Header("همه قطعات قابل تعویض (برای خاموش کردن اولیه)")]
    [SerializeField] private GameObject[] allParts;

    [Header("ست‌های آماده (ترکیب‌های مجاز)")]
    [SerializeField] private Outfit[] outfits;

    [Header("اختیاری: رنگ پوست")]
    [SerializeField] private Renderer[] skinRenderers;
    [SerializeField] private Color[] skinColors;


    public void Randomize()
    {
        DisableAllParts();

        Outfit chosen = PickOutfitNoRepeat();
        if (chosen == null)
        {
            Debug.LogWarning("CustomerAppearance: no outfits");
            return;
        }

        if (chosen.enableParts != null)
        {
            for (int i = 0; i < chosen.enableParts.Length; i++)
            {
                if (chosen.enableParts[i] != null)
                    chosen.enableParts[i].SetActive(true);
            }
        }

        ApplyRandomSkin();
    }

    private void DisableAllParts()
    {
        if (allParts == null) return;

        for (int i = 0; i < allParts.Length; i++)
        {
            if (allParts[i] != null)
                allParts[i].SetActive(false);
        }
    }

    private Outfit PickWeightedOutfit()
    {
        if (outfits == null || outfits.Length == 0)
            return null;

        float total = 0f;
        for (int i = 0; i < outfits.Length; i++)
        {
            if (outfits[i] != null)
                total += Mathf.Max(0f, outfits[i].weight);
        }

        if (total <= 0f)
            return outfits[Random.Range(0, outfits.Length)];

        float r = Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < outfits.Length; i++)
        {
            if (outfits[i] == null) continue;
            acc += Mathf.Max(0f, outfits[i].weight);
            if (r <= acc)
                return outfits[i];
        }

        return outfits[outfits.Length - 1];
    }

    private void ApplyRandomSkin()
    {
        if (skinRenderers == null || skinRenderers.Length == 0) return;
        if (skinColors == null || skinColors.Length == 0) return;

        Color c = skinColors[Random.Range(0, skinColors.Length)];

        for (int i = 0; i < skinRenderers.Length; i++)
        {
            if (skinRenderers[i] == null) continue;

            var mats = skinRenderers[i].materials;
            if (mats == null || mats.Length == 0) continue;

            if (mats[0].HasProperty("_Color"))
                mats[0].color = c;
            else if (mats[0].HasProperty("_BaseColor"))
                mats[0].SetColor("_BaseColor", c);

            skinRenderers[i].materials = mats;
        }
    }

    [ContextMenu("Disable All Parts")]
    private void EditorDisableAll()
    {
        DisableAllParts();
    }

    // داخل کلاس CustomerAppearance:

    // ایندکس ست‌هایی که هنوز در این دور نیامده‌اند
    private static System.Collections.Generic.List<int> remainingOutfitIndices =
        new System.Collections.Generic.List<int>();

    private static int cachedOutfitCount = -1;


    private Outfit PickOutfitNoRepeat()
    {
        if (outfits == null || outfits.Length == 0)
            return null;

        // اگر تعداد ست‌ها عوض شد، کیسه را از نو بساز
        if (cachedOutfitCount != outfits.Length || remainingOutfitIndices.Count == 0)
            RefillBag();

        int listIndex = Random.Range(0, remainingOutfitIndices.Count);
        int outfitIndex = remainingOutfitIndices[listIndex];
        remainingOutfitIndices.RemoveAt(listIndex);

        // اگر کیسه خالی شد، دور بعد پر می‌شود
        return outfits[outfitIndex];
    }

    private void RefillBag()
    {
        remainingOutfitIndices.Clear();
        cachedOutfitCount = outfits.Length;

        for (int i = 0; i < outfits.Length; i++)
        {
            if (outfits[i] == null)
                continue;

            // وزن: چند بار در کیسه باشد (حداقل 1)
            int copies = Mathf.Max(1, Mathf.RoundToInt(outfits[i].weight));
            for (int c = 0; c < copies; c++)
                remainingOutfitIndices.Add(i);
        }

        // اگر همه null بودند
        if (remainingOutfitIndices.Count == 0)
        {
            for (int i = 0; i < outfits.Length; i++)
                remainingOutfitIndices.Add(i);
        }
    }
}