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
    [SerializeField] private float reachDistance = 0.2f;
    [SerializeField] private float seatHeightOffset = 1f;

    [Header("Burger")]
    [SerializeField] private Transform burgerHoldPoint;


    private RestaurantTable currentTable;
    private bool goingToSeat;

    private DeliveryStation deliveryStation;

    private Transform currentTarget;

    private BurgerOrder currentOrder;

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
    }


    // =========================================================
    // MOVE
    // =========================================================

    public void MoveTo(Transform target)
    {

        Debug.Log(
    gameObject.name +
    " MOVETO ? " +
    target.name
);

        if (target == null)
            return;

        if (agent == null)
            return;

        // ???????? ?? ?? ??? ???? ???
        // ???? ????? ???? QueueManager ??????? ???.
        if (IsLeaving && target != currentTarget)
            return;


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

        agent.SetDestination(
            target.position
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


        if (currentTarget == null)
            return;


        if (ReachedTarget)
            return;


        if (agent.pathPending)
            return;


        if (!agent.hasPath)
            return;


        if (
            agent.remainingDistance
            <= reachDistance
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

        agent.isStopped = true;
        agent.ResetPath();

        SetWalk(false);

        // ????? ?? TablePoint
        if (currentTable != null && !goingToSeat)
        {
            goingToSeat = true;

            // ?????? Agent ?? ???? ???? ?? SeatPoint ???? ??? ????????
            MoveTo(currentTable.SeatPoint);

            return;
        }


        // ????? ?? SeatPoint
        if (currentTable != null && goingToSeat)
        {
            // ???? NavMeshAgent ????? ?????? ????? ?? ????? ???
            agent.isStopped = true;
            agent.ResetPath();
            agent.enabled = false;

            // ???? ????? ???? ??? SeatPoint
            transform.position =
                currentTable.SeatPoint.position;

            // ???? ???? ?? ??? ???? ?????
            transform.rotation =
                currentTable.SeatPoint.rotation;

            // ?????
            SetSit(true);

            PlaceBurgerOnTable();

            return;
        }

        currentTarget = null;
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


        SetWalk(false);
    }


    // =========================================================
    // WALK
    // =========================================================

    private void SetWalk(bool value)
    {
        if (animator == null)
            return;


        animator.SetBool(
            isWalkParameter,
            value
        );
    }


    // =========================================================
    // CARRY
    // =========================================================

    public void SetCarry(bool value)
    {
        if (animator == null)
            return;


        animator.SetBool(
            isCarryParameter,
            value
        );
    }


    public bool IsCarrying()
    {
        if (animator == null)
            return false;


        return animator.GetBool(
            isCarryParameter
        );
    }


    // =========================================================
    // SIT
    // =========================================================

    public void SetSit(bool value)
    {
        if (animator == null)
            return;


        animator.SetBool(
            isSitParameter,
            value
        );


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
        if (animator == null)
            return false;


        return animator.GetBool(
            isSitParameter
        );
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
        IsLeaving = true;


        SetSit(false);

        SetCarry(false);

        SetWalk(true);
    }

    public void Leave(Transform exitPoint)
    {
        if (exitPoint == null)
            return;

        IsLeaving = true;

        SetSit(false);
        SetCarry(false);

        currentTarget = exitPoint;

        ReachedTarget = false;

        agent.isStopped = false;
        agent.stoppingDistance = 0f;

        agent.SetDestination(
            exitPoint.position
        );

        SetWalk(true);
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

        // Customer is carrying the burger
        SetCarry(true);

        // Find a free table
        RestaurantTable freeTable =
            TableManager.Instance.GetFreeTable();

        if (freeTable != null)
        {
            if (freeTable.AssignCustomer(this))
            {
                currentTable = freeTable;

                goingToSeat = false;

                MoveTo(
                    currentTable.TablePoint
                );
            }
        }
        else
        {
            Debug.LogWarning(
                "No free table available!"
            );
        }

        Debug.Log("Customer took the burger!");
    }

    private void PlaceBurgerOnTable()
    {
        if (burgerHoldPoint == null)
            return;

        if (currentTable == null)
            return;

        Transform burgerPoint =
            currentTable.BurgerPoint;

        if (burgerPoint == null)
        {
            Debug.LogWarning(
                "Burger Point is not assigned!"
            );

            return;
        }

        if (burgerHoldPoint.childCount == 0)
            return;

        Transform burger =
            burgerHoldPoint.GetChild(0);

        burger.SetParent(burgerPoint);

        burger.localPosition = Vector3.zero;
        burger.localRotation = Quaternion.identity;

        SetCarry(false);
    }

}