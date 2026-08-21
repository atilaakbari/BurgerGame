using System;
using System.Collections.Generic;
using UnityEngine;

public class QueueManager : MonoBehaviour
{
    public event Action OnQueueChanged;

    [Header("Queue Points")]
    [SerializeField]
    private List<Transform> queuePoints =
        new List<Transform>();

    [Header("Exit")]
    [SerializeField]
    private Transform exitPoint;

    public Transform ExitPoint => exitPoint;

    private readonly List<CustomerAI> customers =
        new List<CustomerAI>();


    // =========================================================
    // FREE SPACE
    // =========================================================

    public bool HasFreeSpace()
    {
        CleanNullCustomers();

        return customers.Count < queuePoints.Count;
    }


    // =========================================================
    // GET FREE POINT
    // =========================================================

    public Transform GetFreeQueuePoint()
    {
        CleanNullCustomers();

        if (!HasFreeSpace())
            return null;

        return queuePoints[customers.Count];
    }


    // =========================================================
    // ADD CUSTOMER
    // =========================================================

    public void AddCustomer(CustomerAI customer)
    {
        if (customer == null)
            return;

        CleanNullCustomers();

        if (customers.Contains(customer))
            return;

        if (customers.Count >= queuePoints.Count)
            return;


        int slot = customers.Count;

        customers.Add(customer);


        Debug.Log(
            "QUEUE ADD | " +
            customer.gameObject.name +
            " | SLOT " +
            slot
        );


        MoveCustomerToSlot(
            customer,
            slot
        );

        UpdateOrderUI();
        OnQueueChanged?.Invoke();
    }


    // =========================================================
    // REMOVE CUSTOMER
    // =========================================================

    public void RemoveCustomer(CustomerAI customer)
    {
        if (customer == null)
            return;


        int removedIndex =
            customers.IndexOf(customer);


        if (removedIndex < 0)
            return;


        Debug.Log(
            "QUEUE REMOVE | " +
            customer.gameObject.name +
            " | OLD SLOT " +
            removedIndex
        );


        // ???? ???:
        // ??? ?? ???? ?? ??? ???
        customers.RemoveAt(
            removedIndex
        );


        // =====================================================
        // ????????? ??? ??? ?? ???? ??? ???????
        // =====================================================

        for (
            int i = removedIndex;
            i < customers.Count;
            i++
        )
        {
            CustomerAI nextCustomer =
                customers[i];


            if (nextCustomer == null)
                continue;


            Debug.Log(
                "QUEUE SHIFT | " +
                nextCustomer.gameObject.name +
                " | NEW SLOT " +
                i
            );


            MoveCustomerToSlot(
                nextCustomer,
                i
            );
        }


        UpdateOrderUI();
        OnQueueChanged?.Invoke();
    }


    // =========================================================
    // MOVE CUSTOMER TO SLOT
    // =========================================================

    private void MoveCustomerToSlot(
        CustomerAI customer,
        int slot
    )
    {
        if (customer == null)
            return;


        if (slot < 0 ||
            slot >= queuePoints.Count)
            return;


        Transform point =
            queuePoints[slot];


        if (point == null)
        {
            Debug.LogError(
                "Queue Point " +
                slot +
                " is NULL!"
            );

            return;
        }


        Debug.Log(
            "QUEUE MOVE | " +
            customer.gameObject.name +
            " ? " +
            point.name
        );


        customer.MoveTo(point);
    }


    // =========================================================
    // ORDER UI
    // =========================================================

    private void UpdateOrderUI()
    {
        CleanNullCustomers();


        for (
            int i = 0;
            i < customers.Count;
            i++
        )
        {
            CustomerAI customer =
                customers[i];


            if (customer == null)
                continue;


            if (i == 0)
            {
                customer.ShowOrder();
            }
            else
            {
                customer.HideOrder();
            }
        }
    }


    // =========================================================
    // FIRST CUSTOMER
    // =========================================================

    public CustomerAI GetFirstCustomer()
    {
        CleanNullCustomers();


        if (customers.Count == 0)
            return null;


        return customers[0];
    }


    // =========================================================
    // COUNT
    // =========================================================

    public int GetCustomerCount()
    {
        CleanNullCustomers();

        return customers.Count;
    }


    // =========================================================
    // CLEAN NULL
    // =========================================================

    private void CleanNullCustomers()
    {
        for (
            int i = customers.Count - 1;
            i >= 0;
            i--
        )
        {
            if (customers[i] == null)
            {
                customers.RemoveAt(i);
            }
        }
    }


    // =========================================================
    // TEST
    // =========================================================

    public void TestRemoveFirstCustomer()
    {
        CustomerAI first =
            GetFirstCustomer();


        if (first != null)
        {
            RemoveCustomer(first);
        }
    }
}