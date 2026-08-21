using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    private NavMeshAgent agent;
    private Animator animator;

    [Header("Animation")]
    [SerializeField] private string isWalkParameter = "IsWalk";
    [SerializeField] private string isCarryParameter = "IsCarry";
    [SerializeField] private string isSitParameter = "IsSit";

    [Header("Order UI")]
    [SerializeField] private GameObject orderUI;
    [SerializeField] private OrderUI orderUIController;

    [Header("Movement")]
    [SerializeField] private float reachDistance = 0.4f;
    [SerializeField] private float seatHeightOffset = 1f;

    [Header("Burger")]
    [SerializeField] private Transform burgerHoldPoint;

    //[Space]

    // [SerializeField] private QueueManager queueManager;

    private Transform exitPoint;

    public void SetExitPoint(Transform point)
    {
        exitPoint = point;
    }

    private QueueManager queueManager;

    public void SetQueueManager(QueueManager manager)
    {
        queueManager = manager;
    }

    private RestaurantTable currentTable;
    private bool goingToSeat;
    private bool waitingForTable;
    private bool usingWorldTarget;
    private Vector3 worldTarget;

    private DeliveryStation deliveryStation;

    private Transform currentTarget;

    private BurgerOrder currentOrder;

    private float eatingTimer;

    private bool eatingFinished;
    private GameObject servedBurger;
    private bool walkState;
    private bool carryState;
    private bool sitState;

    public BurgerOrder CurrentOrder =>
        currentOrder;

    public bool HasOrder { get; private set; }

    public bool ReachedTarget { get; private set; }

    public bool IsLeaving { get; private set; }

    public void SetDeliveryStation(
    DeliveryStation station
)
    {
        deliveryStation = station;
    }


    private void Awake()
    {

        agent =
            GetComponent<NavMeshAgent>();

        animator =
            GetComponent<Animator>();


        if (agent == null)
        {
            Debug.LogError(
                "NavMeshAgent missing on " +
                gameObject.name
            );
        }


        if (agent != null)
        {
            agent.obstacleAvoidanceType = ObstacleAvoidanceType.LowQualityObstacleAvoidance;
            agent.autoBraking = true;
            agent.acceleration = 12f;
        }

        if (animator == null)
        {
            Debug.LogError(
                "Animator missing on " +
                gameObject.name
            );
        }


        HideOrder();

        SetWalk(false);
        SetCarry(false);
        SetSit(false);
    }


    private void Update()
    {
        CheckArrival();
        UpdateEating();
        TryTakeFreeTable();
        UpdateLocomotionAnimation();
    }


    // =========================================================
    // MOVE
    // =========================================================

    public void MoveTo(Transform target)
    {
        if (target == null)
            return;

        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        usingWorldTarget = false;
        currentTarget = target;
        ReachedTarget = false;

        SetSit(false);

        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        Vector3 direction = target.position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }

        agent.SetDestination(target.position);
        SetWalk(true);
    }

    public void MoveToPosition(Vector3 position)
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        usingWorldTarget = true;
        worldTarget = position;
        currentTarget = null;
        ReachedTarget = false;

        SetSit(false);

        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        Vector3 direction = position - transform.position;
        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
            transform.rotation = Quaternion.LookRotation(direction);

        agent.SetDestination(position);
        SetWalk(true);
    }


    // =========================================================
    // ARRIVAL
    // =========================================================

    private void CheckArrival()
    {
        if (agent == null || ReachedTarget)
            return;

        if (!agent.enabled)
            return;

        if (currentTarget == null && !usingWorldTarget)
            return;

        Vector3 from = transform.position;
        Vector3 to = usingWorldTarget ? worldTarget : currentTarget.position;
        from.y = 0f;
        to.y = 0f;

        bool closeToTarget =
            Vector3.Distance(from, to) <= reachDistance;

        bool agentReached = false;

        if (!agent.pathPending)
        {
            if (agent.hasPath)
                agentReached = agent.remainingDistance <= reachDistance;
            else
                agentReached = closeToTarget;
        }

        if (agentReached || closeToTarget)
            Arrived();
    }


    private void Arrived()
    {
        if (ReachedTarget)
            return;

        ReachedTarget = true;

        agent.isStopped = true;
        agent.ResetPath();

        SetWalk(false);

        if (currentTable != null && !goingToSeat)
        {
            goingToSeat = true;
            MoveTo(currentTable.SeatPoint);
            return;
        }

        if (currentTable != null && goingToSeat)
        {
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;

            transform.position = currentTable.SeatPoint.position;
            transform.rotation = currentTable.SeatPoint.rotation;

            BeginEating();
            return;
        }

        if (IsLeaving)
        {
            Destroy(gameObject);
            return;
        }

        PlayStandingIdle();
        currentTarget = null;
        usingWorldTarget = false;
    }

    private void PlayStandingIdle()
    {
        SetSit(false);
        SetWalk(false);
        SetCarry(IsHoldingBurger());
    }

    private bool IsHoldingBurger()
    {
        if (servedBurger != null)
            return true;

        return burgerHoldPoint != null && burgerHoldPoint.childCount > 0;
    }



    // =========================================================
    // STOP
    // =========================================================

    public void StopMoving()
    {
        if (agent != null)
        {
            agent.isStopped = true;

            agent.ResetPath();
        }


        currentTarget = null;

        ReachedTarget = true;
        PlayStandingIdle();
    }

    private void UpdateLocomotionAnimation()
    {
        if (sitState)
            return;

        if (agent == null || !agent.enabled)
        {
            PlayStandingIdle();
            return;
        }

        bool moving =
            !agent.isStopped &&
            (agent.velocity.sqrMagnitude > 0.05f ||
             (agent.hasPath && agent.remainingDistance > reachDistance));

        if (moving)
        {
            SetSit(false);
            SetWalk(true);
            SetCarry(IsHoldingBurger());
            return;
        }

        PlayStandingIdle();
    }


    // =========================================================
    // WALK
    // =========================================================

    private void SetWalk(bool value)
    {
        if (animator == null || walkState == value)
            return;

        walkState = value;
        animator.SetBool(isWalkParameter, value);
    }

    public void SetCarry(bool value)
    {
        if (animator == null || carryState == value)
            return;

        carryState = value;
        animator.SetBool(isCarryParameter, value);
    }

    public bool IsCarrying()
    {
        return carryState;
    }

    public void SetSit(bool value)
    {
        if (animator == null)
            return;

        if (sitState != value)
        {
            sitState = value;
            animator.SetBool(isSitParameter, value);
        }

        if (value)
        {
            SetWalk(false);

            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
            }
        }
    }

    public bool IsSitting()
    {
        return sitState;
    }


    // =========================================================
    // ORDER
    // =========================================================

    public void SetOrder(BurgerOrder order)
    {
        currentOrder = order;

        HasOrder = order != null;

        if (order != null)
        {
            Debug.Log(
                gameObject.name +
                " Order: " +
                order.name
            );

            if (orderUIController != null)
            {
                orderUIController.ShowOrder(order);
            }

            ShowOrder();
        }
        else
        {
            HideOrder();
        }
    }


    public void ReceiveOrder()
    {
        HasOrder = true;
    }


    // =========================================================
    // ORDER UI
    // =========================================================

    public void ShowOrder()
    {
        if (orderUI != null)
        {
            orderUI.SetActive(true);
        }
    }


    public void HideOrder()
    {
        if (orderUIController != null)
        {
            orderUIController.ClearUI();
        }

        if (orderUI != null)
        {
            orderUI.SetActive(false);
        }
    }


    // =========================================================
    // LEAVE
    // =========================================================

    public void Leave()
    {
        StartLeaving();
    }

    public void Leave(Transform point)
    {
        if (point != null)
            exitPoint = point;

        StartLeaving();
    }

    private void StartLeaving()
    {
        if (IsLeaving && currentTarget == exitPoint && exitPoint != null)
            return;

        IsLeaving = true;
        goingToSeat = false;
        waitingForTable = false;

        SetSit(false);
        SetCarry(false);
        SetWalk(true);

        PlaceAgentOnNavMesh();

        if (exitPoint == null && queueManager != null)
            exitPoint = queueManager.ExitPoint;

        if (exitPoint == null)
        {
            Debug.LogError("Exit Point is not assigned on " + gameObject.name);
            Destroy(gameObject);
            return;
        }

        MoveTo(exitPoint);
    }

    private void PlaceAgentOnNavMesh()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        Vector3 sampleOrigin = transform.position;

        if (NavMesh.SamplePosition(sampleOrigin, out NavMeshHit hit, 2f, NavMesh.AllAreas))
        {
            agent.Warp(hit.position);
        }
    }

    public void TakeBurgerFromDelivery()
    {
        if (burgerHoldPoint == null)
        {
            Debug.LogError("Burger Hold Point is not assigned!");
            return;
        }

        if (deliveryStation == null)
        {
            Debug.LogError("Delivery Station is not assigned!");
            return;
        }

        GameObject burger =
            deliveryStation.GetDeliveredBurger();

        if (burger == null)
        {
            Debug.Log("No burger on delivery table!");
            return;
        }



        // Burger goes into customer's hand
        servedBurger = burger;

        burger.transform.SetParent(burgerHoldPoint);

        burger.transform.localPosition = Vector3.zero;

        burger.transform.localRotation = Quaternion.identity;

        Rigidbody rb =
            burger.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;

            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        Collider col =
            burger.GetComponent<Collider>();

        if (col != null)
            col.enabled = false;

        deliveryStation.ClearDeliveredBurger();

        if (queueManager != null)
        {
            if (exitPoint == null)
                SetExitPoint(queueManager.ExitPoint);

            queueManager.RemoveCustomer(this);
        }

        SetCarry(true);

        if (!TryGoToTable())
            WaitNearSeatedCustomer();
    }

    private bool TryGoToTable()
    {
        if (TableManager.Instance == null)
            return false;

        RestaurantTable freeTable = TableManager.Instance.GetFreeTable();

        if (freeTable == null)
            return false;

        if (!freeTable.AssignCustomer(this))
            return false;

        waitingForTable = false;
        currentTable = freeTable;
        goingToSeat = false;
        MoveTo(currentTable.TablePoint);
        return true;
    }

    private void WaitNearSeatedCustomer()
    {
        waitingForTable = true;
        currentTable = null;
        goingToSeat = false;

        RestaurantTable busyTable = null;

        if (TableManager.Instance != null)
            busyTable = TableManager.Instance.GetNearestOccupiedTable(transform.position);

        if (busyTable != null)
            MoveToPosition(busyTable.GetWaitPosition());
        else
            StopMoving();
    }

    private void TryTakeFreeTable()
    {
        if (!waitingForTable || IsLeaving || IsSitting())
            return;

        TryGoToTable();
    }

    private void BeginEating()
    {
        waitingForTable = false;
        PlaceBurgerOnTable();
        SetCarry(false);
        SetSit(true);

        if (currentOrder != null)
            eatingTimer = currentOrder.eatingTime;
    }

    private void PlaceBurgerOnTable()
    {
        if (servedBurger == null && burgerHoldPoint != null && burgerHoldPoint.childCount > 0)
            servedBurger = burgerHoldPoint.GetChild(0).gameObject;

        if (servedBurger == null)
            return;

        Transform burger = servedBurger.transform;
        Transform burgerPoint = currentTable != null ? currentTable.BurgerPoint : null;

        Vector3 placePos;
        Quaternion placeRot = Quaternion.identity;

        if (burgerPoint != null)
        {
            burger.SetParent(burgerPoint, true);
            placePos = burgerPoint.position;
            placeRot = burgerPoint.rotation;
        }
        else
        {
            Transform seat = currentTable != null ? currentTable.SeatPoint : null;

            if (seat != null)
            {
                placePos = seat.position + seat.forward * 0.45f + Vector3.up * 0.2f;
                placeRot = Quaternion.LookRotation(seat.forward);
            }
            else
            {
                placePos = transform.position + transform.forward * 0.45f + Vector3.up * 0.85f;
            }

            if (currentTable != null)
                burger.SetParent(currentTable.transform, true);
            else
                burger.SetParent(null, true);
        }

        burger.SetPositionAndRotation(placePos, placeRot);
        SnapBurgerOntoPoint(burger, placePos);

        Rigidbody rb = burger.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        foreach (Collider col in burger.GetComponentsInChildren<Collider>())
            col.enabled = false;
    }

    private void SnapBurgerOntoPoint(Transform burger, Vector3 point)
    {
        Renderer[] renderers = burger.GetComponentsInChildren<Renderer>();

        if (renderers == null || renderers.Length == 0)
            return;

        Bounds bounds = renderers[0].bounds;

        for (int i = 1; i < renderers.Length; i++)
            bounds.Encapsulate(renderers[i].bounds);

        Vector3 bottomCenter = new Vector3(
            bounds.center.x,
            bounds.min.y,
            bounds.center.z
        );

        burger.position += point - bottomCenter;
    }

    private void UpdateEating()
    {
        if (!IsSitting())
            return;

        if (currentTable == null)
            return;

        if (currentOrder == null)
            return;

        if (eatingFinished)
            return;


        eatingTimer -= Time.deltaTime;


        if (eatingTimer <= 0f)
        {
            eatingFinished = true;

            FinishEating();
        }
    }

    private void FinishEating()
    {
        if (currentTable == null)
            return;

        Debug.Log(
            gameObject.name +
            " finished eating!"
        );


        if (servedBurger != null)
        {
            Destroy(servedBurger);
            servedBurger = null;
        }

        if (deliveryStation != null)
        {
            deliveryStation.SpawnEatingMoney(
                this,
                currentTable
            );
        }

        RestaurantTable tableToLeave = currentTable;
        tableToLeave.ReleaseTable();
        currentTable = null;
        waitingForTable = false;

        if (queueManager != null)
            queueManager.RemoveCustomer(this);

        StartCoroutine(StandUpAndLeave());
    }

    private IEnumerator StandUpAndLeave()
    {
        SetSit(false);
        SetCarry(false);

        yield return null;

        SetWalk(true);

        yield return new WaitForSeconds(0.15f);

        StartLeaving();
    }

    private void RemoveBurgerFromTable()
    {
        if (currentTable == null)
            return;

        Transform burgerPoint =
            currentTable.BurgerPoint;

        if (burgerPoint == null)
            return;

        if (burgerPoint.childCount == 0)
            return;

        GameObject burger =
            burgerPoint.GetChild(0).gameObject;

        Destroy(burger);
    }

}