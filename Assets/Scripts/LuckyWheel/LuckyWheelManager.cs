using System;
using System.Collections;
using UnityEngine;

public enum WheelRewardType
{
    Money,
    Gem
}

[Serializable]
public class WheelSlice
{
    public string id = "money_50";
    public WheelRewardType type = WheelRewardType.Money;
    public int amount = 50;
    [Min(0.01f)] public float weight = 1f;
    [Tooltip("زاویه مرکز این خانه روی چرخ (درجه) — برای انیمیشن")]
    public float centerAngle;
}

public class LuckyWheelManager : MonoBehaviour
{
    public static LuckyWheelManager Instance { get; private set; }

    [Header("Spin cost")]
    [SerializeField] private int gemCostPerSpin = 3;

    [Header("Slices (ترتیب روی چرخ)")]
    [SerializeField] private WheelSlice[] slices;

    [Header("Spin feel")]
    [SerializeField] private float spinDuration = 4f;
    [SerializeField] private int fullRotations = 4;

    [Header("UI")]
    [SerializeField] private LuckyWheelUI ui;

    [SerializeField] private bool logDebug = true;

    public LuckyWheelUI UI => ui;

    private bool isSpinning;

    public bool IsSpinning => isSpinning;
    public int GemCost => gemCostPerSpin;
    public WheelSlice[] Slices => slices;

    public bool HasFreeSpinToday
    {
        get
        {
            if (SaveManager.Instance == null) return false;
            string today = DateTime.Now.ToString("yyyy-MM-dd");
            string last = SaveManager.Instance.GetLastFreeWheelDate();
            return last != today;
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void Start()
    {
        if (ui != null)
            ui.Refresh();

        // اگر Daily امروز نیست، خود گردونه را باز کن
        bool dailyBlocking =
            DailyRewardManager.Instance != null &&
            DailyRewardManager.Instance.CanClaimToday;

        if (!dailyBlocking && HasFreeSpinToday && ui != null)
            ui.Open();
    }

    /// <summary>دکمه Spin</summary>
    public void TrySpin()
    {
        if (isSpinning)
            return;

        if (slices == null || slices.Length == 0)
            return;

        bool free = HasFreeSpinToday;

        if (!free)
        {
            if (GemManager.Instance == null || !GemManager.Instance.TrySpend(gemCostPerSpin))
            {
                if (ui != null)
                    ui.OnNotEnoughGems();
                return;
            }
        }
        else
        {
            if (SaveManager.Instance != null)
                SaveManager.Instance.SetLastFreeWheelDate(DateTime.Now.ToString("yyyy-MM-dd"));
        }

        // زاویه نهایی رندوم (نه ایندکس از پیش)
        float randomZ = UnityEngine.Random.Range(0f, 360f);
        StartCoroutine(SpinRoutine(randomZ, free));
    }

    private IEnumerator SpinRoutine(float randomLandingZ, bool usedFree)
    {
        isSpinning = true;

        if (ui != null)
            ui.Refresh(); // دکمه حین چرخش خاموش

        if (ui != null)
            yield return ui.PlaySpinAnimationRandom(randomLandingZ, fullRotations, spinDuration);
        else
            yield return new WaitForSeconds(spinDuration);

        // از روی زاویه نهایی بفهم کدام خانه زیر عقربه است
        int winIndex = GetSliceIndexFromWheelZ(ui != null ? ui.GetWheelZ() : randomLandingZ);
        winIndex = Mathf.Clamp(winIndex, 0, slices.Length - 1);

        WheelSlice win = slices[winIndex];
        GrantReward(win);

        if (logDebug)
            Debug.Log("[Wheel] landed index=" + winIndex + " " + win.id + " amount=" + win.amount);

        if (ui != null)
        {
            ui.ShowResult(win);
            ui.Refresh(); // مهم: دکمه و هزینه بعد از چرخش
        }

        isSpinning = false;

        if (ui != null)
            ui.Refresh();
    }

    /// <summary>
    /// عقربه ثابت بالا (زاویه 0 UI). چرخ local Z را می‌چرخانیم.
    /// خانه‌ها از 0 به ترتیب ساعت‌گرد، هر خانه 360/count درجه.
    /// </summary>
    private int GetSliceIndexFromWheelZ(float wheelZ)
    {
        int count = slices.Length;
        if (count <= 0) return 0;

        float sliceSize = 360f / count;

        // نرمال 0..360
        float z = wheelZ % 360f;
        if (z < 0f) z += 360f;

        // نقطه‌ای که زیر عقربه بالا است
        float pointerLocal = (360f - z) % 360f;

        int index = Mathf.FloorToInt(pointerLocal / sliceSize);
        if (index >= count) index = count - 1;
        return index;
    }
    private int PickWeightedIndex()
    {
        float total = 0f;
        for (int i = 0; i < slices.Length; i++)
        {
            if (slices[i] != null)
                total += Mathf.Max(0.01f, slices[i].weight);
        }

        float r = UnityEngine.Random.Range(0f, total);
        float acc = 0f;

        for (int i = 0; i < slices.Length; i++)
        {
            if (slices[i] == null) continue;
            acc += Mathf.Max(0.01f, slices[i].weight);
            if (r <= acc)
                return i;
        }

        return slices.Length - 1;
    }

    private IEnumerator SpinRoutine(int winIndex, bool usedFree)
    {
        isSpinning = true;

        WheelSlice win = slices[winIndex];
        float targetAngle = win.centerAngle;

        if (ui != null)
            yield return ui.PlaySpinAnimation(targetAngle, fullRotations, spinDuration);
        else
            yield return new WaitForSeconds(spinDuration);

        GrantReward(win);

        if (logDebug)
            Debug.Log("[Wheel] win=" + win.id + " type=" + win.type + " amount=" + win.amount + " free=" + usedFree);

        if (ui != null)
        {
            ui.ShowResult(win);
            ui.Refresh();
        }

        isSpinning = false;
    }

    private void GrantReward(WheelSlice slice)
    {
        if (slice == null) return;

        if (slice.type == WheelRewardType.Money)
        {
            if (MoneyManager.Instance != null && slice.amount > 0)
                MoneyManager.Instance.AddMoney(slice.amount);
        }
        else if (slice.type == WheelRewardType.Gem)
        {
            if (GemManager.Instance != null && slice.amount > 0)
                GemManager.Instance.AddGems(slice.amount);
        }
    }

    [ContextMenu("Debug/Reset Free Spin")]
    private void DebugResetFree()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetLastFreeWheelDate("");
        if (ui != null) ui.Refresh();
    }
}