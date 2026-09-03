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
    // خیلی قابل‌اعتمادتر از OnApplicationQuit هست.
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

    // این رو هر وقت یه چیزی تو بازی واقعاً عوض شد صدا بزن (نه هر فریم!)
    // خودش یه تایمر کوتاه می‌ذاره تا اگه چند تغییر پشت‌سرهم اومد، یه‌جا سیو بشن
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

    // اگه یه‌جایی خواستی همین لحظه، بدون هیچ تاخیری سیو بشه (نه دیبانس)
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
    // کمک‌کننده‌های آماده برای Station Levels و Unlock Zones
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

    public bool IsZoneUnlocked(string zoneId)
    {
        return Data.unlockedZoneIds.Contains(zoneId);
    }

    public void MarkZoneUnlocked(string zoneId)
    {
        if (!Data.unlockedZoneIds.Contains(zoneId))
        {
            Data.unlockedZoneIds.Add(zoneId);
            RequestSave();
        }
    }
}