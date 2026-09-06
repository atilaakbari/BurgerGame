using UnityEngine;

public class LevelUnlocker : MonoBehaviour
{
    [Header("از این لول به بعد باز بشه")]
    [SerializeField] private int requiredLevel = 2;

    [Header("چیزهایی که باید باز بشن")]
    [SerializeField] private GameObject[] objectsToEnable;

    [Header("چیزهایی که باید بسته بشن (اختیاری)")]
    [SerializeField] private GameObject[] objectsToDisable;

    private void OnEnable()
    {
        XPManager.OnLevelUp += OnLevelUp;
        // اگه قبلاً لولش رسیده بود هم چک کن
        CheckLevel();
    }

    private void OnDisable()
    {
        XPManager.OnLevelUp -= OnLevelUp;
    }

    private void OnLevelUp(int newLevel)
    {
        CheckLevel();
    }

    private void CheckLevel()
    {
        if (XPManager.Instance == null) return;

        if (XPManager.Instance.Level >= requiredLevel)
            Apply();
    }

    private void Apply()
    {
        foreach (var obj in objectsToEnable)
            if (obj != null) obj.SetActive(true);

        foreach (var obj in objectsToDisable)
            if (obj != null) obj.SetActive(false);
    }
}