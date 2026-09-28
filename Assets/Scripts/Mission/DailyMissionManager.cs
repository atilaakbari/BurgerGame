using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MissionObjectiveType
{
    DeliverBurgers,
    DeliverSodas,
    CookPatties,
    EarnMoney
}

[Serializable]
public class MissionDefinition
{
    public string id = "burgers_5";
    public MissionObjectiveType type = MissionObjectiveType.DeliverBurgers;
    public string title = "تحویل برگر";
    [TextArea] public string description = "۵ برگر تحویل بده";
    public Sprite icon;
    [Min(1)] public int targetAmount = 5;
    [Min(0)] public int rewardMoney = 50;
    [Min(0)] public int rewardXP = 20;
    [Min(0.01f)] public float weight = 1f;
}

public class DailyMissionManager : MonoBehaviour
{
    public static DailyMissionManager Instance { get; private set; }

    public event Action OnMissionsChanged;
    /// <summary>def, delta, newProgress, remaining</summary>
    public event Action<MissionDefinition, int, int, int> OnMissionProgress;

    [Header("Pool")]
    [SerializeField] private List<MissionDefinition> missionPool = new List<MissionDefinition>();

    [Header("Daily")]
    [SerializeField] private int missionsPerDay = 3;
    [SerializeField] private bool avoidSameTypeInOneDay = true;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    [Serializable]
    public class ActiveMission
    {
        public MissionDefinition def;
        public int progress;
        public bool completed;
        public bool rewardClaimed;

        public int Remaining =>
            def == null ? 0 : Mathf.Max(0, def.targetAmount - progress);
    }

