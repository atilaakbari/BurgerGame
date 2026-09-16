using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CookWorker : MonoBehaviour
{
    public enum WorkerState
    {
        Idle,
        GoingToStorage,
        TakingMeat,
        GoingToCookingStation,
        PlacingMeat,
        Waiting
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private ItemSpawnerStation meatSpawner;
    [SerializeField] private CookingStation cookingStation;

    [Header("Carry")]
    [Min(1)]
    [SerializeField] private int carryCapacity = 1;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.5f;

    [Header("Worker Position")]
    [SerializeField] private Transform storagePoint;
    [SerializeField] private Transform cookingPoint;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.25f;

    private WorkerState currentState = WorkerState.Idle;

    private readonly System.Collections.Generic.List<GameObject> carriedMeat =
        new System.Collections.Generic.List<GameObject>();

    private Coroutine workerRoutine;

    public int CarryCapacity => carryCapacity;
    public float MoveSpeed => moveSpeed;
    public WorkerState CurrentState => currentState;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
    }

    private void Start()
    {
        StartWorker();
    }

    public void StartWorker()
    {
        if (workerRoutine != null)
            StopCoroutine(workerRoutine);

        workerRoutine = StartCoroutine(WorkerLoop());
    }

    public void StopWorker()
    {
        if (workerRoutine != null)
        {
            StopCoroutine(workerRoutine);
            workerRoutine = null;
        }

        if (agent != null)
            agent.isStopped = true;

        currentState = WorkerState.Idle;
    }

    private IEnumerator WorkerLoop()
    {
        while (true)
        {
            // اگر Cooking Station ظرفیت ندارد
            if (!HasCookingSpace())
            {
                currentState = WorkerState.Waiting;

                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // اگر چیزی همراهش نیست، برود گوشت بیاورد
            if (carriedMeat.Count == 0)
            {
                yield return StartCoroutine(GetMeatFromStorage());
            }

            // اگر گوشت دارد، آن را به Cooking Station ببرد
            if (carriedMeat.Count > 0)
            {
                yield return StartCoroutine(DeliverMeat());
            }

            yield return null;
        }
    }

    // =========================================================
    // STORAGE
    // =========================================================

    private IEnumerator GetMeatFromStorage()
    {
        currentState = WorkerState.GoingToStorage;

        yield return MoveTo(storagePoint);

        currentState = WorkerState.TakingMeat;

        int amount = Mathf.Min(
            carryCapacity,
            GetAvailableCookingSlots()
        );

        for (int i = 0; i < amount; i++)
        {
            if (!HasCookingSpace())
                break;

            GameObject meat = meatSpawner.CreateItemForWorker();

            if (meat == null)
                break;

            carriedMeat.Add(meat);

            PrepareCarriedMeat(meat);

            yield return null;
        }
    }

    // =========================================================
    // COOKING STATION
    // =========================================================

    private IEnumerator DeliverMeat()
    {
        currentState = WorkerState.GoingToCookingStation;

        yield return MoveTo(cookingPoint);

        currentState = WorkerState.PlacingMeat;

        for (int i = carriedMeat.Count - 1; i >= 0; i--)
        {
            GameObject meat = carriedMeat[i];

            if (meat == null)
            {
                carriedMeat.RemoveAt(i);
                continue;
            }

            bool success =
                cookingStation.TryPlaceRawPattyFromWorker(meat);

            if (success)
            {
                carriedMeat.RemoveAt(i);
            }
            else
            {
                break;
            }

            yield return null;
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================

    private IEnumerator MoveTo(Transform target)
    {
        if (target == null || agent == null)
            yield break;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;

        agent.SetDestination(target.position);

        while (true)
        {
            if (agent.pathPending)
            {
                yield return null;
                continue;
            }

            if (agent.remainingDistance <=
                agent.stoppingDistance)
            {
                break;
            }

            yield return null;
        }

        agent.isStopped = true;
    }

    // =========================================================
    // CHECKS
    // =========================================================

    private bool HasCookingSpace()
    {
        return GetAvailableCookingSlots() > 0;
    }

    private int GetAvailableCookingSlots()
    {
        // این مقدار فعلاً از CookingStation API گرفته می‌شود.
        // در مرحله بعد متد عمومی دقیق برای تعداد پن‌های خالی اضافه می‌کنیم.

        return 1;
    }

    // =========================================================
    // CARRY
    // =========================================================

    private void PrepareCarriedMeat(GameObject meat)
    {
        if (meat == null)
            return;

        meat.transform.SetParent(transform);

        meat.transform.localPosition =
            Vector3.up * (0.8f + carriedMeat.Count * 0.15f);

        meat.transform.localRotation =
            Quaternion.identity;

        Rigidbody rb =
            meat.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col =
            meat.GetComponent<Collider>();

        if (col != null)
            col.enabled = false;
    }

    // =========================================================
    // UPGRADES
    // =========================================================

    public void UpgradeCapacity(int newCapacity)
    {
        if (newCapacity < 1)
            return;

        carryCapacity = newCapacity;
    }

    public void UpgradeSpeed(float newSpeed)
    {
        if (newSpeed <= 0)
            return;

        moveSpeed = newSpeed;

        if (agent != null)
            agent.speed = moveSpeed;
    }
}