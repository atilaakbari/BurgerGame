using System.Collections.Generic;
using UnityEngine;

public class PlayerPickup : MonoBehaviour
{
    [Header("Carry")]
    [SerializeField] private Transform carryPoint;

    [Header("Animation")]
    [SerializeField] private Animator animator;

    [Header("Movement")]
    [SerializeField] private PlayerController playerController;

    [Header("Inventory")]
    [SerializeField] private int maxCarryCount = 2;

    [Header("Stack Settings")]
    [SerializeField] private float stackGap = 0.02f;

    [Header("Carry Height")]
    [SerializeField] private float carryHeightOffset = 0f;

    [Header("Burger Carry")]
    [SerializeField] private float burgerCarryScale = 0.5f;

    private List<GameObject> carriedItems = new List<GameObject>();

    public bool IsCarrying => carriedItems.Count > 0;

    private bool carryingBurger = false;

    public bool IsHandBusy
    {
        get
        {
            return carryingBurger || carriedItems.Count > 0;
        }
    }

    public GameObject CurrentItem
    {
        get
        {
            if (carriedItems.Count == 0)
                return null;

            return carriedItems[0];
        }
    }

    public int CurrentCarryCount => carriedItems.Count;

    public bool HasSpace =>
        carriedItems.Count < maxCarryCount;

    public int AvailableSpace =>
    maxCarryCount - carriedItems.Count;


    // ==============================
    // PICKUP
    // ==============================

    public bool TryPickup(GameObject item)
    {

        if (carryingBurger)
        {
            Debug.Log("Hand is holding burger!");
            return false;
        }


        if (item == null)
            return false;

        // ??? ???? ???? ??? ???? ???? ???? ???? ????
        if (HasBurger())
        {
            Debug.Log("Cannot pickup item while carrying burger!");
            return false;
        }

        if (!HasSpace)
            return false;


        // Scale ???? ???? ??? ?? ???????
        Vector3 originalPrefabScale =
            item.transform.localScale;

        carriedItems.Add(item);

        // ??? CarryPoint ????
        item.transform.SetParent(carryPoint);

        // ???? ???? ????
        item.transform.localRotation =
            Quaternion.identity;

        // ??? ???? Scale ????? ????
        Burger burger =
            item.GetComponent<Burger>();

        if (burger != null)
        {
            item.transform.localScale =
                Vector3.one * burgerCarryScale;
        }
        else
        {
            // ???????? ?????? ?????? Scale ??????
            item.transform.localScale =
                originalPrefabScale;
        }


        // ????? ???? ?????
        Rigidbody rb =
            item.GetComponent<Rigidbody>();

        if (rb != null)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        // ????? ???? Collider
        Collider col =
            item.GetComponent<Collider>();

        if (col != null)
        {
            col.enabled = false;
        }


        // ???? ???? ???? ???????
        RebuildCarryStack();


        // ???? ???? ???? ???
        UpdateCarryState();


        return true;
    }


    // ==============================
    // REMOVE ITEM BY TYPE
    // ==============================

    public GameObject RemoveItem(ItemType itemType)
    {
        for (int i = 0; i < carriedItems.Count; i++)
        {
            GameObject item =
                carriedItems[i];

            if (item == null)
                continue;


            Item itemData =
                item.GetComponent<Item>();


            if (itemData != null &&
                itemData.Type == itemType)
            {
                carriedItems.RemoveAt(i);


                // ??? ???? ?? CarryPoint
                item.transform.SetParent(null);

                Burger burger =
                 item.GetComponent<Burger>();

                if (burger != null)
                {
                    item.transform.localScale =
                        Vector3.one;
                }


                // ???? ???? ?????
                Rigidbody rb =
                    item.GetComponent<Rigidbody>();

                if (rb != null)
                {
                    rb.isKinematic = false;
                }


                // ???? ???? Collider
                Collider col =
                    item.GetComponent<Collider>();

                if (col != null)
                {
                    col.enabled = true;
                }


                // ???? ???? ???????? ??????????
                RebuildCarryStack();


                // ????? ???????
                UpdateCarryState();


                return item;
            }
        }


        return null;
    }


    // ==============================
    // CHECK ITEM
    // ==============================

    public bool HasItem(ItemType itemType)
    {
        foreach (GameObject item in carriedItems)
        {
            if (item == null)
                continue;


            Item itemData =
                item.GetComponent<Item>();


            if (itemData != null &&
                itemData.Type == itemType)
            {
                return true;
            }
        }


        return false;
    }


    // ==============================
    // DROP LAST ITEM
    // ==============================

    public GameObject DropItem()
    {
        if (carriedItems.Count == 0)
            return null;


        int lastIndex =
            carriedItems.Count - 1;


        GameObject item =
            carriedItems[lastIndex];


        carriedItems.RemoveAt(lastIndex);


        if (item != null)
        {
            item.transform.SetParent(null);

            Burger burger =
    item.GetComponent<Burger>();

            if (burger != null)
            {
                item.transform.localScale =
                    Vector3.one;
            }


            Rigidbody rb =
                item.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
            }


            Collider col =
                item.GetComponent<Collider>();

            if (col != null)
            {
                col.enabled = true;
            }
        }


