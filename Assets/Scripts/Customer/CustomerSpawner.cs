using UnityEngine;

public class CustomerSpawner : MonoBehaviour
{
    [Header("Customer Prefabs")]
    [SerializeField] private GameObject[] customerPrefabs;

    [Header("Spawn")]
    [SerializeField] private Transform spawnPoint;

    [Header("System")]
    [SerializeField] private QueueManager queueManager;

    [Header("Settings")]
    [SerializeField] private float spawnDelay = 5f;

    private float timer;
    private int lastCustomerIndex = -1;


    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= spawnDelay)
        {
            timer = 0f;

            TrySpawnCustomer();
        }
    }


    // =========================================================
    // SPAWN CUSTOMER
    // =========================================================

    private void TrySpawnCustomer()
    {
        // QueueManager
        if (queueManager == null)
        {
            Debug.LogError(
                "CustomerSpawner: QueueManager is not assigned!"
            );

            return;
        }


        // Spawn Point
        if (spawnPoint == null)
        {
            Debug.LogError(
                "CustomerSpawner: Spawn Point is not assigned!"
            );

            return;
        }


        // Prefabs
        if (
            customerPrefabs == null ||
            customerPrefabs.Length == 0
        )
        {
            Debug.LogError(
                "CustomerSpawner: No Customer Prefabs assigned!"
            );

            return;
        }


        // Queue Full
        if (!queueManager.HasFreeSpace())
        {
            Debug.Log("Queue Full!");

            return;
        }


        // ?????? ???
        int index =
            GetRandomCustomerIndex();


        if (
            index < 0 ||
            index >= customerPrefabs.Length
        )
        {
            Debug.LogError(
                "Invalid customer prefab index!"
            );

            return;
        }


        GameObject selectedPrefab =
            customerPrefabs[index];


        if (selectedPrefab == null)
        {
            Debug.LogError(
                "Customer Prefab at index " +
                index +
                " is NULL!"
            );

            return;
        }


        // ???? ?????
        GameObject obj =
            Instantiate(
                selectedPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );


        // ????? ????? ???
        lastCustomerIndex = index;


        // ????? CustomerAI
        CustomerAI customer =
            obj.GetComponent<CustomerAI>();


        if (customer == null)
        {
            Debug.LogError(
                "CustomerAI not found on prefab: " +
                selectedPrefab.name
            );

            Destroy(obj);

            return;
        }


        customer.SetQueueManager(queueManager);
        customer.SetExitPoint(queueManager.ExitPoint);

        queueManager.AddCustomer(customer);
    }


    // =========================================================
    // RANDOM CUSTOMER
    // =========================================================

    private int GetRandomCustomerIndex()
    {
        // ??? ?? ???
        if (customerPrefabs.Length == 1)
            return 0;


        int index;


        // ??? ???? ??? ?? ?? ????? ????
        do
        {
            index =
                Random.Range(
                    0,
                    customerPrefabs.Length
                );

        }
        while (
            index == lastCustomerIndex
        );


        return index;
    }
}