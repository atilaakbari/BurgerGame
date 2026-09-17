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
        Delivering
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
    [SerializeField] private PlayerPickup playerPickup; // برای تشخیص اینکه پلیر آیتم لازم را برداشته

    [Header("Sources - نون و سودا")]
    [SerializeField] private ItemSource bottomBunSource;
    [SerializeField] private ItemSource topBunSource;
    [SerializeField] private ItemSource sodaSource;

    [Header("Points")]
    [SerializeField] private Transform assemblyPoint;
    [SerializeField] private Transform deliveryPoint;
    [SerializeField] private Transform cookingPoint;
    [SerializeField] private Transform cuttingPoint;

    [Header("Animation")]
    [SerializeField] private string isWalkParameter = "IsWalk";
    [SerializeField] private string isCarryParameter = "IsCarry";

    [Header("Carry / Speed")]
    [Min(1)] [SerializeField] private int carryCapacity = 1;
    [SerializeField] private float moveSpeed = 2.5f;
    [SerializeField] private float stoppingDistance = 0.35f;

    [Header("Carry Visual")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float carryHeightStart = 0.8f;
    [SerializeField] private float carryHeightStep = 0.15f;
    [SerializeField] private float burgerCarryScale = 0.45f; // مثل پلیر تنظیم کن

    [SerializeField] private float checkInterval = 0.25f;

    private WorkerState currentState = WorkerState.Idle;
    private readonly List<GameObject> carried = new List<GameObject>();
    private Coroutine loop;
    private GameObject carriedBurger;

    public int CarryCapacity => carryCapacity;
    public float MoveSpeed => moveSpeed;

    private void Awake()
    {
        if (agent == null) agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
        if (agent != null)
        {
            agent.speed = moveSpeed;
            agent.stoppingDistance = stoppingDistance;
        }
        ForceIdle();
    }

    private void Start() => StartWorker();

    public void StartWorker()
    {
        if (loop != null) StopCoroutine(loop);
        loop = StartCoroutine(WorkerLoop());
    }

    public void StopWorker()
    {
        if (loop != null) { StopCoroutine(loop); loop = null; }
        StopAgent();
        ForceIdle();
    }

    private IEnumerator WorkerLoop()
    {
        while (true)
        {
            if (!RefsOk())
            {
                currentState = WorkerState.Idle;
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            CustomerAI customer = queueManager.GetFirstCustomer();
            if (customer == null || customer.CurrentOrder == null)
            {
                currentState = WorkerState.Idle;
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            BurgerOrder order = customer.CurrentOrder;

            // اگر برگر هنوز کامل نشده → اسمبل لایه به لایه
            if (!assemblyStation.IsBurgerClosed)
            {
                yield return AssembleOrder(order);
            }

            // برگر آماده → بردار، سودا (در صورت نیاز)، تحویل
            if (assemblyStation.IsBurgerClosed)
            {
                yield return DeliverOrder(customer, order);
            }

            yield return null;
        }
    }

    // =========================================================
    // اسمبل بر اساس سفارش از پایین به بالا
    // =========================================================
    private IEnumerator AssembleOrder(BurgerOrder order)
    {
        if (order.items == null) yield break;

        while (!assemblyStation.IsBurgerClosed)
        {
            List<ItemType> assembled = assemblyStation.GetAssembledTypes();
            int nextIndex = assembled.Count;

            // همه لایه‌های برگر چیده شده ولی نون بالا نرفته؟
            // سفارش ممکن است BunTop داشته باشد
            if (nextIndex >= order.items.Count)
            {
                // اگر سفارش نون بالا ندارد ولی برگر باز است — صبر
                currentState = WorkerState.WaitingIngredients;
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                yield break;
            }

            ItemType needed = order.items[nextIndex];

            // سودا جزو لایه‌های برگر نیست
            if (needed == ItemType.Soda)
            {
                // رد کن — در سفارش نباید وسط لایه‌ها باشد
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // اگر پلیر همان لایه را خودش گذاشته، برو بعدی
            // (assembled.Count قبلاً nextIndex بوده؛ اگر بیشتر شد یعنی پلیر گذاشت)
            if (assembled.Count > nextIndex)
                continue;

            // صبر برای آماده شدن توسط وورکرهای دیگر / یا پلیر
            yield return WaitUntilItemAvailableOrPlayerPlaced(needed, nextIndex, order);

            // دوباره چک کن شاید پلیر گذاشته
            assembled = assemblyStation.GetAssembledTypes();
            if (assembled.Count > nextIndex)
                continue;

            if (assemblyStation.IsBurgerClosed)
                yield break;

            // گرفتن آیتم
            yield return FetchItem(needed);

            if (carried.Count == 0)
            {
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // گذاشتن روی میز اسمبل
            yield return PlaceCarriedOnAssembly();
        }
    }

    /// <summary>
    /// صبر می‌کند تا:
    /// - آیتم روی کوک/کات آماده شود، یا
    /// - پلیر آن را بردارد و روی اسمبل بگذارد، یا
    /// - نون/سودا باشد (همیشه در دسترس از اسپانر)
    /// </summary>
    private IEnumerator WaitUntilItemAvailableOrPlayerPlaced(
        ItemType needed, int expectedIndex, BurgerOrder order)
    {
        while (true)
        {
            // پلیر گذاشت؟
            List<ItemType> assembled = assemblyStation.GetAssembledTypes();
            if (assembled.Count > expectedIndex)
                yield break;

            if (assemblyStation.IsBurgerClosed)
                yield break;

            // نون‌ها را خود وورکر می‌آورد — منتظر دیگران نیست
            if (needed == ItemType.BunBottem || needed == ItemType.BunTop)
                yield break;

            // پتی پخته
            if (needed == ItemType.CookedPatty)
            {
                if (cookingStation != null && cookingStation.HasReadyCookedPatty())
                    yield break;

                // پلیر برداشته؟ (دستش پتی است) → صبر تا روی اسمبل بگذارد
                if (PlayerHolding(needed))
                {
                    currentState = WorkerState.WaitingPlayer;
                    ForceIdle();
                    yield return new WaitForSeconds(checkInterval);
                    continue;
                }

                currentState = WorkerState.WaitingIngredients;
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // آیتم‌های برش‌خورده
            if (IsCutOutput(needed))
            {
                if (cuttingStation != null && cuttingStation.HasReadyOutput(needed))
                    yield break;

                if (PlayerHolding(needed))
                {
                    currentState = WorkerState.WaitingPlayer;
                    ForceIdle();
                    yield return new WaitForSeconds(checkInterval);
                    continue;
                }

                currentState = WorkerState.WaitingIngredients;
                ForceIdle();
                yield return new WaitForSeconds(checkInterval);
                continue;
            }

            // بقیه — تلاش برای آوردن
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

    private bool PlayerHolding(ItemType type)
    {
        if (playerPickup == null) return false;
        return playerPickup.HasItem(type);
    }

    private IEnumerator FetchItem(ItemType needed)
    {
        // ظرفیت: چند تا پشت‌سرهم (فعلاً برای اسمبل معمولاً ۱ به ۱ دقیق‌تر است)
        int space = carryCapacity - carried.Count;
        if (space <= 0) yield break;

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
            yield return MoveTo(cookingPoint != null ? cookingPoint : cookingStation.transform);

            if (cookingStation.TryTakeCookedPattyForWorker(out GameObject patty))
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
            yield return MoveTo(cuttingPoint != null ? cuttingPoint : cuttingStation.transform);

            if (cuttingStation.TryTakeOutputForWorker(needed, out GameObject cut))
            {
                carried.Add(cut);
                PrepareCarry(cut, carried.Count - 1);
                UpdateCarryAnim();
            }
            yield break;
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

        // اجباری
        SetCarry(true);
        if (animator != null)
            animator.SetBool(isCarryParameter, true);

        UpdateCarryAnim();
    }

    private IEnumerator PlaceCarriedOnAssembly()
    {
        currentState = WorkerState.GoingToAssembly;
        yield return MoveTo(assemblyPoint != null ? assemblyPoint : assemblyStation.transform);

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
        UpdateCarryAnim();
        SetWalk(false);
    }

    // =========================================================
    // تحویل برگر + سودا
    // =========================================================
    private IEnumerator DeliverOrder(CustomerAI customer, BurgerOrder order)
    {
        if (customer == null || order == null)
            yield break;

        // ---------- ۱) فقط برگر ----------
        currentState = WorkerState.TakingBurger;
        yield return MoveTo(assemblyPoint != null ? assemblyPoint : assemblyStation.transform);

        GameObject burgerObj = assemblyStation.TakeCompletedBurgerForWorker();
        if (burgerObj == null)
        {
            yield return new WaitForSeconds(checkInterval);
            yield break;
        }

        carriedBurger = burgerObj;
        PrepareCarry(burgerObj, 0);
        UpdateCarryAnim();

        currentState = WorkerState.GoingToDelivery;
        yield return MoveTo(deliveryPoint != null ? deliveryPoint : deliveryStation.transform);

        currentState = WorkerState.Delivering;

        bool burgerOk = deliveryStation.TryDeliverBurgerFromWorker(carriedBurger, customer);
        if (burgerOk)
            carriedBurger = null;
        else
        {
            // برگر روی دست نماند — اگر نشد، نابود نکن؛ یک فریم بعد دوباره تلاش
            Debug.LogWarning("AssembleWorker: burger delivery failed");
        }

        UpdateCarryAnim();
        ForceIdle();

        // اگر سفارش کامل شد (بدون سودا) مشتری سینی را گرفته
        // بعد از برگر — دست خالی
        carriedBurger = null;
        SetWalk(false);
        SetCarry(false);

        if (order.wantsSoda && customer.NeedsSoda)
        {
            yield return TakeFromSource(sodaSource);

            // بعد از گرفتن سودا حتماً:
            SetCarry(carried.Count > 0);

            if (carried.Count == 0)
                yield break;

            GameObject sodaObj = carried[carried.Count - 1];
            // تا لحظه تحویل در carried بماند تا MoveTo با IsCarry راه برود

            currentState = WorkerState.GoingToDelivery;
            yield return MoveTo(deliveryPoint != null ? deliveryPoint : deliveryStation.transform);

            // اینجا از لیست بردار و تحویل بده
            carried.Remove(sodaObj);
            deliveryStation.TryDeliverSodaFromWorker(sodaObj, customer);

            SetCarry(false);
            SetWalk(false);
        }

        yield return new WaitForSeconds(0.3f);
    }

    // =========================================================
    // Move / Anim / Carry
    // =========================================================
    private bool RefsOk()
    {
        return agent != null && queueManager != null &&
               assemblyStation != null && deliveryStation != null;
    }

    private IEnumerator MoveTo(Transform target)
    {
        if (target == null || agent == null)
            yield break;

        if (!agent.enabled)
            agent.enabled = true;

        agent.isStopped = false;
        agent.speed = moveSpeed;
        agent.SetDestination(target.position);

        while (true)
        {
            bool carrying = carried.Count > 0 || carriedBurger != null;

            if (animator != null)
            {
                animator.SetBool(isWalkParameter, true);
                animator.SetBool(isCarryParameter, carrying); // با سودا true
            }

            if (!agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance + 0.05f)
                break;

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
        if (agent == null || !agent.enabled) return;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    private void PrepareCarry(GameObject item, int index)
    {
        if (item == null) return;

        Transform parent = carryPoint != null ? carryPoint : transform;
        item.transform.SetParent(parent, false);
        item.transform.localPosition =
            Vector3.up * (carryHeightStart + index * carryHeightStep);
        item.transform.localRotation = Quaternion.identity;

        // برگر کوچک‌تر در دست
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
        if (col != null) col.enabled = false;
    }

    private void ForceIdle()
    {
        SetWalk(false);
        if (carried.Count == 0 && carriedBurger == null)
            SetCarry(false);
        else
            UpdateCarryAnim();
    }

    private void UpdateCarryAnim()
    {
        bool hasSomething = carried.Count > 0 || carriedBurger != null;
        SetCarry(hasSomething);
    }

    private void SetCarry(bool value)
    {
        if (animator != null)
            animator.SetBool(isCarryParameter, value);
    }

    private void SetWalk(bool value)
    {
        if (animator != null)
            animator.SetBool(isWalkParameter, value);
    }

    public void UpgradeCapacity(int c) => carryCapacity = Mathf.Max(1, c);
    public void UpgradeSpeed(float s)
    {
        if (s <= 0) return;
        moveSpeed = s;
        if (agent != null) agent.speed = s;
    }
}