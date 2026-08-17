using UnityEngine;

public class RestaurantTable : MonoBehaviour
{
    [Header("Table Points")]
    [SerializeField] private Transform tablePoint;
    [SerializeField] private Transform seatPoint;

    [Header("Settings")]
    [SerializeField] private float stayDuration = 10f;

    private CustomerAI currentCustomer;

    public Transform TablePoint => tablePoint;
    public Transform SeatPoint => seatPoint;

    public float StayDuration => stayDuration;

    public bool IsOccupied => currentCustomer != null;

    public CustomerAI CurrentCustomer => currentCustomer;


    public bool AssignCustomer(CustomerAI customer)
    {
        if (customer == null)
            return false;

        if (IsOccupied)
            return false;

        currentCustomer = customer;

        return true;
    }


    public void ReleaseTable()
    {
        currentCustomer = null;
    }
}
