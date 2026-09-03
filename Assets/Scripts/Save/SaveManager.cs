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

    // رو موبایل، این دقیقاً همون لحظه‌ایه که پلیر داره از بازی خارج می‌شه -
    // چه با دکمه‌ی Home بره بیرون، چه سیستم عامل اپ رو Kill کنه.
    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
            SaveImmediately();
    }

    // برای حالت PC / دسکتاپ
    private void OnApplicationQuit()
    {
        SaveImmediately();
    }

    // ==========================================================
    // API عمومی - بقیه‌ی اسکریپت‌ها فقط این‌ها رو صدا می‌زنن
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
    // Unlock Zones (هم وضعیت باز/بسته، هم پیشرفتِ پرداخت ناقص)
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

    // اگه هنوز باز نشده و قبلاً یه مقدار پول خرجش شده، همون مقدار باقی‌مونده رو برمی‌گردونه
    public int GetZoneRemainingCost(string zoneId, int defaultCost)
    {
        ZoneSaveEntry entry = FindZone(zoneId);

        if (entry != null && !entry.unlocked)
            return entry.remainingCost;

        return defaultCost;
    }

    // هر بار که پیشرفتِ پرداخت عوض شد (نه هر فریم لزوماً، ولی این تابع خودش دیبانس داره)
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
}