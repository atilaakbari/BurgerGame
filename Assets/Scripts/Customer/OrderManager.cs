using UnityEngine;

public class OrderManager : MonoBehaviour
{
    [Header("Orders")]
    [SerializeField] private BurgerOrder[] availableOrders;

    [Header("Queue")]
    [SerializeField] private QueueManager queueManager;

    private BurgerOrder lastOrder;

    private void Update()
    {
        CheckFirstCustomer();
    }

    private void CheckFirstCustomer()
    {
        if (queueManager == null)
            return;

        CustomerAI customer =
            queueManager.GetFirstCustomer();

        if (customer == null)
            return;

        if (customer.CurrentOrder != null)
            return;

        GiveRandomOrder(customer);
    }

    private void GiveRandomOrder(CustomerAI customer)
    {
        if (availableOrders == null ||
            availableOrders.Length == 0)
        {
            Debug.LogError("No Burger Orders assigned!");
            return;
        }

        // ????? Order??? ?????
        int validCount = 0;

        foreach (BurgerOrder order in availableOrders)
        {
            if (order != null)
                validCount++;
        }

        if (validCount == 0)
        {
            Debug.LogError("All Burger Orders are NULL!");
            return;
        }

        // ??? ??? ?? Order ??????
        // ??????? ???? ?? ?????? ????
        if (validCount == 1)
        {
            foreach (BurgerOrder order in availableOrders)
            {
                if (order != null)
                {
                    lastOrder = order;
                    customer.SetOrder(order);

                    Debug.Log(
                        "Random Order: " +
                        order.name
                    );

                    return;
                }
            }
        }

        // ?????? ??????? ??? ?????? ?? ????
        BurgerOrder selectedOrder;

        do
        {
            int randomIndex =
                Random.Range(
                    0,
                    availableOrders.Length
                );

            selectedOrder =
                availableOrders[randomIndex];

        }
        while (
            selectedOrder == null ||
            selectedOrder == lastOrder
        );

        lastOrder = selectedOrder;

        customer.SetOrder(selectedOrder);

        Debug.Log(
            "Random Order: " +
            selectedOrder.name
        );
    }
}