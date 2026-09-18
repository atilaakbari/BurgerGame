using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AssembleWorker : MonoBehaviour
{
    public enum WorkerState
    {
        Idle,
        WaitingIngredients,
        WaitingPlayer,
        GoingToSource,
        TakingItem,
        GoingToAssembly,
        PlacingItem,
        TakingBurger,
        GoingToSoda,
        GoingToDelivery,
        Delivering,
        GoingToTrash
    }

    [Serializable]
    public class ItemSource
    {
        public ItemType type;
        public ItemSpawnerStation spawner;
        public Transform standPoint;
    }

    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private QueueManager queueManager;
    [SerializeField] private BurgerAssemblyStation assemblyStation;
    [SerializeField] private CookingStation cookingStation;
    [SerializeField] private CuttingStation cuttingStation;
    [SerializeField] private DeliveryStation deliveryStation;
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Sources")]
    [SerializeField] private ItemSource bottomBunSource;
    [SerializeField] private ItemSource topBunSource;
    [SerializeField] private ItemSource sodaSource;

    [Header("Points")]
    [SerializeField] private Transform assemblyPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private Transform cookingPoint;
    [SerializeField] private Transform cuttingPoint;
    [SerializeField] private Transform trashPoint;

    [Header("Animation")]
    [SerializeField] private string isWalkParameter = "IsWalk";
    [SerializeField] private string isCarryParameter = "IsCarry";

    [Header("Carry / Speed")]
    [Min(1)]
    [SerializeField] private int carryCapacity = 1;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.35f;
    [SerializeField] private float burgerCarryScale = 0.45f;

    [Header("Carry Visual")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float carryHeightStart = 0.8f;
    [SerializeField] private float carryHeightStep = 0.15f;

    [Header("Settings")]
    [SerializeField] private float checkInterval = 0.25f;

    private WorkerState currentState = WorkerState.Idle;
    private readonly List<GameObject> carried = new List<GameObject>();
    private GameObject carriedBurger;
    private Coroutine loop;

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

        SetWalk(false);
        SetCarry(false);
    }

    private void Start()
    {
        StartWorker();
    }

    public void StartWorker()
    {
        if (loop != null)
            StopCoroutine(loop);

        loop = StartCoroutine(WorkerLoop());
    }

    public void StopWorker()
    {
        if (loop != null)
        {
            StopCoroutine(loop);
            loop = null;
        }

        StopAgent();
        SetWalk(false);
        UpdateCarryAnim();
    }

    // =========================================================
    // MAIN LOOP
    // =========================================================
    private IEnumerator WorkerLoop()
    {
        while (true)
        {
            if (!RefsOk())
            {
                currentState = WorkerState.Idle;
                StopAgent();
                SetWalk(false);
                UpdateCarryAnim();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            CustomerAI customer = queueManager.GetFirstCustomer();

            if (customer == null || customer.CurrentOrder == null)
            {
                currentState = WorkerState.Idle;
                yield return DiscardAllCarried();
                ClearCarriedBurger();
                StopAgent();
                SetWalk(false);
                SetCarry(false);
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            BurgerOrder order = customer.CurrentOrder;

            // پلیر برگر را برداشته تا خودش تحویل دهد
            if (customer.NeedsBurger && PlayerHoldingBurger())
            {
                currentState = WorkerState.WaitingPlayer;
                yield return DiscardAllCarried();
                ClearCarriedBurger();
                StopAgent();
                SetWalk(false);
                SetCarry(false);
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // برگر روی میز آماده است و دست پلیر نیست
            if (assemblyStation.IsBurgerClosed && !PlayerHoldingBurger())
            {
                yield return DeliverOrder(customer, order);
                yield return new WaitForSeconds(0.2f);
                continue;
            }

            // ادامه اسمبل
            if (!assemblyStation.IsBurgerClosed)
            {
                yield return AssembleOrder(order);
            }

            yield return null;
        }
    }

    // =========================================================
    // ASSEMBLE
    // =========================================================
    private IEnumerator AssembleOrder(BurgerOrder order)
    {
        while (!assemblyStation.IsBurgerClosed)
        {
            if (PlayerHoldingBurger())
            {
                currentState = WorkerState.WaitingPlayer;
                yield return DiscardAllCarried();
                yield break;
            }

            if (!TryGetNextNeeded(order, out ItemType needed, out int idx))
            {
                yield return new WaitForSeconds(checkInterval);
                yield break;
            }

            // آیتم اضافه در دست (نون تکراری و ...) را دور بینداز
            yield return DiscardUnneededCarried(needed);

            // پلیر همان آیتم لازم را در دست دارد → صبر تا بگذارد
            if (PlayerHolding(needed))
            {
                currentState = WorkerState.WaitingPlayer;
                StopAgent();
                SetWalk(false);
                UpdateCarryAnim();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // صبر برای آماده شدن از کوک/کات
            yield return WaitUntilAvailableOrPlayerPlaced(needed, idx);

            // شاید پلیر وسط صبر گذاشت
            if (assemblyStation.GetAssembledTypes().Count > idx)
                continue;

            if (PlayerHolding(needed))
                continue;

            if (assemblyStation.IsBurgerClosed || PlayerHoldingBurger())
                yield break;

            yield return FetchItem(needed);

            if (carried.Count == 0)
            {
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // قبل از گذاشتن: پلیر همان لایه را گذاشت؟
            if (assemblyStation.GetAssembledTypes().Count > idx)
            {
                yield return DiscardUnneededCarried(ItemType.None);
                continue;
            }

            yield return PlaceCarriedOnAssembly();
        }
    }

    private bool TryGetNextNeeded(BurgerOrder order, out ItemType needed, out int index)
    {
        needed = ItemType.None;
        index = -1;

        if (order == null || order.items == null)
            return false;

        List<ItemType> onTable = assemblyStation.GetAssembledTypes();
        index = onTable.Count;

        if (index >= order.items.Count)
            return false;

        needed = order.items[index];

        if (needed == ItemType.Soda)
            return false;

        return true;
    }

    private IEnumerator WaitUntilAvailableOrPlayerPlaced(ItemType needed, int expectedIndex)
    {
        while (true)
        {
            if (assemblyStation.GetAssembledTypes().Count > expectedIndex)
                yield break;

            if (assemblyStation.IsBurgerClosed || PlayerHoldingBurger())
                yield break;

            // نون را خود وورکر می‌آورد
            if (needed == ItemType.BunBottem || needed == ItemType.BunTop)
                yield break;

            if (needed == ItemType.CookedPatty)
            {
                if (cookingStation != null && cookingStation.HasReadyCookedPatty())
                    yield break;

                if (PlayerHolding(needed))
                {
                    currentState = WorkerState.WaitingPlayer;
                    StopAgent();
                    SetWalk(false);
                    UpdateCarryAnim();
                    yield return new WaitForSeconds(checkInterval);
                    continue;
                }

                currentState = WorkerState.WaitingIngredients;
                StopAgent();
                SetWalk(false);
                UpdateCarryAnim();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            if (IsCutOutput(needed))
            {
                if (cuttingStation != null && cuttingStation.HasReadyOutput(needed))
                    yield break;

                if (PlayerHolding(needed))
                {
                    currentState = WorkerState.WaitingPlayer;
                    StopAgent();
                    SetWalk(false);
                    UpdateCarryAnim();
                    yield return new WaitForSeconds(checkInterval);
                    continue;
                }

                currentState = WorkerState.WaitingIngredients;
                StopAgent();
                SetWalk(false);
                UpdateCarryAnim();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            yield break;
        }
    }

    private bool IsCutOutput(ItemType t)
    {
        return t == ItemType.Lettuce_Cut ||
               t == ItemType.Tomato_Cut ||
               t == ItemType.Onion_Cut ||
               t == ItemType.Cheese_Cut;
    }

    // =========================================================
    // FETCH
    // =========================================================
    private IEnumerator FetchItem(ItemType needed)
    {
        if (carryCapacity - carried.Count <= 0)
            yield break;

        if (needed == ItemType.BunBottem)
        {
            yield return TakeFromSource(bottomBunSource);
            yield break;
        }

        if (needed == ItemType.BunTop)
        {
            yield return TakeFromSource(topBunSource);
            yield break;
        }

        if (needed == ItemType.CookedPatty)
        {
            currentState = WorkerState.GoingToSource;
            Transform p = cookingPoint != null ? cookingPoint : cookingStation.transform;
            yield return MoveTo(p);

            if (cookingStation != null &&
                cookingStation.TryTakeCookedPattyForWorker(out GameObject patty))
            {
                carried.Add(patty);
                PrepareCarry(patty, carried.Count - 1);
                UpdateCarryAnim();
            }

            yield break;
        }

        if (IsCutOutput(needed))
        {
            currentState = WorkerState.GoingToSource;
            Transform p = cuttingPoint != null ? cuttingPoint : cuttingStation.transform;
            yield return MoveTo(p);

            if (cuttingStation != null &&
                cuttingStation.TryTakeOutputForWorker(needed, out GameObject cut))
            {
                carried.Add(cut);
                PrepareCarry(cut, carried.Count - 1);
                UpdateCarryAnim();
            }
        }
    }

    private IEnumerator TakeFromSource(ItemSource source)
    {
        if (source == null || source.spawner == null)
            yield break;

        currentState = WorkerState.GoingToSource;

        Transform stand = source.standPoint != null
            ? source.standPoint
            : source.spawner.transform;

        yield return MoveTo(stand);

        currentState = WorkerState.TakingItem;

        GameObject item = source.spawner.CreateItemForWorker();
        if (item == null)
            yield break;

        carried.Add(item);
        PrepareCarry(item, carried.Count - 1);
        SetCarry(true);
        UpdateCarryAnim();
    }

    private IEnumerator PlaceCarriedOnAssembly()
    {
        currentState = WorkerState.GoingToAssembly;

        Transform p = assemblyPoint != null
            ? assemblyPoint
            : assemblyStation.transform;

        yield return MoveTo(p);

        currentState = WorkerState.PlacingItem;

        for (int i = carried.Count - 1; i >= 0; i--)
        {
            GameObject item = carried[i];

            if (item == null)
            {
                carried.RemoveAt(i);
                continue;
            }

            if (assemblyStation.TryPlaceItemFromWorker(item))
                carried.RemoveAt(i);
            else
                break;

            yield return null;
        }

        StopAgent();
        SetWalk(false);
        UpdateCarryAnim();
    }

    // =========================================================
    // DELIVER — اول برگر، بعد سودا (جدا)
    // =========================================================
    private IEnumerator DeliverOrder(CustomerAI customer, BurgerOrder order)
    {
        if (customer == null || order == null)
            yield break;

        // اگر وسط کار پلیر برگر را برداشت
        if (PlayerHoldingBurger())
            yield break;

        // ----- برگر -----
        currentState = WorkerState.TakingBurger;

        Transform ap = assemblyPoint != null
            ? assemblyPoint
            : assemblyStation.transform;

        yield return MoveTo(ap);

        if (!assemblyStation.IsBurgerClosed || PlayerHoldingBurger())
            yield break;

        GameObject burgerObj = assemblyStation.TakeCompletedBurgerForWorker();
        if (burgerObj == null)
            yield break;

        carriedBurger = burgerObj;
        PrepareCarry(burgerObj, 0);
        SetCarry(true);
        UpdateCarryAnim();

        currentState = WorkerState.GoingToDelivery;

        Transform dp = deliveryPoint != null
            ? deliveryPoint
            : deliveryStation.transform;

        yield return MoveTo(dp);

        currentState = WorkerState.Delivering;

        bool burgerOk = deliveryStation.TryDeliverBurgerFromWorker(carriedBurger, customer);
        if (burgerOk)
            carriedBurger = null;
        else
            Debug.LogWarning("AssembleWorker: burger delivery failed");

        SetWalk(false);
        UpdateCarryAnim();

        // بدون سودا تمام
        if (!order.wantsSoda)
        {
            SetCarry(false);
            yield break;
        }

        // ----- سودا (سفر جدا) -----
        if (customer.NeedsSoda)
        {
            currentState = WorkerState.GoingToSoda;
            yield return TakeFromSource(sodaSource);

            if (carried.Count == 0)
            {
                Debug.LogWarning("AssembleWorker: could not get soda");
                yield break;
            }

            // IsCarry باید true بماند تا MoveTo
            SetCarry(true);

            GameObject sodaObj = carried[carried.Count - 1];

            currentState = WorkerState.GoingToDelivery;
            yield return MoveTo(dp);

            currentState = WorkerState.Delivering;

            carried.Remove(sodaObj);
            bool sodaOk = deliveryStation.TryDeliverSodaFromWorker(sodaObj, customer);

            if (!sodaOk)
            {
                Debug.LogWarning("AssembleWorker: soda delivery failed");
                if (sodaObj != null)
                    Destroy(sodaObj);
            }

            SetWalk(false);
            SetCarry(false);
            UpdateCarryAnim();
        }
    }

    // =========================================================
    // DISCARD / TRASH
    // =========================================================
    private IEnumerator DiscardUnneededCarried(ItemType currentlyNeeded)
    {
        bool needTrashMove = false;

        for (int i = 0; i < carried.Count; i++)
        {
            if (carried[i] == null)
                continue;

            Item item = carried[i].GetComponent<Item>();
            if (item == null || item.Type != currentlyNeeded)
            {
                needTrashMove = true;
                break;
            }
        }

        if (!needTrashMove && currentlyNeeded != ItemType.None)
            yield break;

        if (needTrashMove && trashPoint != null)
        {
            currentState = WorkerState.GoingToTrash;
            yield return MoveTo(trashPoint);
        }

        for (int i = carried.Count - 1; i >= 0; i--)
        {
            GameObject go = carried[i];

            if (go == null)
            {
                carried.RemoveAt(i);
                continue;
            }

            Item item = go.GetComponent<Item>();

            // currentlyNeeded == None یعنی همه را دور بینداز
            if (currentlyNeeded == ItemType.None ||
                item == null ||
                item.Type != currentlyNeeded)
            {
                Destroy(go);
                carried.RemoveAt(i);
            }
        }

        UpdateCarryAnim();
    }

    private IEnumerator DiscardAllCarried()
    {
        if (carried.Count == 0)
            yield break;

        if (trashPoint != null)
        {
            currentState = WorkerState.GoingToTrash;
            yield return MoveTo(trashPoint);
        }

        for (int i = carried.Count - 1; i >= 0; i--)
        {
            if (carried[i] != null)
                Destroy(carried[i]);
            carried.RemoveAt(i);
        }

        UpdateCarryAnim();
    }

    private void ClearCarriedBurger()
    {
        if (carriedBurger != null)
        {
            Destroy(carriedBurger);
            carriedBurger = null;
        }
    }

    // =========================================================
    // HELPERS
    // =========================================================
    private bool RefsOk()
    {
        return agent != null &&
               queueManager != null &&
               assemblyStation != null &&
               deliveryStation != null;
    }

    private bool PlayerHolding(ItemType type)
    {
        return playerPickup != null && playerPickup.HasItem(type);
    }

    private bool PlayerHoldingBurger()
    {
        return playerPickup != null && playerPickup.HasBurger();
    }

    // =========================================================
    // MOVE
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

        while (true)
        {
            bool carrying = carried.Count > 0 || carriedBurger != null;

            if (animator != null)
            {
                animator.SetBool(isWalkParameter, true);
                animator.SetBool(isCarryParameter, carrying);
            }

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                break;
            }

            yield return null;
        }

        StopAgent();

        bool stillCarrying = carried.Count > 0 || carriedBurger != null;

        if (animator != null)
        {
            animator.SetBool(isWalkParameter, false);
            animator.SetBool(isCarryParameter, stillCarrying);
        }
    }

    private void StopAgent()
    {
        if (agent == null || !agent.enabled)
            return;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    // =========================================================
    // CARRY VISUAL + ANIM
    // =========================================================
    private void PrepareCarry(GameObject item, int index)
    {
        if (item == null)
            return;

        Transform parent = carryPoint != null ? carryPoint : transform;
        item.transform.SetParent(parent, false);
        item.transform.localPosition =
            Vector3.up * (carryHeightStart + index * carryHeightStep);
        item.transform.localRotation = Quaternion.identity;

        if (item.GetComponent<Burger>() != null)
            item.transform.localScale = Vector3.one * burgerCarryScale;

        Rigidbody rb = item.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col = item.GetComponent<Collider>();
        if (col != null)
            col.enabled = false;
    }

    private void UpdateCarryAnim()
    {
        SetCarry(carried.Count > 0 || carriedBurger != null);
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