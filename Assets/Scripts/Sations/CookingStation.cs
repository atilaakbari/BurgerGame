using UnityEngine;

public class CookingStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Pans (????? CookingSlot ??? ??? ?? 3 ??? ??? ?? ????? ???)")]
    [SerializeField] private CookingSlot[] pans;

    [Header("Item")]
    [SerializeField] private GameObject cookedPattyPrefab;

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        // ???: ??? ?? ?? ????? ? ???? ?????? ?????
        CookingSlot readySlot = GetActiveReadySlot();

        if (readySlot != null)
        {
            TakeCookedPatty(readySlot);
            return;
        }

        // ???: ??? ???? ??? ????? ???? ?? ?? ?? ???? ? ????
        TryPlaceRawPatty();
    }

    // ==========================================================
    // ??? ??????? ?? ???? ???? ?? ???? ????? (???? ?????? ??????) ?? ?? ??? ????????.
    // ??? CookingStationUpgrade ???? ?? ??? ?? ??? ?? SetActive/??????? ???????
    // ?????? ??????? ??? ?????? ??????? ???? ??????? ??????? - ????? ?? ????? ???? ????.
    // ==========================================================

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
        {
            Debug.Log("No item in hand");
            return;
        }

        Item itemData = topItem.GetComponent<Item>();

        if (itemData == null || itemData.Type != ItemType.RawPatty)
        {
            Debug.Log("Top item is not Raw Patty!");
            return;
        }

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