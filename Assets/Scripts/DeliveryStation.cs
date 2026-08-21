using System.Collections.Generic;
using UnityEngine;

public class DeliveryStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Queue")]
    [SerializeField] private QueueManager queueManager;

    [Header("Burger Assembly")]
    [SerializeField] private BurgerAssemblyStation burgerAssemblyStation;

    [Header("Delivery Point")]
    [SerializeField] private Transform deliveryBurgerPoint;

    [Header("Burger Scale On Table")]
    [SerializeField] private Vector3 burgerTableScale = Vector3.one;

    [Header("Money")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private Transform moneyPoint;

    [Header("Money Layout")]
    [SerializeField] private int moneyColumns = 5;
    [SerializeField] private int moneyRows = 2;

    [Space]

    [SerializeField] private float moneySpacingX = 0.08f;
    [SerializeField] private float moneySpacingZ = 0.08f;
    [SerializeField] private float moneyLayerHeight = 0.025f;

    private List<GameObject> spawnedMoney = new List<GameObject>();
    private bool waitingForMoney;

    private List<GameObject> spawnedEatingMoney =
    new List<GameObject>();

    private bool waitingForEatingMoney;

    private CustomerAI eatingCustomer;
    private GameObject deliveredBurger;
    private CustomerAI deliveredCustomer;
    private GameObjectPool moneyPool;

    private const int MoneyBillValue = 5;

    private void Awake()
    {
        if (moneyPrefab != null)
            moneyPool = new GameObjectPool(moneyPrefab, transform, 24);
    }

    public void RecycleMoney(GameObject moneyObject)
    {
        if (moneyObject == null)
            return;

        spawnedMoney.Remove(moneyObject);
        spawnedEatingMoney.Remove(moneyObject);

        if (moneyPool != null)
            moneyPool.Release(moneyObject);
        else
            Destroy(moneyObject);
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TryDeliver();
    }


    // =====================================================
    // TRY DELIVER
    // =====================================================

    private void TryDeliver()
    {
        if (playerPickup == null)
            return;

        // ??? ???? ???? ???? ??? ??? ???
        if (deliveredBurger != null)
        {
            Debug.Log("Delivery table is busy!");
            return;
        }


        if (playerPickup.CurrentCarryCount == 0)
        {
            Debug.Log("No Burger!");
            return;
        }


        GameObject item =
            playerPickup.GetTopItem();

        if (item == null)
            return;


        Burger burger =
            item.GetComponent<Burger>();

        if (burger == null)
        {
            Debug.Log("This is not Burger!");
            return;
        }


        CustomerAI customer =
            queueManager.GetFirstCustomer();

        if (customer == null)
        {
            Debug.Log("No Customer!");
            return;
        }


        CheckOrder(
            burger,
            customer
        );
    }


    // =====================================================
    // CHECK ORDER
    // =====================================================

    private void CheckOrder(
        Burger burger,
        CustomerAI customer
    )
    {
        BurgerOrder order =
            customer.CurrentOrder;

        if (order == null)
        {
            Debug.Log("Customer has no order");
            return;
        }


        if (AreOrdersSame(burger.items, order.items))
        {
            Debug.Log("Order Correct!");

            DeliverBurgerToTable(
                customer
            );
        }
        else
        {
            Debug.Log("Wrong Burger!");
        }
    }


    // =====================================================
    // PUT BURGER ON TABLE
    // =====================================================

    private void DeliverBurgerToTable(
        CustomerAI customer
    )
    {
        if (deliveryBurgerPoint == null)
        {
            Debug.LogError(
                "DeliveryBurgerPoint is not assigned!"
            );

            return;
        }


        // ???? ?? ??? ???? ???? ??????
        GameObject burgerObject =
            playerPickup.RemoveTopItem();

        if (burgerObject == null)
            return;


        // ??????? ???? ?? ??? ????????
        deliveredBurger =
            burgerObject;

        deliveredCustomer =
            customer;

        deliveredCustomer.SetDeliveryStation(this);

        deliveredCustomer.SetQueueManager(queueManager);


        // ???? ??? ??? ???? ???????
        burgerObject.transform.SetParent(
            deliveryBurgerPoint
        );

        burgerObject.transform.localPosition =
            Vector3.zero;

        burgerObject.transform.localRotation =
            Quaternion.identity;


        // Scale ????? ???? ??? ???
        burgerObject.transform.localScale =
            burgerTableScale;


        // Rigidbody
        Rigidbody rb =
            burgerObject.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;

            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }


        // Collider
        Collider col =
            burgerObject.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }


        // Order UI ???? ???
        customer.HideOrder();

        SpawnDeliveryMoney();

        customer.TakeBurgerFromDelivery();

        Debug.Log(
            "Burger delivered to table!"
        );


        // =================================================
        // ????? ???? ????? ????? ???:
        //
        // 1. ??? Spawn ???
        // 2. ????? ??? ????
        // 3. ????? ???? ?? ??????
        // =================================================
    }


    // =====================================================
    // GET DELIVERED BURGER
    // =====================================================

    public GameObject GetDeliveredBurger()
    {
        return deliveredBurger;
    }


    public CustomerAI GetDeliveredCustomer()
    {
        return deliveredCustomer;
    }


    // =====================================================
    // CLEAR DELIVERED BURGER
    // ??? ??? ?? ????? ???? Customer ???? ?? ?????? ??? ???????
    // =====================================================

    public void ClearDeliveredBurger()
    {
        deliveredBurger = null;
        deliveredCustomer = null;
    }


    // =====================================================
    // CHECK ORDERS
    // =====================================================

    private bool AreOrdersSame(
        List<ItemType> burger,
        List<ItemType> order
    )
    {
        if (burger.Count != order.Count)
            return false;
            


        for (int i = 0; i < burger.Count; i++)
        {
            if (burger[i] != order[i])
                return false;
        }


        return true;
    }

    // =====================================================
    // SPAWN DELIVERY MONEY
    // =====================================================

    private void SpawnDeliveryMoney()
    {

        if (moneyPrefab == null)
        {
            Debug.LogError("Money Prefab is not assigned!");
            return;
        }

        if (moneyPoint == null)
        {
            Debug.LogError("Money Point is not assigned!");
            return;
        }

        if (deliveredCustomer == null)
        {
            Debug.LogError("No delivered customer!");
            return;
        }


        BurgerOrder order =
            deliveredCustomer.CurrentOrder;

        if (order == null)
        {
            Debug.LogError("Customer has no order!");
            return;
        }


        // =========================================
        // MONEY VALUE
        // =========================================

        const int moneyValue = 5;

        int orderPrice = order.price;

        int moneyCount =
            orderPrice / moneyValue;


        if (moneyCount <= 0)
        {
            Debug.LogWarning(
                "Order price is too low to spawn money!"
            );

            return;
        }


        // =========================================
        // CLEAR OLD MONEY
        // =========================================

        RecycleMoneyList(spawnedMoney);


        // =========================================
        // MAX MONEY PER LAYER
        // =========================================

        int moneyPerLayer =
            moneyColumns * moneyRows;


        // =========================================
        // SPAWN MONEY
        // =========================================

        for (int i = 0; i < moneyCount; i++)
        {
            // ????? ????
            int layer =
                i / moneyPerLayer;


            // ????? ???? ???? ????
            int indexInLayer =
                i % moneyPerLayer;


            // ????
            int column =
                indexInLayer % moneyColumns;


            // ????
            int row =
                indexInLayer / moneyColumns;


            // =====================================
            // CENTER THE RECTANGLE
            // =====================================

            float offsetX =
                (column -
                (moneyColumns - 1) * 0.5f)
                * moneySpacingX;


            float offsetZ =
                (row -
                (moneyRows - 1) * 0.5f)
                * moneySpacingZ;


            float offsetY =
                layer * moneyLayerHeight;


            Vector3 localOffset =
                new Vector3(
                    offsetX,
                    offsetY,
                    offsetZ
                );


            // ????? ?????? ???? ?? ?????? ?????
            Vector3 spawnPosition =
                moneyPoint.TransformPoint(
                    localOffset
                );


            // =====================================
            // SPAWN
            // =====================================

            GameObject moneyObject = SpawnMoneyBill(
                spawnPosition,
                Quaternion.Euler(90f, 0f, 0f)
            );

            if (moneyObject == null)
                continue;

            DeliveryMoney money = moneyObject.GetComponent<DeliveryMoney>();

            if (money == null)
            {
                RecycleMoney(moneyObject);
                continue;
            }

            money.Setup(MoneyBillValue, this);
            spawnedMoney.Add(moneyObject);
        }


        waitingForMoney = true;


        Debug.Log(
            "Spawned " +
            moneyCount +
            " money objects. Total value = " +
            orderPrice
        );
    }

    // =====================================================
    // MONEY COLLECTED
    // =====================================================

    public void OnMoneyCollected(DeliveryMoney collectedMoney)
    {
        if (collectedMoney != null)
            spawnedMoney.Remove(collectedMoney.gameObject);

        if (spawnedMoney.Count == 0)
            waitingForMoney = false;
    }

    public void SpawnEatingMoney(
    CustomerAI customer,
    RestaurantTable table
)
    {
        if (customer == null)
        {
            Debug.LogError(
                "SpawnEatingMoney: Customer is null!"
            );

            return;
        }

        if (table == null)
        {
            Debug.LogError(
                "SpawnEatingMoney: Table is null!"
            );

            return;
        }

        if (moneyPrefab == null)
        {
            Debug.LogError(
                "Money Prefab is not assigned!"
            );

            return;
        }

        if (table.MoneyPoint == null)
        {
            Debug.LogError(
                "Money Point is not assigned on RestaurantTable!"
            );

            return;
        }


        BurgerOrder order =
            customer.CurrentOrder;

        if (order == null)
        {
            Debug.LogError(
                "Customer has no order!"
            );

            return;
        }


        // =========================================
        // MONEY VALUE
        // =========================================

        const int moneyValue = 5;

        int eatingMoney =
            order.eatingMoney;

        int moneyCount =
            eatingMoney / moneyValue;


        if (moneyCount <= 0)
        {
            Debug.LogWarning(
                "Eating money is too low to spawn money!"
            );

            eatingCustomer = customer;
            waitingForEatingMoney = false;

           // customer.OnEatingMoneyCollected();

            return;
        }


        // =========================================
        // CLEAR OLD EATING MONEY
        // =========================================

        RecycleMoneyList(spawnedEatingMoney);


        // =========================================
        // SAVE CUSTOMER
        // =========================================

        eatingCustomer = customer;


        // =========================================
        // MONEY PER LAYER
        // =========================================

        int moneyPerLayer =
            moneyColumns * moneyRows;


        // =========================================
        // SPAWN
        // =========================================

        for (int i = 0; i < moneyCount; i++)
        {
            int layer =
                i / moneyPerLayer;


            int indexInLayer =
                i % moneyPerLayer;


            int column =
                indexInLayer % moneyColumns;


            int row =
                indexInLayer / moneyColumns;


            // Center rectangle
            float offsetX =
                (column -
                (moneyColumns - 1) * 0.5f)
                * moneySpacingX;


            float offsetZ =
                (row -
                (moneyRows - 1) * 0.5f)
                * moneySpacingZ;


            float offsetY =
                layer * moneyLayerHeight;


            Vector3 localOffset =
                new Vector3(
                    offsetX,
                    offsetY,
                    offsetZ
                );


            Vector3 spawnPosition =
                table.MoneyPoint.TransformPoint(
                    localOffset
                );


            GameObject moneyObject = SpawnMoneyBill(
                spawnPosition,
                table.MoneyPoint.rotation * Quaternion.Euler(90f, 0f, 0f)
            );

            if (moneyObject == null)
                continue;

            DeliveryMoney money = moneyObject.GetComponent<DeliveryMoney>();

            if (money == null)
            {
                RecycleMoney(moneyObject);
                continue;
            }

            money.SetupEatingMoney(MoneyBillValue, this);
            spawnedEatingMoney.Add(moneyObject);
        }


        waitingForEatingMoney = true;


        Debug.Log(
            "Spawned " +
            moneyCount +
            " eating money. Total value = " +
            (moneyCount * moneyValue)
        );
    }

    public void OnEatingMoneyCollected(DeliveryMoney collectedMoney)
    {
        if (collectedMoney != null)
            spawnedEatingMoney.Remove(collectedMoney.gameObject);

        if (spawnedEatingMoney.Count == 0)
        {
            waitingForEatingMoney = false;
            eatingCustomer = null;
        }
    }

    private GameObject SpawnMoneyBill(Vector3 position, Quaternion rotation)
    {
        GameObject moneyObject;

        if (moneyPool != null)
            moneyObject = moneyPool.Get(position, rotation);
        else if (moneyPrefab != null)
            moneyObject = Instantiate(moneyPrefab, position, rotation);
        else
            return null;

        Renderer[] renderers = moneyObject.GetComponentsInChildren<Renderer>();
        for (int i = 0; i < renderers.Length; i++)
            renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

        return moneyObject;
    }

    private void RecycleMoneyList(List<GameObject> moneyList)
    {
        for (int i = 0; i < moneyList.Count; i++)
        {
            GameObject moneyObject = moneyList[i];
            if (moneyObject == null)
                continue;

            if (moneyPool != null)
                moneyPool.Release(moneyObject);
            else
                Destroy(moneyObject);
        }

        moneyList.Clear();
    }
}