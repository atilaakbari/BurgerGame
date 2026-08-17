using UnityEngine;

public class CustomerExit : MonoBehaviour
{
    [Header("Exit")]
    [SerializeField] private Transform exitPoint;

    [SerializeField] private float destroyDelay = 5f;


    public void ExitCustomer(CustomerAI customer)
    {
        if (customer == null)
            return;

        if (exitPoint == null)
        {
            Debug.LogError(
                "CustomerExit: Exit Point is not assigned!"
            );

            return;
        }

        customer.Leave(exitPoint);

        Destroy(
            customer.gameObject,
            destroyDelay
        );
    }
}