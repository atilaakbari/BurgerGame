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

    [Header("Tray")]
    [SerializeField] private Transform carryPoint;
    [SerializeField] private float trayCarryHeight = 0f;

    private Transform exitPoint;

    private QueueManager queueManager;
    private DeliveryStation deliveryStation;

    private RestaurantTable currentTable;

    private bool goingToSeat;
    private bool waitingForTable;
    private bool usingWorldTarget;

    private Vector3 worldTarget;
    private Transform currentTarget;

    private BurgerOrder currentOrder;

    private float eatingTimer;

    private bool eatingFinished;

    private DeliveryTray servedTray;

    private bool burgerReceived;
    private bool sodaReceived;

    private bool orderCompleted;

    private bool walkState;
    private bool carryState;
    private bool sitState;

    // =========================================================
    // PUBLIC PROPERTIES
    // =========================================================

    public BurgerOrder CurrentOrder =>
        currentOrder;

    public bool HasOrder { get; private set; }

    public bool ReachedTarget { get; private set; }

    public bool IsLeaving { get; private set; }

    public bool NeedsBurger
    {
        get
        {
            return currentOrder != null &&
                   !burgerReceived;
        }
    }

    public bool NeedsSoda
    {
        get
        {
            return currentOrder != null &&
                   currentOrder.wantsSoda &&
                   !sodaReceived;
        }
    }

    public bool IsOrderComplete
    {
        get
        {
            if (currentOrder == null)
                return false;

            if (!burgerReceived)
                return false;

            if (
                currentOrder.wantsSoda &&
                !sodaReceived
            )
            {
                return false;
            }

            return true;
        }
    }

    public bool HasBurgerReceived =>
        burgerReceived;

    public bool HasSodaReceived =>
        sodaReceived;

    // =========================================================
    // SETTERS
    // =========================================================

    public void SetExitPoint(
        Transform point
    )
    {
        exitPoint = point;
    }

    public void SetQueueManager(
        QueueManager manager
    )
    {
        queueManager = manager;
    }

    public void SetDeliveryStation(
        DeliveryStation station
    )
    {
        deliveryStation = station;
    }

    // =========================================================
    // AWAKE
    // =========================================================

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
            agent.obstacleAvoidanceType =
                ObstacleAvoidanceType
                    .LowQualityObstacleAvoidance;

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

    // =========================================================
    // UPDATE
    // =========================================================

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

    public void MoveTo(
        Transform target
    )
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

        Vector3 direction =
            target.position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction
                );
        }

        agent.SetDestination(
            target.position
        );

        SetWalk(true);
    }

    public void MoveToPosition(
        Vector3 position
    )
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

        Vector3 direction =
            position -
            transform.position;

        direction.y = 0f;

        if (direction.sqrMagnitude > 0.01f)
        {
            transform.rotation =
                Quaternion.LookRotation(
                    direction
                );
        }

        agent.SetDestination(
            position
        );

        SetWalk(true);
    }

    // =========================================================
    // ARRIVAL
    // =========================================================

    private void CheckArrival()
    {
        if (agent == null)
            return;

        if (ReachedTarget)
            return;

        if (!agent.enabled)
            return;

        if (
            currentTarget == null &&
            !usingWorldTarget
        )
        {
            return;
        }

        Vector3 from =
            transform.position;

        Vector3 to =
            usingWorldTarget
                ? worldTarget
                : currentTarget.position;

        from.y = 0f;
        to.y = 0f;

        bool closeToTarget =
            Vector3.Distance(
                from,
                to
            ) <= reachDistance;

        bool agentReached = false;

        if (!agent.pathPending)
        {
            if (agent.hasPath)
            {
                agentReached =
                    agent.remainingDistance
                    <= reachDistance;
            }
            else
            {
                agentReached =
                    closeToTarget;
            }
        }

        if (
            agentReached ||
            closeToTarget
        )
        {
            Arrived();
        }
    }

    private void Arrived()
    {
        if (ReachedTarget)
            return;

        ReachedTarget = true;

        if (agent != null && agent.enabled)
        {
            agent.isStopped = true;
            agent.ResetPath();
        }

        SetWalk(false);

        if (
            currentTable != null &&
            !goingToSeat
        )
        {
            goingToSeat = true;

            MoveTo(
                currentTable.SeatPoint
            );

            return;
        }

        if (
            currentTable != null &&
            goingToSeat
        )
        {
            if (agent != null)
            {
                agent.isStopped = true;
                agent.ResetPath();
                agent.enabled = false;
            }

            if (currentTable.SeatPoint != null)
            {
                transform.position =
                    currentTable
                        .SeatPoint
                        .position;

                transform.rotation =
                    currentTable
                        .SeatPoint
                        .rotation;
            }

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

    // =========================================================
    // ANIMATION
    // =========================================================

    private void PlayStandingIdle()
    {
        SetSit(false);

        SetWalk(false);

        SetCarry(
            HasAnyReceivedFood()
        );
    }

    private bool HasAnyReceivedFood()
    {
        return
            servedTray != null ||
            burgerReceived ||
            sodaReceived;
    }

    private void UpdateLocomotionAnimation()
    {
        if (sitState)
            return;

        if (
            agent == null ||
            !agent.enabled
        )
        {
            PlayStandingIdle();
            return;
        }

        bool moving =
            !agent.isStopped &&
            (
                agent.velocity.sqrMagnitude >
                0.05f ||

                (
                    agent.hasPath &&
                    agent.remainingDistance >
                    reachDistance
                )
            );

        if (moving)
        {
            SetSit(false);

            SetWalk(true);

            SetCarry(
                HasAnyReceivedFood()
            );

            return;
        }

        PlayStandingIdle();
    }

    private void SetWalk(
        bool value
    )
    {
        if (
            animator == null ||
            walkState == value
        )
        {
            return;
        }

        walkState = value;

        animator.SetBool(
            isWalkParameter,
            value
        );
    }

    public void SetCarry(
        bool value
    )
    {
        if (
            animator == null ||
            carryState == value
        )
        {
            return;
        }

        carryState = value;

        animator.SetBool(
            isCarryParameter,
            value
        );
    }

    public bool IsCarrying()
    {
        return carryState;
    }

    public void SetSit(
        bool value
    )
    {
        if (animator == null)
            return;

        if (sitState != value)
        {
            sitState = value;

            animator.SetBool(
                isSitParameter,
                value
            );
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

    public void SetOrder(
        BurgerOrder order
    )
    {
        currentOrder = order;

        HasOrder =
            order != null;

        burgerReceived = false;
        sodaReceived = false;

        orderCompleted = false;

        servedTray = null;

        eatingFinished = false;

        if (order != null)
        {
            Debug.Log(
                gameObject.name +
                " Order: " +
                order.name +
                " | Soda: " +
                order.wantsSoda
            );

            if (
                orderUIController != null
            )
            {
                orderUIController.ShowOrder(
                    order
                );
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
            orderUI.SetActive(true);
    }

    public void HideOrder()
    {
        if (
            orderUIController != null
        )
        {
            orderUIController.ClearUI();
        }

        if (orderUI != null)
            orderUI.SetActive(false);
    }

    // =========================================================
    // RECEIVE TRAY
    // =========================================================

        public bool ReceiveTray(DeliveryTray tray)
    {
        if (tray == null || currentOrder == null)
            return false;

        if (carryPoint == null)
        {
            Debug.LogError("CustomerAI: CarryPoint missing!");
            return false;
        }

        // بدون برگر تحویل نگیر
        if (!tray.ContainsBurger())
            return false;

        // اگر سودا لازم است، باید روی سینی باشد
        if (currentOrder.wantsSoda && !tray.ContainsSoda())
            return false;

        servedTray = tray;
        tray.transform.SetParent(carryPoint, false);
        tray.transform.localPosition = Vector3.up * trayCarryHeight;
        tray.transform.localRotation = Quaternion.identity;

        burgerReceived = true;
        sodaReceived = !currentOrder.wantsSoda || tray.ContainsSoda();

        SetCarry(true);
        TryStartAfterOrderComplete();
        return true;
    }

    // =========================================================
    // ORDER COMPLETE
    // =========================================================

    private void TryStartAfterOrderComplete()
    {
        if (!IsOrderComplete)
            return;

        if (orderCompleted)
            return;

        orderCompleted = true;

        HideOrder();

        if (queueManager != null)
        {
            if (exitPoint == null)
            {
                exitPoint =
                    queueManager.ExitPoint;
            }

            queueManager.RemoveCustomer(
                this
            );
        }

        if (!TryGoToTable())
        {
            WaitNearSeatedCustomer();
        }
    }

    // =========================================================
    // TABLE
    // =========================================================

    private bool TryGoToTable()
    {
        if (TableManager.Instance == null)
            return false;

        RestaurantTable freeTable =
            TableManager.Instance.GetFreeTable();

        if (freeTable == null)
            return false;

        if (
            !freeTable.AssignCustomer(
                this
            )
        )
        {
            return false;
        }

        waitingForTable = false;

        currentTable =
            freeTable;

        goingToSeat = false;

        MoveTo(
            currentTable.TablePoint
        );

        return true;
    }

    private void WaitNearSeatedCustomer()
    {
        waitingForTable = true;

        currentTable = null;

        goingToSeat = false;

        RestaurantTable busyTable = null;

        if (
            TableManager.Instance != null
        )
        {
            busyTable =
                TableManager.Instance
                    .GetNearestOccupiedTable(
                        transform.position
                    );
        }

        if (busyTable != null)
        {
            MoveToPosition(
                busyTable.GetWaitPosition()
            );
        }
        else
        {
            StopMoving();
        }
    }

    private void TryTakeFreeTable()
    {
        if (!waitingForTable)
            return;

        if (IsLeaving)
            return;

        if (IsSitting())
            return;

        if (!IsOrderComplete)
            return;

        TryGoToTable();
    }

    // =========================================================
    // EATING
    // =========================================================

    private void BeginEating()
    {
        if (!IsOrderComplete)
            return;

        waitingForTable = false;

        PlaceTrayOnTable();

        SetCarry(false);

        SetSit(true);

        if (currentOrder != null)
        {
            eatingTimer =
                currentOrder.eatingTime;
        }

        eatingFinished = false;
    }

    private void PlaceTrayOnTable()
    {
        if (servedTray == null)
            return;

        if (currentTable == null)
            return;

        currentTable.PlaceTray(
            servedTray
        );
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

        eatingTimer -=
            Time.deltaTime;

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

        ClearFoodFromTable();

        if (deliveryStation != null)
        {
            deliveryStation.SpawnEatingMoney(
                this,
                currentTable
            );
        }

        RestaurantTable tableToLeave =
            currentTable;

        tableToLeave.ReleaseTable();

        currentTable = null;

        waitingForTable = false;

        if (queueManager != null)
        {
            queueManager.RemoveCustomer(
                this
            );
        }

        StartCoroutine(
            StandUpAndLeave()
        );
    }

    // =========================================================
    // CLEAR TRAY
    // =========================================================

    private void ClearFoodFromTable()
    {
        if (servedTray == null)
            return;

        Destroy(
            servedTray.gameObject
        );

        servedTray = null;
    }

    // =========================================================
    // LEAVE
    // =========================================================

    private IEnumerator StandUpAndLeave()
    {
        SetSit(false);
        SetCarry(false);

        yield return null;

        SetWalk(true);

        yield return new WaitForSeconds(
            0.15f
        );

        StartLeaving();
    }

    public void Leave()
    {
        StartLeaving();
    }

    public void Leave(
        Transform point
    )
    {
        if (point != null)
            exitPoint = point;

        StartLeaving();
    }

    private void StartLeaving()
    {
        if (
            IsLeaving &&
            currentTarget == exitPoint &&
            exitPoint != null
        )
        {
            return;
        }

        IsLeaving = true;

        goingToSeat = false;

        waitingForTable = false;

        SetSit(false);

        SetCarry(false);

        SetWalk(true);

        PlaceAgentOnNavMesh();

        if (
            exitPoint == null &&
            queueManager != null
        )
        {
            exitPoint =
                queueManager.ExitPoint;
        }

        if (exitPoint == null)
        {
            Debug.LogError(
                "Exit Point is not assigned on " +
                gameObject.name
            );

            Destroy(gameObject);

            return;
        }

        MoveTo(
            exitPoint
        );
    }

    private void PlaceAgentOnNavMesh()
    {
        if (agent == null)
            return;

        if (!agent.enabled)
            agent.enabled = true;

        Vector3 sampleOrigin =
            transform.position;

        if (
            NavMesh.SamplePosition(
                sampleOrigin,
                out NavMeshHit hit,
                2f,
                NavMesh.AllAreas
            )
        )
        {
            agent.Warp(
                hit.position
            );
        }
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
}