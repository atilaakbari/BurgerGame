using TMPro;
using UnityEngine;
using UnityEngine.UI;

// این اسکریپت رو روی همون Floor Decal (عکس زمینی با آیکون پول و متن قیمت) بذار.
// یه Collider با Is Trigger = true هم لازم داره که اندازه‌ی همون محدوده باشه.
public class UnlockZone : MonoBehaviour
{
    [Header("شناسه‌ی یکتا (حتماً برای هر Zone فرق کنه - مثلاً \"Zone_TableArea2\")")]
    [SerializeField] private string zoneId;

    [Header("Cost")]
    [SerializeField] private int totalCost = 500;

    [Header("چقدر طول بکشه تا کامل باز شه (اگه پول کافی پیوسته موجود باشه)")]
    [SerializeField] private float unlockDuration = 2.5f;

    [Header("UI (روی همون دکل زمینی)")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image progressFill; // اختیاری

    [Header("چیزی که باز می‌شه (دستگاه/مکان جدید - از اول باید غیرفعال باشه)")]
    [SerializeField] private GameObject[] objectsToReveal;

    [Header("چیزی که ناپدید می‌شه (خودِ این دکل + آیکون پول)")]
    [SerializeField] private GameObject[] objectsToHide;

    [Header("Effect")]
    [SerializeField] private ParticleSystem unlockEffect;
    [SerializeField] private AudioSource unlockSound;

    private int remainingCost;
    private float unlockRate;
    private float accumulator;
    private bool playerInside;
    private bool unlocked;

    private void Start()
    {
        // اگه قبلاً باز شده بود (تو یه Session قبلی)، دیگه نیازی به هیچ منطقی نیست -
        // مستقیم وضعیت بازشده رو بدون افکت/صدا اعمال کن
        if (SaveManager.Instance != null && SaveManager.Instance.IsZoneUnlocked(zoneId))
        {
            ApplyUnlockedVisualsInstant();
            return;
        }

        // وگرنه، اگه قبلاً یه مقدار پول خرجش شده بود، از همون‌جا ادامه بده نه از اول
        remainingCost = SaveManager.Instance != null
            ? SaveManager.Instance.GetZoneRemainingCost(zoneId, totalCost)
            : totalCost;

        unlockRate = unlockDuration > 0f ? totalCost / unlockDuration : totalCost;

        UpdateUI();
    }

    private void Update()
    {
        if (unlocked || !playerInside)
            return;

        TryDeductMoney();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        accumulator = 0f;
    }

    private void TryDeductMoney()
    {
        if (MoneyManager.Instance == null)
            return;

        accumulator += unlockRate * Time.deltaTime;
        bool spentAnything = false;

        while (accumulator >= 1f && remainingCost > 0)
        {
            if (!MoneyManager.Instance.TrySpend(1))
            {
                accumulator = 0f;
                break;
            }

            remainingCost--;
            accumulator -= 1f;
            spentAnything = true;
        }

        UpdateUI();

        if (remainingCost <= 0)
        {
            Unlock();
        }
        else if (spentAnything && SaveManager.Instance != null)
        {
            // پیشرفت رو ذخیره کن که اگه پلیر همینجا بازی رو ببنده، دفعه‌ی بعد از همینجا ادامه بده
            SaveManager.Instance.SetZoneProgress(zoneId, remainingCost);
        }
    }

    private void UpdateUI()
    {
        if (costText != null)
            costText.text = remainingCost.ToString();

        if (progressFill != null && totalCost > 0)
            progressFill.fillAmount = 1f - ((float)remainingCost / totalCost);
    }

    private void Unlock()
    {
        unlocked = true;

        ApplyUnlockedVisualsInstant();

        if (unlockEffect != null)
            unlockEffect.Play();

        if (unlockSound != null)
            unlockSound.Play();

        if (SaveManager.Instance != null)
            SaveManager.Instance.MarkZoneUnlocked(zoneId);
    }

    // این هم موقع باز شدن واقعی صدا زده می‌شه، هم موقع لود کردن یه Save که قبلاً باز شده بوده
    private void ApplyUnlockedVisualsInstant()
    {
        unlocked = true;

        foreach (GameObject obj in objectsToHide)
        {
            if (obj != null)
                obj.SetActive(false);
        }

        foreach (GameObject obj in objectsToReveal)
        {
            if (obj != null)
                obj.SetActive(true);
        }
    }
}