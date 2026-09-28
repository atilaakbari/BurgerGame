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

    private float timer;

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < spawnDelay)
            return;

        timer = 0f;
        TrySpawnCustomer();
    }

    private void TrySpawnCustomer()
    {
        if (queueManager == null)
        {
            Debug.LogError("CustomerSpawner: QueueManager is not assigned!");
            return;
        }

        if (spawnPoint == null)
        {
            Debug.LogError("CustomerSpawner: Spawn Point is not assigned!");
            return;
        }

        if (customerPrefab == null)
        {
            Debug.LogError("CustomerSpawner: Customer Prefab is not assigned!");
            return;
        }

        if (!queueManager.HasFreeSpace())
        {
            Debug.Log("Queue Full!");
            return;
        } 

        GameObject obj = Instantiate(
            customerPrefab,
            spawnPoint.position,
            spawnPoint.rotation
        );

        // ظاهر رندوم
        CustomerAppearance appearance = obj.GetComponent<CustomerAppearance>();
        if (appearance != null)
            appearance.Randomize();
        else
            Debug.LogWarning("CustomerAppearance missing on prefab.");


        Animator anim = GetComponentInChildren<Animator>();
        if (anim != null)
        {
            anim.Play(0, 0, Random.Range(0f, 1f));
            anim.speed = Random.Range(0.95f, 1.05f);
        }

        CustomerAI customer = obj.GetComponent<CustomerAI>();
        if (customer == null)
        {
            Debug.LogError("CustomerAI not found on prefab: " + customerPrefab.name);
            Destroy(obj);
            return;
        }

        customer.SetQueueManager(queueManager);
        customer.SetExitPoint(queueManager.ExitPoint);

        queueManager.AddCustomer(customer);
    }
}