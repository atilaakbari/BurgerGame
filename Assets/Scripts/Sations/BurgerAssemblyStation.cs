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

        // بیرون تریگر یا برگر بسته شده
        if (!playerInside || burgerClosed)
        {
            placeButton.SetActive(false);
            return;
        }

        if (playerPickup == null)
        {
            placeButton.SetActive(false);
            return;
        }

        // اگر برگر کامل دستش است → دکمه Place خاموش
        if (playerPickup.HasBurger())
        {
            placeButton.SetActive(false);
            return;
        }

        GameObject topItem = playerPickup.GetTopItem();
        if (topItem == null)
        {
            placeButton.SetActive(false);
            return;
        }

        // خود آبجکت برگر روی دست
        if (topItem.GetComponent<Burger>() != null)
        {
            placeButton.SetActive(false);
            return;
        }

        Item itemData = topItem.GetComponent<Item>();
        if (itemData == null || !itemData.CanAssemble)
        {
            placeButton.SetActive(false);
            return;
        }

        // فقط وقتی واقعاً بشود گذاشت (مثلاً اول باید نون زیر باشد)
        if (!CanPlaceItem(itemData))
        {
            placeButton.SetActive(false);
            return;
        }

        placeButton.SetActive(true);

        if (placeButtonIcon != null)
        {
            Sprite sprite = GetIconSprite(itemData.Type);
            if (sprite != null)
                placeButtonIcon.sprite = sprite;
        }
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

    /// <summary>
    /// همان قوانین PlaceItem — بدون برداشتن از دست
    /// </summary>
    private bool CanPlaceItem(Item itemData)
    {
        if (itemData == null)
            return false;

        if (!itemData.CanAssemble)
            return false;

        // میز خالی → فقط نون پایین
        if (burgerItems.Count == 0)
            return itemData.Type == ItemType.BunBottem;

        // نون بالا نمی‌تواند اول باشد (بالا پوشش داده شده)
        // بقیه لایه‌ها بعد از نون پایین OK هستند
        // اگر قانون دیگری داری اینجا اضافه کن

        return true;
    }

    // =========================================================
    // Worker API
    // =========================================================

    public bool IsBurgerClosed => burgerClosed;
    public int AssembledCount => burgerItems.Count;

    public List<ItemType> GetAssembledTypes()
    {
        if (burger != null && burger.items != null)
            return new List<ItemType>(burger.items);
        return new List<ItemType>();
    }

    public bool TryPlaceItemFromWorker(GameObject item)
    {
        if (item == null || burgerClosed || assemblyPoint == null)
            return false;

        Item itemData = item.GetComponent<Item>();
        if (itemData == null || !itemData.CanAssemble)
            return false;

        if (burgerItems.Count == 0 && itemData.Type != ItemType.BunBottem)
            return false;

        if (itemData.Type == ItemType.BunTop && burgerItems.Count == 0)
            return false;

        item.transform.SetParent(null);
        PlaceItemOnBurger(item, itemData);

        if (itemData.Type == ItemType.BunTop)
        {
            burgerClosed = true;
            BurgerPickupStation pickup = GetComponent<BurgerPickupStation>();
            if (pickup != null)
                pickup.SetBurgerReady(true);
        }

        return true;
    }

    /// <summary>
    /// برگر کامل را برای وورکر برمی‌دارد و میز را ریست می‌کند
    /// </summary>
    public GameObject TakeCompletedBurgerForWorker()
    {
        if (!burgerClosed || burger == null || burgerRootMissing())
            return null;

        // burger روی assemblyPoint / burger component
        Transform root = burger.transform;
        if (root == null)
            return null;

        if (burger.items == null || burger.items.Count == 0)
            return null;

        GameObject clone = Instantiate(root.gameObject);
        clone.name = "Burger_Worker";

        Burger cloneBurger = clone.GetComponent<Burger>();
        if (cloneBurger != null)
            cloneBurger.items = new List<ItemType>(burger.items);

        BurgerAssemblyStation ca = clone.GetComponent<BurgerAssemblyStation>();
        if (ca != null) ca.enabled = false;
        BurgerPickupStation cp = clone.GetComponent<BurgerPickupStation>();
        if (cp != null) cp.enabled = false;

        foreach (var col in clone.GetComponentsInChildren<Collider>())
            col.enabled = false;
        foreach (var rb in clone.GetComponentsInChildren<Rigidbody>())
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        ResetAssembly();
        return clone;
    }

    private bool burgerRootMissing()
    {
        return burger == null;
    }

}