        // ???? ???? ???????? ??????????
        RebuildCarryStack();


        UpdateCarryState();


        return item;
    }


    // ==============================
    // REBUILD STACK
    // ==============================

    private void RebuildCarryStack()
    {
        float currentTop = 0f;

        for (int i = 0; i < carriedItems.Count; i++)
        {
            GameObject item = carriedItems[i];

            if (item == null)
                continue;

            // ????? ???? Renderer ??? ????
            Renderer[] renderers =
                item.GetComponentsInChildren<Renderer>();

            if (renderers.Length == 0)
                continue;

            // ?????? Bounds ?? ???? CarryPoint
            float lowestPoint = float.MaxValue;
            float highestPoint = float.MinValue;

            foreach (Renderer renderer in renderers)
            {
                Bounds bounds = renderer.bounds;

                // ????? ?????????? ? ???????? ????
                // ?? World Space ?? Local Space CarryPoint
                Vector3 minLocal =
                    carryPoint.InverseTransformPoint(
                        new Vector3(
                            bounds.center.x,
                            bounds.min.y,
                            bounds.center.z
                        )
                    );

                Vector3 maxLocal =
                    carryPoint.InverseTransformPoint(
                        new Vector3(
                            bounds.center.x,
                            bounds.max.y,
                            bounds.center.z
                        )
                    );

                lowestPoint =
                    Mathf.Min(
                        lowestPoint,
                        minLocal.y
                    );

                highestPoint =
                    Mathf.Max(
                        highestPoint,
                        maxLocal.y
                    );
            }


            // ?????? ????? Mesh
            float itemHeight =
                highestPoint - lowestPoint;


            // ???? ???:
            // ?????????? ???? Mesh ?????? ??? CarryPoint
            //
            // ???????? ????:
            // ?????????? ???? Mesh ??? ???????? ???? ???? ????
            // + ????? ????

            float targetBottom =
                currentTop;


            float offsetY =
                targetBottom - lowestPoint;


            item.transform.localPosition =
                new Vector3(
                    0f,
                    item.transform.localPosition.y + offsetY,
                    0f
                );


            // ?????? ???????? ???? ????
            currentTop =
                targetBottom +
                itemHeight +
                stackGap;
        }

        // ?????? ?? Stack ?? ???? ?? ?????
        for (int i = 0; i < carriedItems.Count; i++)
        {
            if (carriedItems[i] == null)
                continue;

            Vector3 position =
                carriedItems[i].transform.localPosition;

            position.y += carryHeightOffset;

            carriedItems[i].transform.localPosition =
                position;
        }
    }


    // ==============================
    // GET REAL ITEM BOUNDS
    // ==============================

    private Bounds GetItemBounds(GameObject item)
    {
        Renderer[] renderers =
            item.GetComponentsInChildren<Renderer>();


        if (renderers.Length == 0)
        {
            return new Bounds(
                item.transform.position,
                Vector3.one * 0.1f
            );
        }


        Bounds bounds =
            renderers[0].bounds;


        for (int i = 1;
             i < renderers.Length;
             i++)
        {
            bounds.Encapsulate(
                renderers[i].bounds
            );
        }


        return bounds;
    }


    // ==============================
    // UPDATE CARRY STATE
    // ==============================

    private void UpdateCarryState()
    {
        bool carrying =
            carriedItems.Count > 0;


        if (animator != null)
        {
            animator.SetBool(
                "IsCarry",
                carrying
            );
        }


        if (playerController != null)
        {
            playerController.SetCarryState(
                carrying
            );
        }
    }

    public GameObject GetTopItem()
    {
        if (carriedItems.Count == 0)
            return null;

        // ????? ???? ???? = ???????? ???? Stack
        return carriedItems[carriedItems.Count - 1];
    }


    public GameObject RemoveTopItem()
    {
        if (carriedItems.Count == 0)
            return null;

        int lastIndex =
            carriedItems.Count - 1;

        GameObject item =
            carriedItems[lastIndex];

        // ??? ?? Inventory
        carriedItems.RemoveAt(lastIndex);

        if (item != null)
        {
            // ??? ???? ?? ???
            item.transform.SetParent(null);

            // ???? ???? ?????
            Rigidbody rb =
                item.GetComponent<Rigidbody>();

            if (rb != null)
            {
                rb.isKinematic = false;
            }

            // ???? ???? Collider
            Collider col =
                item.GetComponent<Collider>();

            if (col != null)
            {
                col.enabled = true;
            }
        }

        // ???? ???? ???????? ??????????
        RebuildCarryStack();

        // ????? ???? Carry
        UpdateCarryState();

        return item;
    }

    public void SetBurgerCarry(bool state)
    {
        carryingBurger = state;


        if (animator != null)
        {
            animator.SetBool(
                "IsCarry",
                state
            );
        }


        if (playerController != null)
        {
            playerController.SetCarryState(state);
        }
    }

    public bool HasBurger()
    {
        foreach (GameObject item in carriedItems)
        {
            if (item == null)
                continue;


            Burger burger =
                item.GetComponent<Burger>();


            if (burger != null)
            {
                return true;
            }
        }


        return false;
    }

}