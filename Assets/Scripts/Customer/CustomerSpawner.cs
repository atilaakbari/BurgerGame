using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Single Customer Prefab")]
    [SerializeField] private GameObject customerPrefab;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("System")]
    [SerializeField] private QueueManager queueManager;

    [Header("Settings")]
    [SerializeField] private float spawnDelay = 5f;

    [Header("VIP")]
    [SerializeField] [Range(0f, 1f)] private float vipChance = 0.12f;
    [SerializeField] private float vipCooldown = 180f;      // 3 دقیقه بین VIPها
    [SerializeField] private float vipFirstDelay = 90f;     // از شروع بازی
    [SerializeField] private float vipMoneyMultiplier = 1.5f;
    [SerializeField] private float vipXpMultiplier = 1.5f;
    [SerializeField] private float vipPatienceBonus = 20f;  // ثانیه اضافه

    private float timer;
    private float timeSinceStart;
    private float vipCooldownLeft;

    private void Update()
    {
        timeSinceStart += Time.deltaTime;

        if (vipCooldownLeft > 0f)
            vipCooldownLeft -= Time.deltaTime;

        timer += Time.deltaTime;
        if (timer < spawnDelay)
            return;

        timer = 0f;
        TrySpawnCustomer();
    }

    private void TrySpawnCustomer()
    {
        if (queueManager == null || spawnPoint == null || customerPrefab == null)
            return;

        if (!queueManager.HasFreeSpace())
            return;

        GameObject obj = Instantiate(
            customerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerAppearance appearance = obj.GetComponent<CustomerAppearance>();
        if (appearance != null)
            appearance.Randomize();

        // انیمیشن روی خود مشتری، نه روی Spawner
        Animator anim = obj.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.Play(0, 0, Random.Range(0f, 1f));
            anim.speed = Random.Range(0.95f, 1.05f);
        }

        CustomerAI customer = obj.GetComponent<CustomerAI>();
        if (customer == null)
        {
            Destroy(obj);
            return;
        }

        customer.SetQueueManager(queueManager);
        customer.SetExitPoint(queueManager.ExitPoint);

        // VIP؟
        if (ShouldSpawnVIP())
        {
            customer.SetAsVIP(vipMoneyMultiplier, vipXpMultiplier, vipPatienceBonus);
            vipCooldownLeft = vipCooldown;
            Debug.Log("[VIP] Customer spawned");
        }

        queueManager.AddCustomer(customer);
    }

    private bool ShouldSpawnVIP()
    {
        if (timeSinceStart < vipFirstDelay)
            return false;

        if (vipCooldownLeft > 0f)
            return false;

        return Random.value < vipChance;
    }

    [ContextMenu("Debug/Spawn VIP Customer")]
    private void DebugSpawnVIP()
    {
        if (queueManager == null || spawnPoint == null || customerPrefab == null)
        {
            Debug.LogError("[VIP Debug] Spawner fields missing");
            return;
        }

        if (!queueManager.HasFreeSpace())
        {
            Debug.LogWarning("[VIP Debug] Queue full");
            return;
        }

        GameObject obj = Instantiate(
            customerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        CustomerAppearance appearance = obj.GetComponent<CustomerAppearance>();
        if (appearance != null)
            appearance.Randomize();

        Animator anim = obj.GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.Play(0, 0, Random.Range(0f, 1f));
            anim.speed = Random.Range(0.95f, 1.05f);
        }

        CustomerAI customer = obj.GetComponent<CustomerAI>();
        if (customer == null)
        {
            Destroy(obj);
            Debug.LogError("[VIP Debug] CustomerAI missing");
            return;
        }

        customer.SetQueueManager(queueManager);
        customer.SetExitPoint(queueManager.ExitPoint);

        customer.SetAsVIP(vipMoneyMultiplier, vipXpMultiplier, vipPatienceBonus);

        queueManager.AddCustomer(customer);

        Debug.Log("[VIP Debug] VIP customer spawned");
    }
}