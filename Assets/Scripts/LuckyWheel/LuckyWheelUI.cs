using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LuckyWheelUI : MonoBehaviour
{
    [Header("Panel")]
    [SerializeField] private GameObject panelRoot;

    [Header("Wheel")]
    [SerializeField] private RectTransform wheelRotator; // بخشی که می‌چرخد

    [Header("Buttons")]
    [SerializeField] private Button spinButton;
    [SerializeField] private TextMeshProUGUI spinCostText; // فقط عدد یا "FREE"

    [Header("Result (اختیاری)")]
    [SerializeField] private GameObject resultRoot;
    [SerializeField] private TextMeshProUGUI resultAmountText;

    [Header("Not enough")]
    [SerializeField] private Image spinButtonImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color notEnoughColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    private Coroutine flashRoutine;
    private float baseZ;

    private void Awake()
    {
        if (wheelRotator != null)
            baseZ = wheelRotator.localEulerAngles.z;

        if (resultRoot != null)
            resultRoot.SetActive(false);

        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);
        Refresh();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (OfflineEarningsManager.Instance != null)
            OfflineEarningsManager.Instance.CheckOfflineEarnings();
    }


    public void OnClickSpin()
    {
        if (LuckyWheelManager.Instance != null)
            LuckyWheelManager.Instance.TrySpin();
    }

    public void OnNotEnoughGems()
    {
        if (spinButtonImage == null) return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(Flash());
    }

    private IEnumerator Flash()
    {
        spinButtonImage.color = notEnoughColor;
        yield return new WaitForSeconds(flashDuration);
        spinButtonImage.color = normalColor;
        flashRoutine = null;
    }

    /// <summary>چرخش تا زاویه مرکز خانه برنده (درجه، فضای UI)</summary>
    public IEnumerator PlaySpinAnimation(float targetCenterAngle, int fullRotations, float duration)
    {
        if (wheelRotator == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        if (spinButton != null)
            spinButton.interactable = false;

        float startZ = wheelRotator.localEulerAngles.z;
        // عقربه ثابت بالا فرض: چرخ می‌چرخد تا centerAngle زیر عقربه بیاید
        float endZ = startZ - (fullRotations * 360f + NormalizeAngle(targetCenterAngle));

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            // ease-out
            float eased = 1f - Mathf.Pow(1f - a, 3f);
            float z = Mathf.Lerp(startZ, endZ, eased);
            wheelRotator.localEulerAngles = new Vector3(0f, 0f, z);
            yield return null;
        }

        wheelRotator.localEulerAngles = new Vector3(0f, 0f, endZ);
    }

    private float NormalizeAngle(float a)
    {
        a %= 360f;
        if (a < 0f) a += 360f;
        return a;
    }

    public void ShowResult(WheelSlice slice)
    {
        if (resultRoot != null)
            resultRoot.SetActive(true);

        if (resultAmountText != null && slice != null)
            resultAmountText.text = slice.amount.ToString("N0");
        // نوع (پول/جم): آیکون جدا در یونیتی با دو Image و SetActive
    }

    public void HideResult()
    {
        if (resultRoot != null)
            resultRoot.SetActive(false);
    }

    public float GetWheelZ()
    {
        if (wheelRotator == null) return 0f;
        return wheelRotator.localEulerAngles.z;
    }

    public IEnumerator PlaySpinAnimationRandom(float landingZ, int fullRotations, float duration)
    {
        if (wheelRotator == null)
        {
            yield return new WaitForSeconds(duration);
            yield break;
        }

        float startZ = wheelRotator.localEulerAngles.z;
        float endZ = startZ - (fullRotations * 360f) - (landingZ % 360f);

        float t = 0f;
        while (t < duration)
        {
            t += Time.unscaledDeltaTime;
            float a = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - a, 3f);
            float z = Mathf.Lerp(startZ, endZ, eased);
            wheelRotator.localEulerAngles = new Vector3(0f, 0f, z);
            yield return null;
        }

        wheelRotator.localEulerAngles = new Vector3(0f, 0f, endZ);
    }

    public void Refresh()
    {
        var m = LuckyWheelManager.Instance;
        if (m == null) return;

        bool spinning = m.IsSpinning;
        bool free = m.HasFreeSpinToday;
        bool canPay = free ||
            (GemManager.Instance != null && GemManager.Instance.Gems >= m.GemCost);

        if (spinButton != null)
            spinButton.interactable = !spinning && canPay;

        if (spinCostText != null)
        {
            if (free)
                spinCostText.text = "FREE";
            else
                spinCostText.text = m.GemCost.ToString("N0");
        }
    }
}