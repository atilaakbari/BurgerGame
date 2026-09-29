using System;
using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    public event Action OnComboUpdated;

    [Header("Timing")]
    [SerializeField] private float comboWindow = 18f;

    [Header("Reward")]
    [SerializeField] private float bonusPerStep = 0.1f;
    [SerializeField] private float maxMultiplier = 2f;
    [SerializeField] private int maxCombo = 10;

    [SerializeField] private bool logDebug = true;

    private int currentCombo;
    private float timer;
    private bool running;

    public int CurrentCombo => currentCombo;
    public float TimeLeft => Mathf.Max(0f, timer);
    public float TimeLeftNormalized =>
        comboWindow <= 0.01f ? 0f : Mathf.Clamp01(timer / comboWindow);
    public bool IsActive => running && currentCombo > 0;

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
        if (!running)
            return;

        timer -= Time.deltaTime;
        if (timer <= 0f)
            ResetCombo();
        else
            OnComboUpdated?.Invoke(); // نوار تایمر هر فریم
    }

    /// <summary>بعد از هر تحویل موفق — ضریب همان تحویل را برمی‌گرداند</summary>
    public float RegisterSuccessfulDelivery()
    {
        currentCombo = Mathf.Min(currentCombo + 1, maxCombo);
        timer = comboWindow;
        running = true;

        float mult = GetMultiplier(currentCombo);

        if (logDebug)
            Debug.Log("[Combo] delivery → x" + currentCombo + " mult=" + mult.ToString("0.00"));

        OnComboUpdated?.Invoke();
        return mult;
    }

    public float GetMultiplier(int combo)
    {
        if (combo <= 1)
            return 1f;

        float mult = 1f + (combo - 1) * bonusPerStep;
        return Mathf.Min(mult, maxMultiplier);
    }

    public void ResetCombo()
    {
        currentCombo = 0;
        timer = 0f;
        running = false;

        if (logDebug)
            Debug.Log("[Combo] reset");

        OnComboUpdated?.Invoke();
    }

    [ContextMenu("Debug/+1")]
    private void DebugPlus()
    {
        RegisterSuccessfulDelivery();
    }

    [ContextMenu("Debug/Reset")]
    private void DebugReset()
    {
        ResetCombo();
    }
}