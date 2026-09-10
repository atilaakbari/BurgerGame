using UnityEngine;

public class RestaurantTable : MonoBehaviour
{
    [Header("Table")]
    [SerializeField] private Transform tablePoint;
    [SerializeField] private Transform seatPoint;
    [SerializeField] private Transform waitPosition;

    [Header("Tray")]
    [SerializeField] private Transform trayPoint;

    [Header("Money")]
    [SerializeField] private Transform moneyPoint;

    private CustomerAI assignedCustomer;

    // =========================================================
    // PUBLIC PROPERTIES
    // =========================================================

    public Transform TablePoint =>
        tablePoint;

    public Transform SeatPoint =>
        seatPoint;

    public Transform WaitPosition =>
        waitPosition;

    public Transform TrayPoint =>
        trayPoint;

    public Transform MoneyPoint =>
        moneyPoint;

    public CustomerAI AssignedCustomer =>
        assignedCustomer;

    public bool IsOccupied =>
        assignedCustomer != null;

    // =========================================================
    // ASSIGN CUSTOMER
    // =========================================================

    public bool AssignCustomer(
        CustomerAI customer
    )
    {
        if (customer == null)
            return false;

        if (IsOccupied)
            return false;

        assignedCustomer =
            customer;

        return true;
    }

    // =========================================================
    // RELEASE TABLE
    // =========================================================

    public void ReleaseTable()
    {
        assignedCustomer = null;
    }

    // =========================================================
    // WAIT POSITION
    // =========================================================

    public Vector3 GetWaitPosition()
    {
        if (waitPosition != null)
            return waitPosition.position;

        if (tablePoint != null)
            return tablePoint.position;

        return transform.position;
    }

    // =========================================================
    // PLACE TRAY
    // =========================================================

    public bool PlaceTray(
        DeliveryTray tray
    )
    {
        if (tray == null)
            return false;

        if (trayPoint == null)
        {
            Debug.LogError(
                "RestaurantTable: Tray Point is not assigned on " +
                gameObject.name
            );

            return false;
        }

        tray.transform.SetParent(
            trayPoint
        );

        tray.transform.localPosition =
            Vector3.zero;

        tray.transform.localRotation =
            Quaternion.identity;

        return true;
    }

    // =========================================================
    // CLEAR TRAY
    // =========================================================

    public void ClearTray()
    {
        if (trayPoint == null)
            return;

        for (
            int i = trayPoint.childCount - 1;
            i >= 0;
            i--
        )
        {
            Transform child =
                trayPoint.GetChild(i);

            if (child == null)
                continue;

            DeliveryTray tray =
                child.GetComponent<DeliveryTray>();

            if (tray != null)
            {
                Destroy(
                    tray.gameObject
                );
            }
        }
    }
}