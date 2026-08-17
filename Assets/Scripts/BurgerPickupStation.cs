using UnityEngine;

public class BurgerPickupStation : MonoBehaviour
{
    [Header("Burger")]
    [SerializeField] private Transform burgerRoot;

    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("UI")]
    [SerializeField] private GameObject pickButton;


    private bool burgerReady = false;
    private bool playerInside = false;


    private void Start()
    {
        if (pickButton != null)
            pickButton.SetActive(false);
    }


    // =====================================================
    // BURGER READY
    // =====================================================

    public void SetBurgerReady(bool state)
    {
        burgerReady = state;

        if (pickButton != null)
        {
            pickButton.SetActive(
                burgerReady && playerInside
            );
        }
    }


    // =====================================================
    // PLAYER ENTER
    // =====================================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        if (pickButton != null)
            pickButton.SetActive(burgerReady);
    }


    // =====================================================
    // PLAYER EXIT
    // =====================================================

    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        if (pickButton != null)
            pickButton.SetActive(false);
    }


    // =====================================================
    // PICK BURGER
    // =====================================================

    public void PickBurger()
    {
        if (!burgerReady)
            return;


        if (playerPickup == null)
        {
            Debug.LogError(
                "BurgerPickupStation: PlayerPickup is missing!"
            );

            return;
        }


        if (burgerRoot == null)
        {
            Debug.LogError(
                "BurgerPickupStation: BurgerRoot is missing!"
            );

            return;
        }


        if (playerPickup.CurrentCarryCount > 0)
        {
            Debug.Log("Hand is not empty!");
            return;
        }


        Burger originalBurger =
            burgerRoot.GetComponent<Burger>();


        if (originalBurger == null)
        {
            Debug.LogError(
                "Burger component not found on BurgerRoot!"
            );

            return;
        }


        if (originalBurger.items == null ||
            originalBurger.items.Count == 0)
        {
            Debug.LogError(
                "Burger has no items!"
            );

            return;
        }


        // =================================================
        // ???? ??? ???? ???? ??? ????
        // =================================================

        GameObject burgerClone =
            Instantiate(
                burgerRoot.gameObject
            );


        burgerClone.name =
            "Burger_Carry";


        // ??? Burger
        Burger cloneBurger =
            burgerClone.GetComponent<Burger>();


        if (cloneBurger == null)
        {
            Debug.LogError(
                "Burger component missing on clone!"
            );

            Destroy(burgerClone);
            return;
        }


        // ????? ?? ???? ?????? ??? ???? ???? ???
        cloneBurger.items =
            new System.Collections.Generic.List<ItemType>(
                originalBurger.items
            );


        // =================================================
        // ????? ????? ??? ?? ??? ??? ????? ??
        // =================================================

        BurgerAssemblyStation cloneAssembly =
            burgerClone.GetComponent<BurgerAssemblyStation>();


        if (cloneAssembly != null)
            cloneAssembly.enabled = false;


        BurgerPickupStation clonePickup =
            burgerClone.GetComponent<BurgerPickupStation>();


        if (clonePickup != null)
            clonePickup.enabled = false;


        // ???? Collider??? ??? ?????
        Collider[] colliders =
            burgerClone.GetComponentsInChildren<Collider>();


        foreach (Collider col in colliders)
        {
            col.enabled = false;
        }


        // Rigidbody??? ??? Kinematic
        Rigidbody[] rigidbodies =
            burgerClone.GetComponentsInChildren<Rigidbody>();


        foreach (Rigidbody rb in rigidbodies)
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }


        // =================================================
        // ????? ??? ?? PlayerPickup
        // =================================================

        bool success =
            playerPickup.TryPickup(
                burgerClone
            );


        if (!success)
        {
            Debug.Log(
                "Cannot pickup burger!"
            );

            Destroy(burgerClone);
            return;
        }


        // =================================================
        // ???? ???? ??? ??? ???? ???
        // =================================================

        if (GetComponent<BurgerAssemblyStation>() != null)
        {
            GetComponent<BurgerAssemblyStation>()
                .ResetAssembly();
        }


        burgerReady = false;


        if (pickButton != null)
            pickButton.SetActive(false);


        Debug.Log(
            "Burger Picked Successfully!"
        );
    }
}