using UnityEngine;

public class ComboManager : MonoBehaviour
{
    public static ComboManager Instance { get; private set; }

    [Header("Settings")]
    [SerializeField] private float windowSeconds = 18f;
    [SerializeField] private float bonusPerStep = 0.1f;
    [SerializeField] private float maxMultiplier = 2f;
    [SerializeField] private int maxCombo = 10;
    [SerializeField] private bool logDebug = true;

    private int combo;
    private float timer;

    public int Combo => combo;
    public bool IsActive => combo > 0 && timer > 0f;
    public float TimerNormalized =>
        windowSeconds <= 0.01f ? 0f : Mathf.Clamp01(timer / windowSeconds);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        combo = 0;
        timer = 0f;
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }

    private void Update()
    {
        if (timer <= 0f)
            return;

        timer -= Time.deltaTime;

        if (timer <= 0f)
        {
            timer = 0f;
            combo = 0;

            if (logDebug)
                Debug.Log("[Combo] timeout → reset");
        }
    }

    /// <summary>
    /// یک بار بعد از هر تحویل موفق سفارش.
    /// ضریب پول همان تحویل را برمی‌گرداند.
    /// </summary>
    public float OnDelivered()
    {
        combo = Mathf.Min(combo + 1, maxCombo);
        timer = windowSeconds;

        float mult = GetMultiplier(combo);

        if (logDebug)
            Debug.Log("[Combo] OnDelivered x" + combo + " mult=" + mult.ToString("0.00"));

        return mult;
    }

    public float GetMultiplier(int value)
    {
        if (value <= 1)
            return 1f;

        float mult = 1f + (value - 1) * bonusPerStep;
        return Mathf.Min(mult, maxMultiplier);
    }

    [ContextMenu("Debug/OnDelivered")]
    private void DebugDeliver()
    {
        OnDelivered();
    }

    [ContextMenu("Debug/Reset")]
    private void DebugReset()
    {
        combo = 0;
        timer = 0f;
    }
}