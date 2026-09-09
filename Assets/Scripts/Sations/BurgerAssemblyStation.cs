using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BurgerAssemblyStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;


    [Header("Assembly")]
    [SerializeField] private Transform assemblyPoint;
    [SerializeField] private Burger burger;


    [Header("Stack")]
    [SerializeField] private float stackGap = 0.01f;

    [Header("Place Button (دکمه‌ای که خودت می‌سازی)")]
    [SerializeField] private GameObject placeButton;
    [SerializeField] private Image placeButtonIcon;

    [Header("Clear Button (وقتی رو میز حداقل یه آیتم باشه فعال می‌شه)")]
    [SerializeField] private GameObject clearButton;

    [Header("Item Icons (همون چیزی که تو OrderUI/CuttingStation هم استفاده کردی)")]
    [SerializeField] private List<ItemIconData> itemIcons;


    private List<GameObject> burgerItems = new List<GameObject>();

    private float currentTop = 0f;

    private bool burgerClosed = false;

    private bool playerInside = false;


    private void Update()
    {
        RefreshPlaceButton();
        RefreshClearButton();
    }


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
    }


    // این متد رو به دکمه‌ی UI وصل کن (OnClick)
    public void OnPlaceButtonPressed()
    {
        PlaceItem();
    }


    private void RefreshPlaceButton()
    {
        if (placeButton == null)
            return;

        GameObject topItem = playerPickup != null ? playerPickup.GetTopItem() : null;

        bool show = playerInside && !burgerClosed && topItem != null;

        // اگه آیتمی که دستشه اصلاً قابل Assembly نیست، دکمه نیاد
        if (show)
        {
            Item itemData = topItem.GetComponent<Item>();

            if (itemData == null || !itemData.CanAssemble)
                show = false;
        }

        placeButton.SetActive(show);

        if (!show)
            return;

        if (placeButtonIcon == null)
            return;

        Item iconItemData = topItem.GetComponent<Item>();

        if (iconItemData == null)
            return;

        Sprite sprite = GetIconSprite(iconItemData.Type);

        if (sprite != null)
            placeButtonIcon.sprite = sprite;
    }


    private Sprite GetIconSprite(ItemType type)
    {
        if (itemIcons == null)
            return null;

        foreach (ItemIconData data in itemIcons)
        {
            if (data.type == type)
                return data.sprite;
        }

        return null;
    }


    private void RefreshClearButton()
    {
        if (clearButton == null)
            return;

        clearButton.SetActive(playerInside && burgerItems.Count > 0);
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


        // آیتم بالای دست پلیر
        GameObject item =
            playerPickup.GetTopItem();

        if (item == null)
            return;


        Item itemData =
            item.GetComponent<Item>();

        if (itemData == null)
            return;


        // آیا این آیتم قابل Assembly هست
        if (!itemData.CanAssemble)
        {
            Debug.Log("Cannot assemble");
            return;
        }


        // ==========================================
        // اولین آیتم باید حتما Bottom Bun باشه
        // ==========================================

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


        // ==========================================
        // TOP BUN
        // ==========================================

        if (itemData.Type == ItemType.BunTop)
        {
            // اگه item از GetTopItem گرفتیم
            // یعنی Top Bun اولین آیتمی نیست که میذاره

            if (burgerItems.Count == 0)
            {
                Debug.Log(
                    "Top bun cannot be placed first!"
                );

                return;
            }
        }


        // ==========================================
        // برداشتن آیتم از دست
        // ==========================================

        GameObject placedItem =
            playerPickup.RemoveTopItem();

        if (placedItem == null)
            return;


        PlaceItemOnBurger(
            placedItem,
            itemData
        );


        // ==========================================
        // بستن Burger
        // ==========================================

        if (itemData.Type == ItemType.BunTop)
        {
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
        }
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
        // پاک کردن همه‌ی آیتم‌های روی هم
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


        // ریست کردن Burger
        if (burger != null)
        {
            burger.ResetBurger();
        }


        // دیگه برگری آماده نیست - دکمه‌ی پیکاپ رو خاموش کن
        BurgerPickupStation pickup =
            GetComponent<BurgerPickupStation>();

        if (pickup != null)
        {
            pickup.SetBurgerReady(false);
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