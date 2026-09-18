using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    [Header("تنظیمات")]
    [SerializeField] private float minSecondsBetweenWrites = 0.5f;

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

        if (Data == null)
            Data = new GameSaveData();

        Data.EnsureLists();

        string json = JsonUtility.ToJson(Data, true);
        File.WriteAllText(SavePath, json);

        Debug.Log("Game Saved -> " + SavePath);
    }

    private void Load()
    {
        if (File.Exists(SavePath))
        {
            try
            {
                string json = File.ReadAllText(SavePath);
                Data = JsonUtility.FromJson<GameSaveData>(json);
            }
            catch (Exception e)
            {
                Debug.LogError("Save load failed, creating new save. " + e.Message);
                Data = null;
            }
        }

        if (Data == null)
            Data = new GameSaveData();

        Data.EnsureLists();

        Debug.Log("Game Loaded <- " + SavePath);
    }

    [ContextMenu("Delete Save (Reset Progress)")]
    public void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);

        Data = new GameSaveData();
        Data.EnsureLists();

        Debug.Log("Save Deleted -> " + SavePath);
    }

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
        if (Data == null) return defaultLevel;
        Data.EnsureLists();

        foreach (StationSaveEntry entry in Data.stationLevels)
        {
            if (entry != null && entry.id == stationId)
                return entry.level;
        }

        return defaultLevel;
    }

    public void SetStationLevel(string stationId, int level)
    {
        if (Data == null) return;
        Data.EnsureLists();

        foreach (StationSaveEntry entry in Data.stationLevels)
        {
            if (entry != null && entry.id == stationId)
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
        if (Data == null) return null;
        Data.EnsureLists();

        foreach (ZoneSaveEntry entry in Data.zones)
        {
            if (entry != null && entry.id == zoneId)
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
        if (Data == null) return;
        Data.EnsureLists();

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
        if (Data == null) return;
        Data.EnsureLists();

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
    // Money Piles
    // ==========================================================

    public int GetStationMoneyPile(string stationId)
    {
        if (Data == null) return 0;
        Data.EnsureLists();

        foreach (MoneyPileEntry entry in Data.moneyPiles)
        {
            if (entry != null && entry.stationId == stationId)
                return entry.amount;
        }

        return 0;
    }

    public void SetStationMoneyPile(string stationId, int amount)
    {
        if (Data == null) return;
        Data.EnsureLists();

        amount = Mathf.Max(0, amount);

        foreach (MoneyPileEntry entry in Data.moneyPiles)
        {
            if (entry != null && entry.stationId == stationId)
            {
                entry.amount = amount;
                RequestSave();
                return;
            }
        }

        Data.moneyPiles.Add(new MoneyPileEntry
        {
            stationId = stationId,
            amount = amount
        });

        RequestSave();
    }

    // ==========================================================
    // Achievements
    // ==========================================================

    public int GetTotalBurgersDelivered()
    {
        if (Data == null) return 0;
        return Mathf.Max(0, Data.totalBurgersDelivered);
    }

    public void SetTotalBurgersDelivered(int value)
    {
        if (Data == null) return;
        Data.totalBurgersDelivered = Mathf.Max(0, value);
        RequestSave();
    }

    private AchievementSaveEntry FindAchievement(string id)
    {
        if (Data == null) return null;
        Data.EnsureLists();

        foreach (AchievementSaveEntry e in Data.achievements)
        {
            if (e != null && e.id == id)
                return e;
        }

        return null;
    }

    public bool IsAchievementUnlocked(string id)
    {
        AchievementSaveEntry e = FindAchievement(id);
        return e != null && e.unlocked;
    }

    public int GetAchievementProgress(string id)
    {
        AchievementSaveEntry e = FindAchievement(id);
        return e != null ? e.progress : 0;
    }

    public void SetAchievementState(string id, bool unlocked, int progress)
    {
        if (Data == null) return;
        Data.EnsureLists();

        AchievementSaveEntry e = FindAchievement(id);

        if (e == null)
        {
            e = new AchievementSaveEntry { id = id };
            Data.achievements.Add(e);
        }

        e.unlocked = unlocked;
        e.progress = Mathf.Max(0, progress);
        RequestSave();
    }
}