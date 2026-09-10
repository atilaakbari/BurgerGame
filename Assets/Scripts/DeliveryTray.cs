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

    // =========================================================
    // PROPERTIES
    // =========================================================

    public bool HasBurger =>
        burger != null;

    public bool HasSoda =>
        soda != null;

    public int ItemCount
    {
        get
        {
            int count = 0;

            if (burger != null)
                count++;

            if (soda != null)
                count++;

            return count;
        }
    }

    public bool IsFull =>
        HasBurger && HasSoda;

    // =========================================================
    // ADD BURGER
    // =========================================================

    public bool AddBurger(GameObject item)
    {
        if (item == null)
            return false;

        if (burger != null)
        {
            Debug.LogWarning(
                "DeliveryTray already has a Burger!"
            );

            return false;
        }

        if (burgerPoint == null)
        {
            Debug.LogError(
                "DeliveryTray: Burger Point is not assigned!"
            );

            return false;
        }

        if (itemParent == null)
        {
            Debug.LogError(
                "DeliveryTray: Item Parent is not assigned!"
            );

            return false;
        }

        burger = item;

        // Burger زیر ItemParent قرار می‌گیرد
        item.transform.SetParent(
            itemParent
        );

        // ولی موقعیتش از BurgerPoint گرفته می‌شود
        item.transform.position =
            burgerPoint.position;

        item.transform.rotation =
            burgerPoint.rotation;

        return true;
    }

    // =========================================================
    // ADD SODA
    // =========================================================

    public bool AddSoda(GameObject item)
    {
        if (item == null)
            return false;

        if (soda != null)
        {
            Debug.LogWarning(
                "DeliveryTray already has a Soda!"
            );

            return false;
        }

        if (sodaPoint == null)
        {
            Debug.LogError(
                "DeliveryTray: Soda Point is not assigned!"
            );

            return false;
        }

        if (itemParent == null)
        {
            Debug.LogError(
                "DeliveryTray: Item Parent is not assigned!"
            );

            return false;
        }

        soda = item;

        // Soda هم زیر همان ItemParent قرار می‌گیرد
        item.transform.SetParent(
            itemParent
        );

        // ولی جای مخصوص خودش را دارد
        item.transform.position =
            sodaPoint.position;

        item.transform.rotation =
            sodaPoint.rotation;

        return true;
    }

    // =========================================================
    // CHECK
    // =========================================================

    public bool ContainsBurger()
    {
        return burger != null;
    }

    public bool ContainsSoda()
    {
        return soda != null;
    }

    // =========================================================
    // GET
    // =========================================================

    public GameObject GetBurger()
    {
        return burger;
    }

    public GameObject GetSoda()
    {
        return soda;
    }

    // =========================================================
    // CLEAR
    // =========================================================

    public void ClearTray()
    {
        burger = null;
        soda = null;
    }
}