using System.Collections.Generic;
using UnityEngine;

public class DeliveryStation : MonoBehaviour
{
    [Header("شناسه‌ی یکتا")]
    [SerializeField] private string stationId;

    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Queue")]
    [SerializeField] private QueueManager queueManager;

    [Header("Burger Assembly")]
    [SerializeField] private BurgerAssemblyStation burgerAssemblyStation;

    [Header("Old Delivery Points - No Longer Used")]
    [SerializeField] private Transform deliveryBurgerPoint;
    [SerializeField] private Transform deliverySodaPoint;

    [Header("Old Scale Settings")]
    [SerializeField] private Vector3 burgerTableScale = Vector3.one;
    [SerializeField] private Vector3 sodaTableScale = Vector3.one;

    [Header("Money")]
    [SerializeField] private GameObject moneyPrefab;
    [SerializeField] private Transform moneyPoint;

    [Header("Tray")]
    [SerializeField] private DeliveryTray trayPrefab;
    [SerializeField] private Transform trayPoint;

    [Header("Money Layout")]
    [SerializeField] private int moneyColumns = 5;
    [SerializeField] private int moneyRows = 2;

    [SerializeField] private float moneySpacingX = 0.08f;
    [SerializeField] private float moneySpacingZ = 0.08f;
    [SerializeField] private float moneyLayerHeight = 0.025f;

    private readonly List<GameObject> spawnedMoney =
        new List<GameObject>();

    private readonly List<GameObject> spawnedEatingMoney =
        new List<GameObject>();

    private bool waitingForMoney;
    private bool waitingForEatingMoney;

    private DeliveryTray currentTray;

    private CustomerAI eatingCustomer;

    private GameObjectPool moneyPool;

    private const int MoneyBillValue = 5;

    // =========================================================
    // AWAKE
    // =========================================================

    private void Awake()
    {
        if (moneyPrefab != null)
        {
            moneyPool =
                new GameObjectPool(
                    moneyPrefab,
                    transform,
                    24
                );
        }
    }

    // =========================================================
    // START
    // =========================================================

    private void Start()
    {
        CreateTray();

        if (SaveManager.Instance != null)
        {
            int savedAmount =
                SaveManager.Instance.GetStationMoneyPile(
                    stationId
                );

            if (savedAmount > 0)
            {
                SpawnMoneyPile(
                    savedAmount
                );
            }
        }
    }

