using UnityEngine;

public class RestaurantTable : MonoBehaviour
{
    [Header("Table Points")]
    [SerializeField] private Transform tablePoint;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform burgerPoint;
    [SerializeField] private Transform sodaPoint;

    [Header("Settings")]
    [SerializeField] private float stayDuration = 10f;

    [Header("Money")]
    [SerializeField] private Transform moneyPoint;

    private CustomerAI currentCustomer;

    public Transform TablePoint =>
        tablePoint;

    public Transform SeatPoint =>
        seatPoint;

    public Transform BurgerPoint =>
        burgerPoint;

    public Transform SodaPoint =>
        sodaPoint;

    public Transform MoneyPoint =>
        moneyPoint;

    public float StayDuration =>
        stayDuration;

    public bool IsOccupied =>
        currentCustomer != null;

    public CustomerAI CurrentCustomer =>
        currentCustomer;

    public Vector3 GetWaitPosition()
    {
        Transform point =
            tablePoint != null
                ? tablePoint
                : seatPoint;

        if (point == null)
        {
            return transform.position +
                   transform.right * 0.9f;
        }

        return point.position +
               point.right * 0.9f;
    }

    public bool AssignCustomer(
        CustomerAI customer
    )
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