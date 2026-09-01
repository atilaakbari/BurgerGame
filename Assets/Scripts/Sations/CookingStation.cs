using System.Collections.Generic;
using UnityEngine;

public class CookingStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Pans (همه CookingSlot ها - از سطح 1 تا 3)")]
    [SerializeField] private CookingSlot[] pans;

    [Header("Item")]
    [SerializeField] private GameObject cookedPattyPrefab;

    [Header("Pickup Button (دکمه‌ای که خودت می‌سازی)")]
    [SerializeField] private GameObject pickupButton;

    private bool playerInside;

    private void OnEnable()
    {
        CookingStationUpgrade.OnStationUpgraded += OnStationUpgraded;
    }

    private void OnDisable()
    {
        CookingStationUpgrade.OnStationUpgraded -= OnStationUpgraded;
    }

    private void Start()
    {
        if (pickupButton != null)
            pickupButton.SetActive(false);
    }

    private void Update()
    {
        RefreshPickupButton();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        // دیگه خودکار پیکاپ نمی‌کنیم
        // فقط سعی می‌کنیم پتی خام بذاریم
        TryPlaceRawPatty();
        RefreshPickupButton();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;
        RefreshPickupButton();
    }

    // =========================================================
    // این متد رو به دکمه UI وصل کن (OnClick)
    // =========================================================
    public void OnPickupButtonPressed()
    {
        CookingSlot readySlot = GetActiveReadySlot();
        if (readySlot == null)
            return;

        TakeCookedPatty(readySlot);
        RefreshPickupButton();
    }

    private void RefreshPickupButton()
    {
        if (pickupButton == null)
            return;

        bool show = playerInside && GetActiveReadySlot() != null;
        pickupButton.SetActive(show);
    }

    // =========================================================
    // انتقال وضعیت موقع ارتقا
    // =========================================================
    private void OnStationUpgraded(CookingStationUpgrade upgrade)
    {
        // وضعیت فعلی رو قبل از اینکه مدل کامل عوض بشه ذخیره کردیم
        // چون event بعد از ApplyLevelVisuals صدا زده می‌شه،
        // باید وضعیت رو قبل از Upgrade ذخیره کنیم.
        // پس در CookingStationUpgrade.Upgrade قبل از Apply صدا می‌زنیم.
    }

    // این متد رو از CookingStationUpgrade صدا می‌زنیم (قبل از عوض شدن مدل)
    public List<CookingSlot.SlotState> CaptureAllActiveStates()
    {
        List<CookingSlot.SlotState> states = new List<CookingSlot.SlotState>();

        foreach (CookingSlot slot in pans)
        {
            if (slot != null && slot.gameObject.activeInHierarchy)
                states.Add(slot.CaptureState());
        }

        return states;
    }

    // این متد رو بعد از عوض شدن مدل صدا می‌زنیم
    public void RestoreStates(List<CookingSlot.SlotState> states)
    {
        if (states == null || states.Count == 0)
            return;

        // اول همه اسلات‌های فعال جدید رو ریست کن
        List<CookingSlot> activeSlots = new List<CookingSlot>();
        foreach (CookingSlot slot in pans)
        {
            if (slot != null && slot.gameObject.activeInHierarchy)
            {
                slot.ResetSlot();
                activeSlots.Add(slot);
            }
        }

        // وضعیت‌های غیرخالی رو به ترتیب روی اسلات‌های جدید بگذار
        int targetIndex = 0;
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].type == CookingSlot.SlotStateType.Empty)
                continue;

            if (targetIndex >= activeSlots.Count)
                break; // دیگه اسلات خالی نداریم

            activeSlots[targetIndex].ApplyState(states[i]);
            targetIndex++;
        }

        RefreshPickupButton();
    }

    // =========================================================
    // منطق اصلی
    // =========================================================
    private CookingSlot GetActiveReadySlot()
    {
        foreach (CookingSlot slot in pans)
        {
            if (slot != null && slot.gameObject.activeInHierarchy && slot.IsReady)
                return slot;
        }
        return null;
    }

    private CookingSlot GetActiveEmptySlot()
    {
        foreach (CookingSlot slot in pans)
        {
            if (slot != null && slot.gameObject.activeInHierarchy && slot.IsEmpty)
                return slot;
        }
        return null;
    }

    private void TryPlaceRawPatty()
    {
        if (playerPickup == null)
            return;

        GameObject topItem = playerPickup.GetTopItem();
        if (topItem == null)
            return;

        Item itemData = topItem.GetComponent<Item>();
        if (itemData == null || itemData.Type != ItemType.RawPatty)
            return;

        CookingSlot emptySlot = GetActiveEmptySlot();
        if (emptySlot == null)
        {
            Debug.Log("All pans are busy or locked!");
            return;
        }

        GameObject rawPatty = playerPickup.RemoveTopItem();
        if (rawPatty == null)
            return;

        Destroy(rawPatty);
        emptySlot.TryStartCooking();
    }

    private void TakeCookedPatty(CookingSlot slot)
    {
        if (playerPickup == null || !playerPickup.HasSpace)
        {
            Debug.Log("Inventory is Full!");
            return;
        }

        if (cookedPattyPrefab == null)
        {
            Debug.LogError("Cooked Patty Prefab is not assigned!");
            return;
        }

        GameObject cookedItem = Instantiate(cookedPattyPrefab);
        bool success = playerPickup.TryPickup(cookedItem);

        if (success)
        {
            slot.Collect();
            Debug.Log("Cooked Patty Picked Up!");
        }
        else
        {
            Destroy(cookedItem);
            Debug.Log("Could not pickup Cooked Patty!");
        }
    }
}