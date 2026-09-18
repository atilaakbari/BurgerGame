using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CutWorker : MonoBehaviour
{
    public enum WorkerState
    {
        Idle,
        GoingToStorage,
        TakingItem,
        GoingToCuttingStation,
        PlacingItem,
        Waiting
    }

    [Serializable]
    public class IngredientSource
    {
        public ItemType type;                 // خام: Lettuce, Tomato, ...
        public ItemSpawnerStation spawner;
        public Transform standPoint;
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private CuttingStation cuttingStation;
    [SerializeField] private QueueManager queueManager;
    [SerializeField] private Animator animator;

    [Header("منابع مواد خام (Lettuce / Tomato / Onion / Cheese)")]
    [SerializeField] private IngredientSource[] ingredientSources;

    [Header("Animation")]
    [SerializeField] private string isWalkParameter = "IsWalk";
    [SerializeField] private string isCarryParameter = "IsCarry";

    [Header("Carry")]
    [Min(1)]
    [SerializeField] private int carryCapacity = 1;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.35f;

    [Header("Points")]
    [SerializeField] private Transform cuttingPoint;

    [Header("Carry Visual")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float carryHeightStart = 0.8f;
    [SerializeField] private float carryHeightStep = 0.15f;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.25f;

    private WorkerState currentState = WorkerState.Idle;
    private readonly List<GameObject> carriedItems = new List<GameObject>();
    private Coroutine workerRoutine;

    public int CarryCapacity => carryCapacity;
    public float MoveSpeed => moveSpeed;
    public WorkerState CurrentState => currentState;

    private CustomerAI trackedCustomer;
    private BurgerOrder trackedOrder;
    private readonly HashSet<ItemType> alreadyHandledInputs = new HashSet<ItemType>();

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();

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
    // MAIN LOOP — مشتری دیر اسپان شود هم صبر می‌کند
    // =========================================================
    private IEnumerator WorkerLoop()
    {
        while (true)
        {
            if (cuttingStation == null || queueManager == null)
            {
                currentState = WorkerState.Waiting;
                StopAgent();
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // اسلات خالی نیست و دست خالی
            if (carriedItems.Count == 0 && cuttingStation.GetEmptySlotCount() <= 0)
            {
                currentState = WorkerState.Waiting;
                StopAgent();
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            if (carriedItems.Count == 0)
            {
                List<ItemType> needed = GetNeededCutInputsFromOrder();

                // هنوز مشتری / سفارش / آیتم برش‌پذیر نیست → صبر (مثلاً قبل از اسپان)
                if (needed.Count == 0)
                {
                    currentState = WorkerState.Waiting;
                    StopAgent();
                    ForceIdle();
                    yield return new WaitForSeconds(checkInterval);
                    continue;
                }

                yield return CollectItems(needed);
            }

            if (carriedItems.Count > 0)
            {
                yield return DeliverItems();

                if (carriedItems.Count == 0)
                    yield return WaitUntilSlotFreed();
            }

            yield return null;
        }
    }

    // =========================================================
    // سفارش از پایین به بالا → تبدیل Cut به Input خام
    // BunBottem / BunTop / CookedPatty و ... رد می‌شوند
    // =========================================================
    private List<ItemType> GetNeededCutInputsFromOrder()
    {
        List<ItemType> result = new List<ItemType>();

        RefreshOrderTracking();

        if (trackedCustomer == null || trackedOrder == null)
            return result;

        List<ItemType> orderItems = trackedOrder.items;
        if (orderItems == null || orderItems.Count == 0)
            return result;

        // از پایین به بالا
        for (int i = 0; i < orderItems.Count; i++)
        {
            ItemType orderType = orderItems[i];
            ItemType inputType = cuttingStation.GetCutInputFromOrderItem(orderType);

            // نون، پتی، و ... 
            if (inputType == ItemType.None)
                continue;

            // قبلاً برای این سفارش آورده و گذاشته → دیگر نیاور
            if (alreadyHandledInputs.Contains(inputType))
                continue;

            // در همین لیست این سفر تکراری نباشد
            if (result.Contains(inputType))
                continue;

            if (!cuttingStation.CanCut(inputType))
                continue;

            if (FindSource(inputType) == null)
                continue;

            result.Add(inputType);
        }

        return result;
    }

    // =========================================================
    private IEnumerator CollectItems(List<ItemType> needed)
    {
        int maxTake = Mathf.Min(
            carryCapacity,
            cuttingStation.GetEmptySlotCount(),
            needed.Count
        );

        if (maxTake <= 0)
            yield break;

        int taken = 0;

        for (int i = 0; i < needed.Count && taken < maxTake; i++)
        {
            if (cuttingStation.GetEmptySlotCount() <= carriedItems.Count)
                break;

            ItemType type = needed[i];
            IngredientSource source = FindSource(type);

            if (source == null || source.spawner == null)
                continue;

            currentState = WorkerState.GoingToStorage;

            Transform stand = source.standPoint != null
                ? source.standPoint
                : source.spawner.transform;

            yield return MoveTo(stand);

            currentState = WorkerState.TakingItem;

            GameObject item = source.spawner.CreateItemForWorker();
            if (item == null)
                continue;

            Item itemData = item.GetComponent<Item>();
            if (itemData == null || !cuttingStation.CanCut(itemData.Type))
            {
                Destroy(item);
                continue;
            }

            carriedItems.Add(item);
            PrepareCarriedItem(item, carriedItems.Count - 1);
            UpdateCarryAnimation();
            taken++;

            yield return null;
        }

        SetWalk(false);
        UpdateCarryAnimation();
    }

    private IngredientSource FindSource(ItemType type)
    {
        if (ingredientSources == null) return null;

        for (int i = 0; i < ingredientSources.Length; i++)
        {
            if (ingredientSources[i] != null &&
                ingredientSources[i].type == type)
                return ingredientSources[i];
        }
        return null;
    }

    private IEnumerator DeliverItems()
    {
        currentState = WorkerState.GoingToCuttingStation;
        yield return MoveTo(cuttingPoint);

        currentState = WorkerState.PlacingItem;

        for (int i = carriedItems.Count - 1; i >= 0; i--)
        {
            GameObject item = carriedItems[i];
            if (item == null)
            {
                carriedItems.RemoveAt(i);
                continue;
            }

            Item itemData = item.GetComponent<Item>();
            ItemType placedType = itemData != null ? itemData.Type : ItemType.None;

            bool success = cuttingStation.TryPlaceItemFromWorker(item);

            if (success)
            {
                carriedItems.RemoveAt(i);

                // این ورودی برای این سفارش دیگر تکرار نشود
                if (placedType != ItemType.None)
                    alreadyHandledInputs.Add(placedType);
            }
            else
            {
                break;
            }

            yield return null;
        }

        StopAgent();
        SetWalk(false);
        UpdateCarryAnimation();
    }

    private IEnumerator WaitUntilSlotFreed()
    {
        currentState = WorkerState.Waiting;
        StopAgent();
        ForceIdle();

        int emptyAfter = cuttingStation.GetEmptySlotCount();

        while (true)
        {
            ForceIdle();
            if (cuttingStation.GetEmptySlotCount() > emptyAfter)
                break;

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private IEnumerator MoveTo(Transform target)
    {
        if (target == null || agent == null)
            yield break;

        if (!agent.enabled) agent.enabled = true;

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
                break;

            yield return null;
        }

        StopAgent();
        SetWalk(false);
        UpdateCarryAnimation();
    }

    private void StopAgent()
    {
        if (agent == null || !agent.enabled) return;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    private void ForceIdle()
    {
        SetWalk(false);
        SetCarry(false);
    }

    private void UpdateCarryAnimation()
    {
        SetCarry(carriedItems.Count > 0);
    }

    private void SetWalk(bool value)
    {
        if (animator != null)
            animator.SetBool(isWalkParameter, value);
    }

    private void SetCarry(bool value)
    {
        if (animator != null)
            animator.SetBool(isCarryParameter, value);
    }

    private void PrepareCarriedItem(GameObject item, int index)
    {
        if (item == null) return;

        Transform parent = carryPoint != null ? carryPoint : transform;
        item.transform.SetParent(parent, false);
        item.transform.localPosition =
            Vector3.up * (carryHeightStart + index * carryHeightStep);
        item.transform.localRotation = Quaternion.identity;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = item.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void UpgradeCapacity(int newCapacity)
    {
        carryCapacity = Mathf.Max(1, newCapacity);
    }

    public void UpgradeSpeed(float newSpeed)
    {
        if (newSpeed <= 0f) return;
        moveSpeed = newSpeed;
        if (agent != null) agent.speed = moveSpeed;
    }

    private void RefreshOrderTracking()
    {
        CustomerAI customer = queueManager != null
            ? queueManager.GetFirstCustomer()
            : null;

        BurgerOrder order = customer != null ? customer.CurrentOrder : null;

        if (customer != trackedCustomer || order != trackedOrder)
        {
            trackedCustomer = customer;
            trackedOrder = order;
            alreadyHandledInputs.Clear();
        }
    }

    private void OnEnable()
    {
        GameFoodEvents.OnItemTrashed += OnItemTrashed;
    }

    private void OnDisable()
    {
        GameFoodEvents.OnItemTrashed -= OnItemTrashed;
    }

    private void OnItemTrashed(ItemType trashedType)
    {
        // Lettuce_Cut → دوباره Lettuce باید کات شود
        ItemType input = trashedType;

        if (trashedType == ItemType.Lettuce_Cut) input = ItemType.Lettuce;
        else if (trashedType == ItemType.Tomato_Cut) input = ItemType.Tomato;
        else if (trashedType == ItemType.Onion_Cut) input = ItemType.Onion;
        else if (trashedType == ItemType.Cheese_Cut) input = ItemType.Cheese;

        if (alreadyHandledInputs.Contains(input))
        {
            alreadyHandledInputs.Remove(input);
            Debug.Log("CutWorker: will recut " + input);
        }
    }
}