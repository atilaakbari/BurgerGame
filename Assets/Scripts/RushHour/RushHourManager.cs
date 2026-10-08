using System;
using UnityEngine;

public class RushHourManager : MonoBehaviour
{
    public static RushHourManager Instance { get; private set; }

    public event Action OnRushStarted;
    public event Action OnRushEnded;
    public event Action OnRushUpdated; // هر فریم برای UI تایمر

    [Header("Duration")]
    [SerializeField] private float rushDuration = 60f;

    [Header("Cooldown — بعد از هر راش")]
    [SerializeField] private float cooldownAfterRush = 600f; // 10 دقیقه

    [Header("First rush delay from game start")]
    [SerializeField] private float firstRushMinDelay = 120f; // 2 دقیقه

    [Header("Random schedule (وقتی در کول‌داون نیستی)")]
    [SerializeField] private float checkInterval = 30f;
    [SerializeField] [Range(0f, 1f)] private float chancePerCheck = 0.25f;

    [Header("Money")]
    [SerializeField] private float moneyMultiplier = 1.45f;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private bool isRush;
    private float rushTimeLeft;
    private float cooldownLeft;
    private float timeSinceStart;
    private float checkTimer;

    public bool IsRushActive => isRush;
    public float RushTimeLeft => rushTimeLeft;
    public float RushTimeNormalized =>
        rushDuration <= 0.01f ? 0f : Mathf.Clamp01(rushTimeLeft / rushDuration);
    public float MoneyMultiplier => isRush ? moneyMultiplier : 1f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        timeSinceStart += Time.deltaTime;

        if (isRush)
        {
            rushTimeLeft -= Time.deltaTime;
            OnRushUpdated?.Invoke();

            if (rushTimeLeft <= 0f)
                EndRush();

            return;
        }

        if (cooldownLeft > 0f)
        {
            cooldownLeft -= Time.deltaTime;
            return;
        }

        // هنوز برای اولین راش زود است
        if (timeSinceStart < firstRushMinDelay)
            return;

        checkTimer += Time.deltaTime;
        if (checkTimer < checkInterval)
            return;

        checkTimer = 0f;

        if (UnityEngine.Random.value <= chancePerCheck)
            StartRush();
    }

    public void StartRush()
    {
        if (isRush)
            return;

        isRush = true;
        rushTimeLeft = rushDuration;

        if (logDebug)
            Debug.Log("[Rush] START x" + moneyMultiplier);

        OnRushStarted?.Invoke();
        OnRushUpdated?.Invoke();
    }

    private void EndRush()
    {
        isRush = false;
        rushTimeLeft = 0f;
        cooldownLeft = cooldownAfterRush;

        if (logDebug)
            Debug.Log("[Rush] END → cooldown " + cooldownAfterRush + "s");

        OnRushEnded?.Invoke();
        OnRushUpdated?.Invoke();
    }

    /// <summary>ضریب پول برای FinalizeCompletedOrder</summary>
    public float GetMoneyMultiplier()
    {
        return isRush ? moneyMultiplier : 1f;
    }

    [ContextMenu("Debug/Start Rush")]
    private void DebugStart()
    {
        cooldownLeft = 0f;
        timeSinceStart = firstRushMinDelay;
        StartRush();
    }

    [ContextMenu("Debug/End Rush")]
    private void DebugEnd()
    {
        if (isRush)
            EndRush();
    }
}