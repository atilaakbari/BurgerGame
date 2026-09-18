using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BurgerDeliveredCounterUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TextMeshProUGUI tmpText; // اگر TextMeshPro داری
    [SerializeField] private Text uiText;           // اگر UI Text قدیمی داری

    [Header("Format")]
    [SerializeField] private string format = "Burgers: {0}";

    private void OnEnable()
    {
        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        // ساده و مطمئن — هر فریم از سیو می‌خواند
        Refresh();
    }

    public void Refresh()
    {
        int count = 0;

        if (SaveManager.Instance != null)
            count = SaveManager.Instance.GetTotalBurgersDelivered();

        string msg = string.Format(format, count);

        if (tmpText != null)
            tmpText.text = msg;

        if (uiText != null)
            uiText.text = msg;
    }
}