using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Orders")]
    [SerializeField] private BurgerOrder[] availableOrders;

    [Header("Queue")]
    [SerializeField] private QueueManager queueManager;

    [Header("Soda")]
    [SerializeField] private string sodaZoneId = "Zone_SodaStation";

    [Range(0f, 1f)]
    [SerializeField] private float sodaOrderChance = 0.4f;

    private BurgerOrder lastOrder;

    private void OnEnable()
    {
        if (queueManager != null)
            queueManager.OnQueueChanged += AssignOrderToFirstCustomer;

        AssignOrderToFirstCustomer();
    }

    private void OnDisable()
    {
        if (queueManager != null)
            queueManager.OnQueueChanged -= AssignOrderToFirstCustomer;
    }

    private void AssignOrderToFirstCustomer()
    {
        if (queueManager == null)
            return;

        CustomerAI customer =
            queueManager.GetFirstCustomer();

        if (customer == null)
            return;

        if (customer.CurrentOrder != null)
            return;

        BurgerOrder baseOrder =
            PickOrder();

        if (baseOrder == null)
            return;

        BurgerOrder runtimeOrder =
            Instantiate(baseOrder);

        runtimeOrder.name =
            baseOrder.name + "_Runtime";

        runtimeOrder.wantsSoda = false;

        if (IsSodaUnlocked())
        {
            runtimeOrder.wantsSoda =
                Random.value < sodaOrderChance;
        }

        customer.SetOrder(runtimeOrder);
    }

    private bool IsSodaUnlocked()
    {
        if (SaveManager.Instance == null)
            return false;

        if (string.IsNullOrEmpty(sodaZoneId))
            return false;

        return SaveManager.Instance.IsZoneUnlocked(
            sodaZoneId
        );
    }

    private BurgerOrder PickOrder()
    {
        if (
            availableOrders == null ||
            availableOrders.Length == 0
        )
        {
            return null;
        }

        BurgerOrder fallback = null;
        int validCount = 0;

        for (
            int i = 0;
            i < availableOrders.Length;
            i++
        )
        {
            BurgerOrder order =
                availableOrders[i];

            if (order == null)
                continue;

            fallback = order;
            validCount++;
        }

        if (validCount == 0)
            return null;

        if (validCount == 1)
            return fallback;

        BurgerOrder selected;
        int guard = 0;

        do
        {
            selected =
                availableOrders[
                    Random.Range(
                        0,
                        availableOrders.Length
                    )
                ];

            guard++;

        } while (
            (
                selected == null ||
                selected == lastOrder
            )
            &&
            guard < 16
        );

        if (selected == null)
            selected = fallback;

        lastOrder = selected;

        return selected;
    }
}