    // =========================================================
    // TRIGGER
    // =========================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        TryDeliver();
    }

    // =========================================================
    // TRY DELIVERY
    // =========================================================

    private void TryDeliver()
    {
        if (playerPickup == null)
            return;

        if (queueManager == null)
            return;

        if (playerPickup.CurrentCarryCount <= 0)
            return;

        GameObject item =
            playerPickup.GetTopItem();

        if (item == null)
            return;

        CustomerAI customer =
            queueManager.GetFirstCustomer();

        if (customer == null)
        {
            Debug.Log("No Customer!");
            return;
        }

        if (customer.CurrentOrder == null)
            return;

        if (currentTray == null)
        {
            CreateTray();

            if (currentTray == null)
                return;
        }

        // =========================================
        // BURGER
        // =========================================

        Burger burger =
            item.GetComponent<Burger>();

        if (burger != null)
        {
            TryDeliverBurger(
                burger,
                customer
            );

            return;
        }

        // =========================================
        // SODA
        // =========================================

        Item itemComponent =
            item.GetComponent<Item>();

        if (
            itemComponent != null &&
            itemComponent.Type == ItemType.Soda
        )
        {
            TryDeliverSoda(
                item,
                customer
            );

            return;
        }

        Debug.Log(
            "This item cannot be delivered here."
        );
    }

    // =========================================================
    // BURGER
    // =========================================================

    private void TryDeliverBurger(
        Burger burger,
        CustomerAI customer
    )
    {
        if (burger == null)
            return;

        if (customer == null)
            return;

        if (customer.CurrentOrder == null)
            return;

        if (!customer.NeedsBurger)
        {
            Debug.Log(
                "Customer does not need Burger."
            );

            return;
        }

        if (currentTray == null)
        {
            CreateTray();

            if (currentTray == null)
                return;
        }

        if (currentTray.ContainsBurger())
        {
            Debug.Log(
                "Tray already contains a Burger."
            );

            return;
        }

        BurgerOrder order =
            customer.CurrentOrder;

        if (
            !AreOrdersSame(
                burger.items,
                order.items
            )
        )
        {
            Debug.Log(
                "Wrong Burger!"
            );

            return;
        }

        GameObject burgerObject =
            playerPickup.RemoveTopItem();

        if (burgerObject == null)
            return;

        if (
            !AddBurgerToTray(
                burgerObject
            )
        )
        {
            Debug.LogError(
                "Failed to add Burger to Tray."
            );

            return;
        }

        customer.SetDeliveryStation(
            this
        );

        customer.SetQueueManager(
            queueManager
        );

        Debug.Log(
            "Burger added to Delivery Tray."
        );

        TryFinishTrayDelivery(
            customer
        );
    }

    // =========================================================
    // SODA
    // =========================================================

    private void TryDeliverSoda(
        GameObject soda,
        CustomerAI customer
    )
    {
        if (soda == null)
            return;

        if (customer == null)
            return;

        if (customer.CurrentOrder == null)
            return;

        if (!customer.NeedsSoda)
        {
            Debug.Log(
                "Customer does not need Soda."
            );

            return;
        }

        if (currentTray == null)
        {
            CreateTray();

            if (currentTray == null)
                return;
        }

        if (currentTray.ContainsSoda())
        {
            Debug.Log(
                "Tray already contains a Soda."
            );

            return;
        }

        GameObject sodaObject =
            playerPickup.RemoveTopItem();

        if (sodaObject == null)
            return;

        if (
            !AddSodaToTray(
                sodaObject
            )
        )
        {
            Debug.LogError(
                "Failed to add Soda to Tray."
            );

            return;
        }

        customer.SetDeliveryStation(
            this
        );

        customer.SetQueueManager(
            queueManager
        );

        Debug.Log(
            "Soda added to Delivery Tray."
        );

        TryFinishTrayDelivery(
            customer
        );
    }

    // =========================================================
    // ADD BURGER TO TRAY
    // =========================================================

        private bool AddBurgerToTray(
        GameObject burger
    )
    {
        if (burger == null)
            return false;

        if (currentTray == null)
            return false;

        if (currentTray.ContainsBurger())
        {
            Debug.Log(
                "Tray already has Burger."
            );

            return false;
        }

        bool added =
            currentTray.AddBurger(
                burger
            );

        if (!added)
            return false;

        burger.transform.localScale =
            burgerTableScale;

        PrepareDeliveredObject(
            burger
        );

        return true;
    }

    // =========================================================
    // ADD SODA TO TRAY
    // =========================================================

        private bool AddSodaToTray(
        GameObject soda
    )
    {
        if (soda == null)
            return false;

        if (currentTray == null)
            return false;

        if (currentTray.ContainsSoda())
        {
            Debug.Log(
                "Tray already has Soda."
            );

            return false;
        }

        bool added =
            currentTray.AddSoda(
                soda
            );

        if (!added)
            return false;

        soda.transform.localScale =
            sodaTableScale;

        PrepareDeliveredObject(
            soda
        );

        return true;
    }

    // =========================================================
    // PREPARE DELIVERED OBJECT
    // =========================================================

    private void PrepareDeliveredObject(
        GameObject obj
    )
    {
        if (obj == null)
            return;

        Rigidbody rb =
            obj.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;

            rb.linearVelocity =
                Vector3.zero;

            rb.angularVelocity =
                Vector3.zero;
        }

        Collider[] colliders =
            obj.GetComponentsInChildren<Collider>();

        for (
            int i = 0;
            i < colliders.Length;
            i++
        )
        {
            colliders[i].enabled = false;
        }
    }

    // =========================================================
    // CHECK TRAY
    // =========================================================

        private void TryFinishTrayDelivery(
        CustomerAI customer
    )
    {
        if (customer == null)
            return;

        BurgerOrder order =
            customer.CurrentOrder;

        if (order == null)
            return;

        if (currentTray == null)
            return;

        // =========================================================
        // BURGER MUST EXIST
        // =========================================================

        if (!currentTray.ContainsBurger())
        {
            return;
        }

        // =========================================================
        // SODA CHECK
        // =========================================================

        if (order.wantsSoda)
        {
            // سفارش Soda دارد ولی هنوز Soda داخل Tray نیست
            if (!currentTray.ContainsSoda())
            {
                Debug.Log(
                    "Order is NOT complete. Waiting for Soda."
                );

                return;
            }
        }
        else
        {
            // اگر سفارش Soda نمی‌خواهد،
            // نباید Soda اضافی داخل Tray باشد.
            if (currentTray.ContainsSoda())
            {
                Debug.LogWarning(
                    "Tray contains Soda but customer does not want Soda."
                );

                return;
            }
        }

        // =========================================================
        // COMPLETE TRAY
        // =========================================================

        DeliveryTray completedTray =
            TakeTray();

        if (completedTray == null)
            return;

        bool received =
            customer.ReceiveTray(
                completedTray
            );

        if (!received)
        {
            Debug.LogError(
                "Customer failed to receive Tray!"
            );

            return;
        }

        FinalizeCompletedOrder(
            customer
        );

        Debug.Log(
            "Complete Tray delivered successfully."
        );
    }

    // =========================================================
    // FINALIZE ORDER
    // =========================================================

    private void FinalizeCompletedOrder(
        CustomerAI customer
    )
    {
        if (customer == null)
            return;

        BurgerOrder order =
            customer.CurrentOrder;

        if (order == null)
            return;

        // -----------------------------------------
        // DELIVERY MONEY
        // -----------------------------------------

        int totalPrice =
            order.price;

        if (order.wantsSoda)
        {
            totalPrice +=
                order.sodaPrice;
        }

        SpawnMoneyPile(
            totalPrice
        );

        // -----------------------------------------
        // XP
        // -----------------------------------------

        if (
            XPManager.Instance != null &&
            order.xpReward > 0
        )
        {
            XPManager.Instance.AddXP(
                order.xpReward
            );
        }
    }

    // =========================================================
    // TRAY
    // =========================================================

    private void CreateTray()
    {
        if (
            trayPrefab == null ||
            trayPoint == null
        )
        {
            Debug.LogError(
                "DeliveryStation: Tray Prefab or Tray Point is missing!"
            );

            return;
        }

        currentTray =
            Instantiate(
                trayPrefab,
                trayPoint.position,
                trayPoint.rotation,
                trayPoint
            );
    }

        public bool AddItemToTray(GameObject item)
    {
        if (item == null)
            return false;

        if (currentTray == null)
            CreateTray();

        if (currentTray == null)
            return false;

        Burger burger =
            item.GetComponent<Burger>();

        if (burger != null)
        {
            return AddBurgerToTray(item);
        }

        Item itemComponent =
            item.GetComponent<Item>();

        if (
            itemComponent != null &&
            itemComponent.Type == ItemType.Soda
        )
        {
            return AddSodaToTray(item);
        }

        Debug.LogWarning(
            "This item cannot be added to the Delivery Tray."
        );

        return false;
    }

    public DeliveryTray TakeTray()
    {
        if (currentTray == null)
            return null;

        DeliveryTray trayToGive =
            currentTray;

        currentTray = null;

        // سینی جدید برای سفارش بعدی
        CreateTray();

        return trayToGive;
    }

    public DeliveryTray GetCurrentTray()
    {
        return currentTray;
    }

    public void GiveTrayToCustomer(
        CustomerAI customer
    )
    {
        if (customer == null)
            return;

        DeliveryTray tray =
            TakeTray();

        if (tray == null)
            return;

        customer.ReceiveTray(
            tray
        );
    }

    // =========================================================
    // CHECK BURGER ORDER
    // =========================================================

    private bool AreOrdersSame(
        List<ItemType> burger,
        List<ItemType> order
    )
    {
        if (
            burger == null ||
            order == null
        )
        {
            return false;
        }

        if (
            burger.Count !=
            order.Count
        )
        {
            return false;
        }

        for (
            int i = 0;
            i < burger.Count;
            i++
        )
        {
            if (
                burger[i] !=
                order[i]
            )
            {
                return false;
            }
        }

        return true;
    }

    // =========================================================
    // DELIVERY MONEY
    // =========================================================

    private void SpawnMoneyPile(
        int totalValue
    )
    {
        if (totalValue <= 0)
            return;

        if (
            moneyPrefab == null ||
            moneyPoint == null
        )
        {
            Debug.LogError(
                "Money Prefab or Money Point is not assigned!"
            );

            return;
        }

        int moneyCount =
            totalValue /
            MoneyBillValue;

        if (moneyCount <= 0)
        {
            Debug.LogWarning(
                "Amount is too low to spawn money!"
            );

            return;
        }

        RecycleMoneyList(
            spawnedMoney
        );

        int moneyPerLayer =
            moneyColumns *
            moneyRows;

        if (moneyPerLayer <= 0)
            moneyPerLayer = 1;

        for (
            int i = 0;
            i < moneyCount;
            i++
        )
        {
            int layer =
                i /
                moneyPerLayer;

            int indexInLayer =
                i %
                moneyPerLayer;

            int column =
                indexInLayer %
                moneyColumns;

            int row =
                indexInLayer /
                moneyColumns;

            float offsetX =
                (
                    column -
                    (
                        moneyColumns - 1
                    ) * 0.5f
                ) *
                moneySpacingX;

            float offsetZ =
                (
                    row -
                    (
                        moneyRows - 1
                    ) * 0.5f
                ) *
                moneySpacingZ;

            float offsetY =
                layer *
                moneyLayerHeight;

            Vector3 localOffset =
                new Vector3(
                    offsetX,
                    offsetY,
                    offsetZ
                );

            Vector3 spawnPosition =
                moneyPoint.TransformPoint(
                    localOffset
                );

            GameObject moneyObject =
                SpawnMoneyBill(
                    spawnPosition,
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f
                    )
                );

            if (moneyObject == null)
                continue;

            DeliveryMoney money =
                moneyObject.GetComponent<DeliveryMoney>();

            if (money == null)
            {
                RecycleMoney(
                    moneyObject
                );

                continue;
            }

            money.Setup(
                MoneyBillValue,
                this
            );

            spawnedMoney.Add(
                moneyObject
            );
        }

        waitingForMoney = true;

        SyncMoneyPileToSave();

        Debug.Log(
            "Spawned money pile. Total value = " +
            (
                spawnedMoney.Count *
                MoneyBillValue
            )
        );
    }

    // =========================================================
    // MONEY COLLECTED
    // =========================================================

    public void OnMoneyCollected(
        DeliveryMoney collectedMoney
    )
    {
        if (collectedMoney != null)
        {
            spawnedMoney.Remove(
                collectedMoney.gameObject
            );
        }

        if (spawnedMoney.Count == 0)
        {
            waitingForMoney = false;
        }

        SyncMoneyPileToSave();
    }

    private void SyncMoneyPileToSave()
    {
        if (SaveManager.Instance == null)
            return;

        SaveManager.Instance.SetStationMoneyPile(
            stationId,
            spawnedMoney.Count *
            MoneyBillValue
        );
    }

    // =========================================================
    // EATING MONEY
    // =========================================================

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

        int eatingMoney =
            order.eatingMoney;

        int moneyCount =
            eatingMoney /
            MoneyBillValue;

        if (moneyCount <= 0)
        {
            Debug.LogWarning(
                "Eating money is too low to spawn money!"
            );

            eatingCustomer =
                customer;

            waitingForEatingMoney = false;

            return;
        }

        RecycleMoneyList(
            spawnedEatingMoney
        );

        eatingCustomer =
            customer;

        int moneyPerLayer =
            moneyColumns *
            moneyRows;

        if (moneyPerLayer <= 0)
            moneyPerLayer = 1;

        for (
            int i = 0;
            i < moneyCount;
            i++
        )
        {
            int layer =
                i /
                moneyPerLayer;

            int indexInLayer =
                i %
                moneyPerLayer;

            int column =
                indexInLayer %
                moneyColumns;

            int row =
                indexInLayer /
                moneyColumns;

            float offsetX =
                (
                    column -
                    (
                        moneyColumns - 1
                    ) * 0.5f
                ) *
                moneySpacingX;

            float offsetZ =
                (
                    row -
                    (
                        moneyRows - 1
                    ) * 0.5f
                ) *
                moneySpacingZ;

            float offsetY =
                layer *
                moneyLayerHeight;

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

            GameObject moneyObject =
                SpawnMoneyBill(
                    spawnPosition,
                    table.MoneyPoint.rotation *
                    Quaternion.Euler(
                        90f,
                        0f,
                        0f
                    )
                );

            if (moneyObject == null)
                continue;

            DeliveryMoney money =
                moneyObject.GetComponent<DeliveryMoney>();

            if (money == null)
            {
                RecycleMoney(
                    moneyObject
                );

                continue;
            }

            money.SetupEatingMoney(
                MoneyBillValue,
                this
            );

            spawnedEatingMoney.Add(
                moneyObject
            );
        }

        waitingForEatingMoney = true;

        Debug.Log(
            "Spawned " +
            moneyCount +
            " eating money. Total value = " +
            (
                moneyCount *
                MoneyBillValue
            )
        );
    }

    public void OnEatingMoneyCollected(
        DeliveryMoney collectedMoney
    )
    {
        if (collectedMoney != null)
        {
            spawnedEatingMoney.Remove(
                collectedMoney.gameObject
            );
        }

        if (
            spawnedEatingMoney.Count == 0
        )
        {
            waitingForEatingMoney = false;
            eatingCustomer = null;
        }
    }

    // =========================================================
    // MONEY SPAWN
    // =========================================================

    private GameObject SpawnMoneyBill(
        Vector3 position,
        Quaternion rotation
    )
    {
        GameObject moneyObject;

        if (moneyPool != null)
        {
            moneyObject =
                moneyPool.Get(
                    position,
                    rotation
                );
        }
        else if (moneyPrefab != null)
        {
            moneyObject =
                Instantiate(
                    moneyPrefab,
                    position,
                    rotation
                );
        }
        else
        {
            return null;
        }

        Renderer[] renderers =
            moneyObject.GetComponentsInChildren<Renderer>();

        for (
            int i = 0;
            i < renderers.Length;
            i++
        )
        {
            renderers[i].shadowCastingMode =
                UnityEngine.Rendering
                    .ShadowCastingMode.Off;
        }

        return moneyObject;
    }

    private void RecycleMoneyList(
        List<GameObject> moneyList
    )
    {
        if (moneyList == null)
            return;

        for (
            int i = 0;
            i < moneyList.Count;
            i++
        )
        {
            GameObject moneyObject =
                moneyList[i];

            if (moneyObject == null)
                continue;

            if (moneyPool != null)
            {
                moneyPool.Release(
                    moneyObject
                );
            }
            else
            {
                Destroy(
                    moneyObject
                );
            }
        }

        moneyList.Clear();
    }

    public void RecycleMoney(
        GameObject moneyObject
    )
    {
        if (moneyObject == null)
            return;

        spawnedMoney.Remove(
            moneyObject
        );

        spawnedEatingMoney.Remove(
            moneyObject
        );

        if (moneyPool != null)
        {
            moneyPool.Release(
                moneyObject
            );
        }
        else
        {
            Destroy(
                moneyObject
            );
        }
    }
}