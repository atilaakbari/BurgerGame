using System.Collections;
using System.IO;
using UnityEngine;

// این رو یه‌بار روی یه GameObject خالی تو اولین صحنه‌ی بازی بذار (مثلاً یه آبجکت به اسم "Managers").
public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("تنظیمات")]
    [SerializeField] private float minSecondsBetweenWrites = 0.5f; // جلوی نوشتن مکرر رو دیسک رو می‌گیره

    private string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

    public GameSaveData Data { get; private set; }

    private Coroutine pendingSaveRoutine;
    private bool isDirty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        Load();
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveImmediately();
    }

    private void OnApplicationQuit()
    {
        SaveImmediately();
    }

    // ==========================================================
    // API عمومی
    // ==========================================================

    public void RequestSave()
    {
        isDirty = true;

        if (pendingSaveRoutine == null)
            pendingSaveRoutine = StartCoroutine(DebouncedSaveRoutine());
    }

    private IEnumerator DebouncedSaveRoutine()
    {
        yield return new WaitForSeconds(minSecondsBetweenWrites);

        if (isDirty)
            SaveImmediately();

        pendingSaveRoutine = null;
    }

    public void SaveImmediately()
    {
        isDirty = false;

        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game Saved -> " + SavePath);
    }

    private void Load()
    {
        if (File.Exists(SavePath))
        {
            string json = File.ReadAllText(SavePath);
            Data = JsonUtility.FromJson<GameSaveData>(json);
        }

        if (Data == null)
            Data = new GameSaveData();

        Debug.Log("Game Loaded <- " + SavePath);
    }

    // ==========================================================
    // پاک کردن سیو (برای تست یا دکمه‌ی "شروع مجدد بازی")
    // ==========================================================

    // رو Inspector، رو کامپوننت SaveManager راست‌کلیک کن (یا رو سه‌نقطه‌ی بالای کامپوننت بزن)،
    // گزینه‌ی "Delete Save (Reset Progress)" رو می‌بینی - هم تو Play Mode هم تو Editor کار می‌کنه.
    [ContextMenu("Delete Save (Reset Progress)")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        Data = new GameSaveData();

        Debug.Log("Save Deleted -> " + SavePath);

        // نکته: آبجکت‌هایی که از قبل تو صحنه هستن مقدار قدیمی رو تو Start خودشون
        // خونده بودن و خودکار عوض نمی‌شن؛ اگه می‌خوای همون لحظه همه‌چی صفر بشه،
        // بعد از این متد صحنه رو Reload کن.
    }

    // اگه از کد (مثلاً دکمه‌ی "Reset" تو تنظیمات بازی) می‌خوای صدا بزنی و صحنه هم ریست بشه
    public void DeleteSaveAndReloadScene()
    {
        DeleteSave();
        UnityEngine.SceneManagement.SceneManager.LoadScene(
            UnityEngine.SceneManagement.SceneManager.GetActiveScene().buildIndex
        );
    }

    // ==========================================================
    // Station Levels
    // ==========================================================

    public int GetStationLevel(string stationId, int defaultLevel)
    {
        foreach (StationSaveEntry entry in Data.stationLevels)
        {
            if (entry.id == stationId)
                return entry.level;
        }

        return defaultLevel;
    }

    public void SetStationLevel(string stationId, int level)
    {
        foreach (StationSaveEntry entry in Data.stationLevels)
        {
            if (entry.id == stationId)
            {
                entry.level = level;
                RequestSave();
                return;
            }
        }

        Data.stationLevels.Add(new StationSaveEntry { id = stationId, level = level });
        RequestSave();
    }

    // ==========================================================
    // Unlock Zones
    // ==========================================================

    private ZoneSaveEntry FindZone(string zoneId)
    {
        foreach (ZoneSaveEntry entry in Data.zones)
        {
            if (entry.id == zoneId)
                return entry;
        }

        return null;
    }

    public bool IsZoneUnlocked(string zoneId)
    {
        ZoneSaveEntry entry = FindZone(zoneId);
        return entry != null && entry.unlocked;
    }

    public int GetZoneRemainingCost(string zoneId, int defaultCost)
    {
        ZoneSaveEntry entry = FindZone(zoneId);

        if (entry != null && !entry.unlocked)
            return entry.remainingCost;

        return defaultCost;
    }

    public void SetZoneProgress(string zoneId, int remainingCost)
    {
        ZoneSaveEntry entry = FindZone(zoneId);

        if (entry == null)
        {
            entry = new ZoneSaveEntry { id = zoneId };
            Data.zones.Add(entry);
        }

        entry.unlocked = false;
        entry.remainingCost = remainingCost;

        RequestSave();
    }

    public void MarkZoneUnlocked(string zoneId)
    {
        ZoneSaveEntry entry = FindZone(zoneId);

        if (entry == null)
        {
            entry = new ZoneSaveEntry { id = zoneId };
            Data.zones.Add(entry);
        }

        entry.unlocked = true;
        entry.remainingCost = 0;

        RequestSave();
    }

    // ==========================================================
    // Money Piles (پول‌های نقدیِ رو زمین کنار ایستگاه‌ها)
    // ==========================================================

    public int GetStationMoneyPile(string stationId)
    {
        foreach (MoneyPileEntry entry in Data.moneyPiles)
        {
            if (entry.stationId == stationId)
                return entry.amount;
        }

        return 0;
    }

    public void SetStationMoneyPile(string stationId, int amount)
    {
        foreach (MoneyPileEntry entry in Data.moneyPiles)
        {
            if (entry.stationId == stationId)
            {
                entry.amount = amount;
                RequestSave();
                return;
            }
        }

        Data.moneyPiles.Add(new MoneyPileEntry { stationId = stationId, amount = amount });
        RequestSave();
    }
}