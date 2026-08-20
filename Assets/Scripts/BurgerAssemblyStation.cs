using System.Collections.Generic;
using UnityEngine;

public class BurgerAssemblyStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;


    [Header("Assembly")]
    [SerializeField] private Transform assemblyPoint;
    [SerializeField] private Burger burger;


    [Header("Stack")]
    [SerializeField] private float stackGap = 0.01f;


    private List<GameObject> burgerItems = new List<GameObject>();

    private float currentTop = 0f;

    private bool burgerClosed = false;



    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        PlaceItem();
    }





    private void PlaceItem()
    {
        if (assemblyPoint == null)
        {
            Debug.LogError(
                "BurgerAssemblyStation: Assembly Point is missing or destroyed!"
            );

            return;
        }

        if (playerPickup == null)
        {
            Debug.LogError(
                "BurgerAssemblyStation: PlayerPickup is missing!"
            );

            return;
        }

        if (burgerClosed)
        {
            Debug.Log("Burger is closed!");
            return;
        }


        // ==========================================
        // ???? ???? ???? ?????
        // ==========================================

        GameObject item =
            playerPickup.GetTopItem();


        if (item == null)
            return;


        Item itemData =
            item.GetComponent<Item>();


        if (itemData == null)
            return;


        // ==========================================
        // TOP BUN
        // ==========================================

        if (playerPickup.HasItem(ItemType.BunTop))
        {
            GameObject topBun =
                playerPickup.RemoveItem(ItemType.BunTop);

            if (topBun == null)
                return;


            Item topBunData =
                topBun.GetComponent<Item>();


            if (topBunData == null)
                return;


            // Top Bun ???? ??? ?? ????? ?? ???? ???? ?????
            if (burgerItems.Count == 0)
            {
                Debug.Log(
                    "Cannot place top bun first!"
                );

                // ??? ??????? Top Bun ?? ?????????
                // ????? ??? ??????????
                playerPickup.TryPickup(topBun);

                return;
            }


            PlaceItemOnBurger(
                topBun,
                topBunData
            );


            burgerClosed = true;


            BurgerPickupStation pickup =
                GetComponent<BurgerPickupStation>();


            if (pickup != null)
            {
                pickup.SetBurgerReady(true);
            }


            Debug.Log(
                "Burger Completed!"
            );

            return;
        }


        // ==========================================
        // ???????? ??????
        // ==========================================

        if (!itemData.CanAssemble)
        {
            Debug.Log("Cannot assemble");
            return;
        }


        // ????? ???? ???? Bottom Bun ????
        if (burgerItems.Count == 0)
        {
            if (itemData.Type != ItemType.BunBottem)
            {
                Debug.Log(
                    "First item must be bottom bun"
                );

                return;
            }
        }


        // ???? ?????? ?? ?? ????? Stack ?????
        GameObject placedItem =
            playerPickup.RemoveTopItem();


        if (placedItem == null)
            return;


        PlaceItemOnBurger(
            placedItem,
            itemData
        );
    }






    private float GetBottomPoint(GameObject obj)
    {
        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>();


        float min =
            float.MaxValue;



        foreach (Renderer r in renderers)
        {
            Bounds b = r.bounds;


            Vector3 local =
                assemblyPoint.InverseTransformPoint(
                    new Vector3(
                        b.center.x,
                        b.min.y,
                        b.center.z
                    )
                );


            min = Mathf.Min(
                min,
                local.y
            );
        }


        return min;
    }






    private float GetTopPoint(GameObject obj)
    {
        Renderer[] renderers =
            obj.GetComponentsInChildren<Renderer>();


        float max =
            float.MinValue;



        foreach (Renderer r in renderers)
        {
            Bounds b = r.bounds;


            Vector3 local =
                assemblyPoint.InverseTransformPoint(
                    new Vector3(
                        b.center.x,
                        b.max.y,
                        b.center.z
                    )
                );


            max = Mathf.Max(
                max,
                local.y
            );
        }


        return max;
    }

    public void ResetAssembly()
    {
        // ??? ???? ???????? ???? ?? ??? ???
        for (int i = burgerItems.Count - 1; i >= 0; i--)
        {
            GameObject item = burgerItems[i];

            if (item != null)
            {
                Destroy(item);
            }
        }


        burgerItems.Clear();


        currentTop = 0f;


        burgerClosed = false;


        // ???? ??????? Burger
        if (burger != null)
        {
            burger.ResetBurger();
        }


        Debug.Log(
            "Burger Assembly Completely Reset!"
        );
    }

    private void PlaceItemOnBurger(
    GameObject placedItem,
    Item itemData
)
    {
        placedItem.transform.SetParent(
            assemblyPoint,
            false
        );

        placedItem.transform.localPosition =
            Vector3.zero;

        placedItem.transform.localRotation =
            Quaternion.identity;

        placedItem.transform.localScale =
            Vector3.one;


        float bottom =
            GetBottomPoint(placedItem);


        float offset =
            currentTop -
            bottom +
            stackGap;


        placedItem.transform.localPosition =
            new Vector3(
                0,
                placedItem.transform.localPosition.y +
                offset,
                0
            );


        burgerItems.Add(
            placedItem
        );


        if (burger != null)
        {
            burger.AddItem(
                itemData.Type
            );
        }


        currentTop =
            GetTopPoint(placedItem);
    }

}