using TMPro;
using UnityEngine;
using UnityEngine.UI;

// این اسکریپت رو روی همون Floor Decal (عکس زمینی با آیکون پول و متن قیمت) بذار.
// یه Collider با Is Trigger = true هم لازم داره که اندازه‌ی همون محدوده باشه.
public class UnlockZone : MonoBehaviour
{
    [Header("Cost")]
    [SerializeField] private int totalCost = 500;

    [Header("چقدر طول بکشه تا کامل باز شه (اگه پول کافی پیوسته موجود باشه)")]
    [SerializeField] private float unlockDuration = 2.5f;

    [Header("UI (روی همون دکل زمینی)")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private Image progressFill; // اختیاری - اگه یه دایره/نوار پیشرفت هم داری

    [Header("چیزی که باز می‌شه (دستگاه/مکان جدید - از اول باید غیرفعال باشه)")]
    [SerializeField] private GameObject[] objectsToReveal;

    [Header("چیزی که ناپدید می‌شه (خودِ این دکل + آیکون پول)")]
    [SerializeField] private GameObject[] objectsToHide;

    [Header("Effect")]
    [SerializeField] private ParticleSystem unlockEffect;
    [SerializeField] private AudioSource unlockSound;

    private int remainingCost;
    private float unlockRate; // پول در ثانیه - از روی totalCost/unlockDuration محاسبه می‌شه
    private float accumulator;
    private bool playerInside;
    private bool unlocked;

    private void Start()
    {
        remainingCost = totalCost;
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

        // وقتی پلیر می‌ره، مقدار جمع‌شده‌ی ناقصِ این فریم‌ها رو پاک کن که دفعه‌ی بعد از صفر شروع بشه
        accumulator = 0f;
    }

    private void TryDeductMoney()
    {
        if (MoneyManager.Instance == null)
            return;

        accumulator += unlockRate * Time.deltaTime;

        // به‌جای کم کردن یه مقدار بزرگ یهویی، واحد به واحد کم می‌کنیم -
        // این باعث می‌شه اگه پول پلیر وسط راه تموم بشه، دقیقاً همون‌جا که پولش ته کشیده متوقف بشه
        while (accumulator >= 1f && remainingCost > 0)
        {
            if (!MoneyManager.Instance.TrySpend(1))
            {
                // پول پلیر تموم شده - همین‌جا صبر می‌کنیم تا دوباره پول بیاره
                accumulator = 0f;
                break;
            }

            remainingCost--;
            accumulator -= 1f;
        }

        UpdateUI();

        if (remainingCost <= 0)
            Unlock();
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

        if (unlockEffect != null)
            unlockEffect.Play();

        if (unlockSound != null)
            unlockSound.Play();
    }
}