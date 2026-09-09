using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

// این استیشن ظرفیت دست پلیر (تعداد آیتمی که می‌تونه هم‌زمان حمل کنه) رو با پول زیاد می‌کنه.
// دقیقاً هم‌الگوی CookingStationUpgrade، فقط به‌جای عوض کردن مدل، ظرفیت PlayerPickup رو زیاد می‌کنه.
public class PlayerUpgradeStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("شناسه‌ی یکتا (برای سیو - مثلاً \"PlayerCapacityUpgrade\")")]
    [SerializeField] private string stationId;

    [Header("هر آپگرید چقدر فضا اضافه می‌کنه (ایندکس 0 = آپگرید اول)")]
    [SerializeField] private int[] capacityIncreasePerLevel = new int[] { 1, 1, 1 };

    [Header("هزینه‌ی هر آپگرید (هم‌اندازه‌ی آرایه‌ی بالا)")]
    [SerializeField] private int[] upgradeCosts = new int[] { 100, 250, 500 };

    [Header("Stars (هر چندتا ستاره که خودت بذاری، به همون تعداد پر می‌شن)")]
    [SerializeField] private GameObject[] filledStars;
    [SerializeField] private GameObject[] emptyStars;

    [Header("Upgrade Button")]
    [SerializeField] private GameObject upgradeButton;
    [SerializeField] private Image upgradeButtonImage;
    [SerializeField] private Color normalButtonColor = Color.white;
    [SerializeField] private Color notEnoughMoneyColor = Color.red;
    [SerializeField] private float flashDuration = 0.3f;

    [Header("متن‌ها")]
    [SerializeField] private TextMeshProUGUI costText;
    [SerializeField] private TextMeshProUGUI nextCapacityText;

    [Header("Effect")]
    [SerializeField] private ParticleSystem upgradeEffect;
    [SerializeField] private AudioSource upgradeSound;

    // چندبار تا الان آپگرید شده (نه ظرفیت فعلی - اون از PlayerPickup خونده می‌شه)
    private int currentLevel = 0;
    private bool playerInside;
    private Coroutine flashRoutine;

    private void Start()
    {
        if (SaveManager.Instance != null)
            currentLevel = SaveManager.Instance.GetStationLevel(stationId, 0);

        // چون ظرفیت پلیر خودش تو Save ذخیره نمی‌شه، هر بار که صحنه لود می‌شه
        // باید همه‌ی آپگرید‌های قبلی رو دوباره روی PlayerPickup اعمال کنیم
        ApplyAllPreviousUpgrades();

        RefreshUI();
        UpdateStars();
    }

    private void ApplyAllPreviousUpgrades()
    {
        if (playerPickup == null || capacityIncreasePerLevel == null)
            return;

        for (int i = 0; i < currentLevel && i < capacityIncreasePerLevel.Length; i++)
            playerPickup.IncreaseMaxCarryCount(capacityIncreasePerLevel[i]);
    }

    private void Update()
    {
        RefreshUI();
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
    }

    public bool CanUpgrade()
    {
        return capacityIncreasePerLevel != null
            && upgradeCosts != null
            && currentLevel < capacityIncreasePerLevel.Length
            && currentLevel < upgradeCosts.Length;
    }

    private int GetCurrentCost()
    {
        if (upgradeCosts == null || currentLevel < 0 || currentLevel >= upgradeCosts.Length)
            return 0;

        return upgradeCosts[currentLevel];
    }

    private int GetCurrentIncrease()
    {
        if (capacityIncreasePerLevel == null || currentLevel < 0 || currentLevel >= capacityIncreasePerLevel.Length)
            return 0;

        return capacityIncreasePerLevel[currentLevel];
    }

    private void RefreshUI()
    {
        bool canUpgrade = CanUpgrade();

        if (upgradeButton != null)
            upgradeButton.SetActive(playerInside && canUpgrade);

        if (costText != null)
            costText.text = canUpgrade ? GetCurrentCost().ToString("N0") : "MAX";

        if (nextCapacityText != null)
        {
            if (canUpgrade && playerPickup != null)
            {
                int nextCapacity = playerPickup.MaxCarryCount + GetCurrentIncrease();
                nextCapacityText.text = "فضا میره به " + nextCapacity;
            }
            else
            {
                nextCapacityText.text = "به حداکثر رسیده";
            }
        }
    }

    // این رو به دکمه‌ی UI وصل کن (OnClick)
    public void Upgrade()
    {
        if (!CanUpgrade())
            return;

        int cost = GetCurrentCost();

        if (MoneyManager.Instance == null || !MoneyManager.Instance.TrySpend(cost))
        {
            FlashNotEnoughMoney();
            return;
        }

        int increase = GetCurrentIncrease();

        if (playerPickup != null)
            playerPickup.IncreaseMaxCarryCount(increase);

        currentLevel++;

        if (SaveManager.Instance != null)
            SaveManager.Instance.SetStationLevel(stationId, currentLevel);

        if (upgradeEffect != null)
            upgradeEffect.Play();

        if (upgradeSound != null)
            upgradeSound.Play();

        RefreshUI();
        UpdateStars();
    }

    // برخلاف کوکینگ استیشن (که عدد 3 رو تو کد ثابت کرده بود)، اینجا تعداد ستاره‌ها
    // خودکار از رو طول آرایه‌ی filledStars گرفته می‌شه - هر چندتا بذاری همون‌قدر کار می‌کنه
    private void UpdateStars()
    {
        if (filledStars == null)
            return;

        for (int i = 0; i < filledStars.Length; i++)
        {
            bool isFilled = i < currentLevel;

            if (filledStars[i] != null)
                filledStars[i].SetActive(isFilled);

            if (emptyStars != null && i < emptyStars.Length && emptyStars[i] != null)
                emptyStars[i].SetActive(!isFilled);
        }
    }

    private void FlashNotEnoughMoney()
    {
        if (upgradeButtonImage == null)
            return;

        if (flashRoutine != null)
            StopCoroutine(flashRoutine);

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine()
    {
        upgradeButtonImage.color = notEnoughMoneyColor;

        yield return new WaitForSeconds(flashDuration);

        upgradeButtonImage.color = normalButtonColor;
        flashRoutine = null;
    }
}