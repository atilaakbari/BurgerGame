using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Orders")]
    [SerializeField] private BurgerOrder[] availableOrders;

    [Header("Queue")]
    [SerializeField] private QueueManager queueManager;

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

        CustomerAI customer = queueManager.GetFirstCustomer();

        if (customer == null || customer.CurrentOrder != null)
            return;

        BurgerOrder order = PickOrder();
        if (order != null)
            customer.SetOrder(order);
    }

    private BurgerOrder PickOrder()
    {
        if (availableOrders == null || availableOrders.Length == 0)
            return null;

        BurgerOrder fallback = null;
        int validCount = 0;

        for (int i = 0; i < availableOrders.Length; i++)
        {
            BurgerOrder order = availableOrders[i];
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
            selected = availableOrders[Random.Range(0, availableOrders.Length)];
            guard++;
        }
        while ((selected == null || selected == lastOrder) && guard < 16);

        lastOrder = selected;
        return selected;
    }
}
