using TMPro;
using UnityEngine;

public class GemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI gemText;

    private void OnEnable()
    {
        GemManager.OnGemsChanged += UpdateText;

        if (GemManager.Instance != null)
            UpdateText(GemManager.Instance.Gems);
    }

    private void OnDisable()
    {
        GemManager.OnGemsChanged -= UpdateText;
    }

    private void UpdateText(int amount)
    {
        if (gemText == null)
            return;

        gemText.text = amount.ToString("N0");
    }
}