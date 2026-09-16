using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private Animator animator;

    [Header("Animation Parameters")]
    [SerializeField] private string isWalkParameter = "IsWalk";
    [SerializeField] private string isCarryParameter = "IsCarry";

    [Header("Carry (تعداد برداشت)")]
    [Min(1)]
    [SerializeField] private int carryCapacity = 1;

    [Header("Movement (سرعت)")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.35f;

    [Header("Points")]
    [SerializeField] private Transform storagePoint;
    [SerializeField] private Transform cookingPoint;

    [Header("Carry Visual")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float carryHeightStart = 0.8f;
    [SerializeField] private float carryHeightStep = 0.15f;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.2f;

    private WorkerState currentState = WorkerState.Idle;
    private readonly List<GameObject> carriedMeat = new List<GameObject>();
    private Coroutine workerRoutine;

    private bool walkState;
    private bool carryState;

    public int CarryCapacity => carryCapacity;
    public float MoveSpeed => moveSpeed;
    public WorkerState CurrentState => currentState;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
            agent.autoBraking = true;
        }

        ForceIdle();
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

        StopAgent();
        currentState = WorkerState.Idle;
        UpdateCarryAnimation();
    }

    // =========================================================
    // MAIN LOOP
    // =========================================================
    private IEnumerator WorkerLoop()
    {
        while (true)
        {
            if (cookingStation == null || meatSpawner == null)
            {
                currentState = WorkerState.Idle;
                StopAgent();
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // دست خالی + هیچ اسلات خالی نیست → Idle و صبر
            if (carriedMeat.Count == 0 && cookingStation.GetEmptySlotCount() <= 0)
            {
                currentState = WorkerState.Waiting;
                StopAgent();
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // دست خالی + اسلات خالی هست → برو گوشت بیاور
            if (carriedMeat.Count == 0)
            {
                yield return GetMeatFromStorage();
            }

            // گوشت در دست دارد → ببر بگذار روی تابه
            if (carriedMeat.Count > 0)
            {
                yield return DeliverMeat();

                // بعد از گذاشتن، اگر دست خالی شد صبر کن تا اسلات آزاد شود
                if (carriedMeat.Count == 0)
                    yield return WaitUntilSlotFreed();
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
            cookingStation.GetEmptySlotCount()
        );

        if (amount <= 0)
        {
            ForceIdle();
            yield break;
        }

        for (int i = 0; i < amount; i++)
        {
            if (cookingStation.GetEmptySlotCount() <= carriedMeat.Count)
                break;

            GameObject meat = meatSpawner.CreateItemForWorker();
            if (meat == null)
                break;

            carriedMeat.Add(meat);
            PrepareCarriedMeat(meat, carriedMeat.Count - 1);
            UpdateCarryAnimation();

            yield return null;
        }

        // ایستاده با گوشت در دست → Carry Idle
        SetWalk(false);
        UpdateCarryAnimation();
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

            bool success = cookingStation.TryPlaceRawPattyFromWorker(meat);

            if (success)
                carriedMeat.RemoveAt(i);
            else
                break;

            yield return null;
        }

        // بعد از گذاشتن فوری انیمیشن را آپدیت کن
        StopAgent();
        SetWalk(false);
        UpdateCarryAnimation(); // اگر دست خالی شد → Idle
    }

    private IEnumerator WaitUntilSlotFreed()
    {
        currentState = WorkerState.Waiting;
        StopAgent();
        ForceIdle();

        int emptyAfterPlace = cookingStation.GetEmptySlotCount();

        while (true)
        {
            // همیشه Idle بمان وقتی منتظری و دستت خالی است
            StopAgent();
            ForceIdle();

            if (cookingStation.GetEmptySlotCount() > emptyAfterPlace)
                break;

            yield return new WaitForSeconds(checkInterval);
        }
    }

    // =========================================================
    // MOVEMENT
    // =========================================================
    private IEnumerator MoveTo(Transform target)
    {
        if (target == null || agent == null)
            yield break;

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.stoppingDistance = stoppingDistance;
        agent.SetDestination(target.position);

        SetWalk(true);
        UpdateCarryAnimation();

        while (true)
        {
            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                break;
            }

            // وسط راه انیمیشن را زنده نگه دار
            SetWalk(true);
            UpdateCarryAnimation();
            yield return null;
        }

        StopAgent();
        SetWalk(false);
        UpdateCarryAnimation();
    }

    private void StopAgent()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    // =========================================================
    // ANIMATION
    // =========================================================
    private void ForceIdle()
    {
        SetWalk(false);
        SetCarry(false);
    }

    private void UpdateCarryAnimation()
    {
        SetCarry(carriedMeat.Count > 0);
    }

    private void SetWalk(bool value)
    {
        walkState = value;

        if (animator != null)
            animator.SetBool(isWalkParameter, value);
    }

    private void SetCarry(bool value)
    {
        carryState = value;

        if (animator != null)
            animator.SetBool(isCarryParameter, value);
    }

    // =========================================================
    // CARRY VISUAL
    // =========================================================
    private void PrepareCarriedMeat(GameObject meat, int index)
    {
        if (meat == null)
            return;

        Transform parent = carryPoint != null ? carryPoint : transform;
        meat.transform.SetParent(parent, false);
        meat.transform.localPosition =
            Vector3.up * (carryHeightStart + index * carryHeightStep);
        meat.transform.localRotation = Quaternion.identity;

        Rigidbody rb = meat.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = meat.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    // =========================================================
    // UPGRADES
    // =========================================================
    public void UpgradeCapacity(int newCapacity)
    {
        carryCapacity = Mathf.Max(1, newCapacity);
    }

    public void UpgradeSpeed(float newSpeed)
    {
        if (newSpeed <= 0f)
            return;

        moveSpeed = newSpeed;

        if (agent != null)
            agent.speed = moveSpeed;
    }
}