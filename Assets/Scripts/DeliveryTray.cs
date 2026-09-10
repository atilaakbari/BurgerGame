using UnityEngine;

public class DeliveryTray : MonoBehaviour
{
    [Header("Items Parent")]
    [SerializeField] private Transform itemParent;

    [Header("Burger Point")]
    [SerializeField] private Transform burgerPoint;

    [Header("Soda Point")]
    [SerializeField] private Transform sodaPoint;

    private GameObject burger;
    private GameObject soda;

    public bool HasBurger => burger != null;
    public bool HasSoda => soda != null;

    public int ItemCount
    {
        get
        {
            int count = 0;
            if (burger != null) count++;
            if (soda != null) count++;
            return count;
        }
    }

    public bool IsFull => HasBurger && HasSoda;

    public bool ContainsBurger() => burger != null;
    public bool ContainsSoda() => soda != null;

    public GameObject GetBurger() => burger;
    public GameObject GetSoda() => soda;

    public bool AddBurger(GameObject item)
    {
        if (item == null || burger != null)
            return false;

        if (burgerPoint == null || itemParent == null)
        {
            Debug.LogError("DeliveryTray: BurgerPoint or ItemParent missing!");
            return false;
        }

        burger = item;
        PlaceOnPoint(item, burgerPoint);
        return true;
    }

    public bool AddSoda(GameObject item)
    {
        if (item == null || soda != null)
            return false;

        if (sodaPoint == null || itemParent == null)
        {
            Debug.LogError("DeliveryTray: SodaPoint or ItemParent missing!");
            return false;
        }

        soda = item;
        PlaceOnPoint(item, sodaPoint);
        return true;
    }

    private void PlaceOnPoint(GameObject item, Transform point)
    {
        // اول زیر itemParent، بعد موقعیت local از روی point
        item.transform.SetParent(itemParent, false);

        // اگر point زیر tray/itemParent باشد، local آن را کپی می‌کنیم
        if (point.parent == itemParent || point.IsChildOf(itemParent))
        {
            item.transform.localPosition = point.localPosition;
            item.transform.localRotation = point.localRotation;
        }
        else
        {
            // نقطه خارج از parent است → تبدیل به فضای itemParent
            item.transform.position = point.position;
            item.transform.rotation = point.rotation;
        }
    }

    public void ClearTray()
    {
        burger = null;
        soda = null;
    }
}