    private readonly List<ActiveMission> active = new List<ActiveMission>();
    public IReadOnlyList<ActiveMission> ActiveMissions => active;

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
        EnsureDailyMissions();
    }

    private void OnApplicationPause(bool pause)
    {
        if (!pause) EnsureDailyMissions();
    }

    private void OnApplicationFocus(bool focus)
    {
        if (focus) EnsureDailyMissions();
    }

    public static string TodayKey()
    {
        return DateTime.Now.ToString("yyyy-MM-dd");
    }

    public void EnsureDailyMissions()
    {
        if (SaveManager.Instance == null)
            return;

        string today = TodayKey();
        string saved = SaveManager.Instance.GetDailyMissionDate();

        if (saved == today && LoadFromSave())
        {
            OnMissionsChanged?.Invoke();
            return;
        }

        RollNewDay(today);
    }

    private bool LoadFromSave()
    {
        active.Clear();
        var list = SaveManager.Instance.GetDailyMissions();
        if (list == null || list.Count == 0)
            return false;

        foreach (var entry in list)
        {
            if (entry == null || string.IsNullOrEmpty(entry.id))
                continue;

            MissionDefinition def = FindDef(entry.id);
            if (def == null)
                continue;

            active.Add(new ActiveMission
            {
                def = def,
                progress = Mathf.Clamp(entry.progress, 0, def.targetAmount),
                completed = entry.completed || entry.progress >= def.targetAmount,
                rewardClaimed = entry.rewardClaimed
            });
        }

        return active.Count > 0;
    }

    private void RollNewDay(string today)
    {
        active.Clear();
        List<MissionDefinition> picks = PickMissions(missionsPerDay);

        foreach (var def in picks)
        {
            if (def == null) continue;
            active.Add(new ActiveMission
            {
                def = def,
                progress = 0,
                completed = false,
                rewardClaimed = false
            });
        }

        SaveManager.Instance.SetDailyMissionDate(today);
        Persist();

        if (logDebug)
            Debug.Log("[Mission] New day " + today + " count=" + active.Count);

        OnMissionsChanged?.Invoke();
    }

    private List<MissionDefinition> PickMissions(int count)
    {
        var result = new List<MissionDefinition>();
        if (missionPool == null || missionPool.Count == 0)
            return result;

        var bag = new List<MissionDefinition>();
        foreach (var m in missionPool)
        {
            if (m != null && !string.IsNullOrEmpty(m.id) && m.targetAmount > 0)
                bag.Add(m);
        }

        var usedTypes = new HashSet<MissionObjectiveType>();
        var usedIds = new HashSet<string>();
        int safety = 0;

        while (result.Count < count && bag.Count > 0 && safety < 200)
        {
            safety++;
            MissionDefinition pick = PickWeighted(bag);
            if (pick == null) break;

            if (usedIds.Contains(pick.id))
            {
                bag.Remove(pick);
                continue;
            }

            if (avoidSameTypeInOneDay && usedTypes.Contains(pick.type))
            {
                bool otherExists = false;
                foreach (var b in bag)
                {
                    if (b != null && !usedIds.Contains(b.id) && !usedTypes.Contains(b.type))
                    {
                        otherExists = true;
                        break;
                    }
                }
                if (otherExists)
                {
                    bag.Remove(pick);
                    continue;
                }
            }

            result.Add(pick);
            usedIds.Add(pick.id);
            usedTypes.Add(pick.type);
            bag.Remove(pick);
        }

        return result;
    }

    private MissionDefinition PickWeighted(List<MissionDefinition> list)
    {
        float total = 0f;
        foreach (var m in list)
            total += Mathf.Max(0.01f, m.weight);

        float r = UnityEngine.Random.Range(0f, total);
        float acc = 0f;
        foreach (var m in list)
        {
            acc += Mathf.Max(0.01f, m.weight);
            if (r <= acc) return m;
        }
        return list[list.Count - 1];
    }

    private MissionDefinition FindDef(string id)
    {
        if (missionPool == null) return null;
        foreach (var m in missionPool)
        {
            if (m != null && m.id == id)
                return m;
        }
        return null;
    }

    private void Persist()
    {
        if (SaveManager.Instance == null) return;

        var list = new List<DailyMissionSaveEntry>();
        foreach (var a in active)
        {
            if (a?.def == null) continue;
            list.Add(new DailyMissionSaveEntry
            {
                id = a.def.id,
                progress = a.progress,
                completed = a.completed,
                rewardClaimed = a.rewardClaimed
            });
        }
        SaveManager.Instance.SetDailyMissions(list);
    }

    // ---------- Reports ----------

    public void ReportBurgers(int amount = 1) =>
        AddProgress(MissionObjectiveType.DeliverBurgers, amount);

    public void ReportSodas(int amount = 1) =>
        AddProgress(MissionObjectiveType.DeliverSodas, amount);

    public void ReportPattiesCooked(int amount = 1) =>
        AddProgress(MissionObjectiveType.CookPatties, amount);

    public void ReportMoneyEarned(int amount)
    {
        if (amount <= 0) return;
        if (grantRewardLock) return; // پاداش مأموریت را دوباره نشمار

        AddProgress(MissionObjectiveType.EarnMoney, amount);
    }

    private void AddProgress(MissionObjectiveType type, int amount)
    {
        if (amount <= 0) return;
        EnsureDailyMissions();

        bool any = false;
        var justFinished = new List<ActiveMission>();

        for (int i = 0; i < active.Count; i++)
        {
            ActiveMission m = active[i];
            if (m?.def == null) continue;
            if (m.def.type != type) continue;
            if (m.completed) continue;

            int before = m.progress;
            m.progress = Mathf.Min(m.def.targetAmount, m.progress + amount);
            int delta = m.progress - before;
            if (delta <= 0) continue;

            any = true;
            int remaining = Mathf.Max(0, m.def.targetAmount - m.progress);

            if (logDebug)
                Debug.Log("[Mission] +" + delta + " " + m.def.id +
                          " now=" + m.progress + "/" + m.def.targetAmount +
                          " remain=" + remaining);

            // رویداد Toast — قبل از پاداش
            try
            {
                if (MissionToastUI.Instance != null)
                {
                    if (MissionToastUI.Instance != null)
                        MissionToastUI.Instance.Enqueue(m.def.icon, delta, remaining);
                }
                else if (logDebug)
                {
                    Debug.LogWarning("[Mission] MissionToastUI.Instance is NULL");
                }

                // اختیاری: اگر جای دیگر هم گوش می‌دهد
                try
                {
                    OnMissionProgress?.Invoke(m.def, delta, m.progress, remaining);
                }
                catch (System.Exception e)
                {
                    Debug.LogException(e);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            if (m.progress >= m.def.targetAmount)
            {
                m.completed = true;
                justFinished.Add(m);
            }
        }

        if (!any)
            return;

        Persist();
        OnMissionsChanged?.Invoke();

        // پاداش یک فریم بعد — جلوگیری از کرش
        for (int i = 0; i < justFinished.Count; i++)
            StartCoroutine(GrantNextFrame(justFinished[i]));
    }

    private IEnumerator GrantNextFrame(ActiveMission m)
    {
        yield return null;
        TryGrantReward(m);
        Persist();
        OnMissionsChanged?.Invoke();
    }

    private bool grantRewardLock;

    private void TryGrantReward(ActiveMission m)
    {
        if (m == null || m.def == null) return;
        if (!m.completed || m.rewardClaimed) return;

        m.rewardClaimed = true;

        grantRewardLock = true;

        if (m.def.rewardMoney > 0 && MoneyManager.Instance != null)
            MoneyManager.Instance.AddMoney(m.def.rewardMoney);

        if (m.def.rewardXP > 0 && XPManager.Instance != null)
            XPManager.Instance.AddXP(m.def.rewardXP);

        grantRewardLock = false;

        if (logDebug)
            Debug.Log("[Mission] COMPLETE " + m.def.id);
    }

    [ContextMenu("Debug/Force New Day")]
    private void DebugForceNewDay()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SetDailyMissionDate("");
        EnsureDailyMissions();
    }
}