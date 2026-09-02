using System.Collections.Generic;
using UnityEngine;

public class CuttingStation : MonoBehaviour
{
    [System.Serializable]
    public class CuttingRecipe
    {
        public ItemType inputType;
        public GameObject outputPrefab;
        [Min(1)] public int outputCount = 1;
        public float cuttingTime = 3f;
        public Vector3 inputScaleOnBoard = Vector3.one;
        public Vector3 outputScaleOnBoard = Vector3.one;
    }

    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Slots (دو تا تخته برش)")]
    [SerializeField] private CuttingSlot[] slots;

    [Header("Recipes")]
    [SerializeField] private CuttingRecipe[] recipes;

    [Header("Pickup Buttons")]
    [SerializeField] private GameObject[] pickupButtons;

    private bool playerInside;

    private void Start()
    {
        if (pickupButtons != null)
            for (int i = 0; i < pickupButtons.Length; i++)
                if (pickupButtons[i] != null) pickupButtons[i].SetActive(false);
    }

    private void Update()
    {
        RefreshPickupButtons();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = true;
        TryPlaceItem();
        RefreshPickupButtons();
    }

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player")) return;

        playerInside = false;
        RefreshPickupButtons();
    }

    public void OnPickupButtonPressed(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= slots.Length) return;

        CuttingSlot readySlot = GetActiveReadySlot(slotIndex);
        if (readySlot == null) return;

        TakeOneOutput(readySlot);
        RefreshPickupButtons();
    }

    private void RefreshPickupButtons()
    {
        if (pickupButtons == null || slots == null) return;

        for (int i = 0; i < pickupButtons.Length; i++)
        {
            if (pickupButtons[i] == null) continue;

            CuttingSlot slot = GetActiveReadySlot(i);
            bool show = playerInside && slot != null;
            pickupButtons[i].SetActive(show);
        }
    }

    // ========== انتقال وضعیت ==========
    public List<CuttingSlot.SlotState> CaptureAllActiveStates()
    {
        List<CuttingSlot.SlotState> states = new List<CuttingSlot.SlotState>();
        foreach (CuttingSlot slot in slots)
        {
            if (slot != null && slot.gameObject.activeInHierarchy)
                states.Add(slot.CaptureState());
        }
        return states;
    }

    public void RestoreStates(List<CuttingSlot.SlotState> states)
    {
        if (states == null || states.Count == 0) return;

        List<CuttingSlot> activeSlots = new List<CuttingSlot>();
        foreach (CuttingSlot slot in slots)
        {
            if (slot != null && slot.gameObject.activeInHierarchy)
            {
                slot.ResetSlot();
                activeSlots.Add(slot);
            }
        }

        int targetIndex = 0;
        for (int i = 0; i < states.Count; i++)
        {
            if (states[i].type == CuttingSlot.SlotStateType.Empty) continue;

            if (targetIndex >= activeSlots.Count) break;

            CuttingRecipe recipe = FindRecipe(states[i].inputType);
            activeSlots[targetIndex].ApplyState(states[i], recipe);
            targetIndex++;
        }

        RefreshPickupButtons();
    }

    private CuttingSlot GetActiveReadySlot(int index)
    {
        if (index < 0 || index >= slots.Length) return null;
        return slots[index].IsReady ? slots[index] : null;
    }

    private CuttingSlot GetActiveEmptySlot()
    {
        foreach (CuttingSlot slot in slots)
        {
            if (slot != null && slot.gameObject.activeInHierarchy && slot.IsEmpty)
                return slot;
        }
        return null;
    }

    private CuttingRecipe FindRecipe(ItemType type)
    {
        if (recipes == null) return null;
        foreach (CuttingRecipe r in recipes)
            if (r != null && r.inputType == type)
                return r;
        return null;
    }

    private void TryPlaceItem()
    {
        if (playerPickup == null) return;

        GameObject topItem = playerPickup.GetTopItem();
        if (topItem == null) return;

        Item itemData = topItem.GetComponent<Item>();
        if (itemData == null) return;

        CuttingRecipe recipe = FindRecipe(itemData.Type);
        if (recipe == null)
        {
            Debug.Log("این آیتم را نمی‌توان برش داد!");
            return;
        }

        CuttingSlot emptySlot = GetActiveEmptySlot();
        if (emptySlot == null)
        {
            Debug.Log("همه اسلات‌ها شلوغ هستند!");
            return;
        }

        GameObject inputItem = playerPickup.RemoveTopItem();
        if (inputItem == null) return;

        emptySlot.TryStartCutting(inputItem, recipe);
    }

    private void TakeOneOutput(CuttingSlot slot)
    {
        if (playerPickup == null || !playerPickup.HasSpace)
        {
            Debug.Log("انبار پر است!");
            return;
        }

        if (!slot.TryTakeOneOutput(out GameObject outputItem))
            return;

        bool success = playerPickup.TryPickup(outputItem);

        if (!success)
        {
            Destroy(outputItem);
            Debug.Log("نمی‌توان برش را برداشت!");
        }
    }
}