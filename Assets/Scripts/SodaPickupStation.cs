using UnityEngine;

public class SodaPickupStation : MonoBehaviour
{
    [Header("Soda")]
    [SerializeField] private Transform sodaRoot;

    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("UI")]
    [SerializeField] private GameObject pickButton;

    private bool sodaReady;
    private bool playerInside;

    private void Start()
    {
        if (pickButton != null)
            pickButton.SetActive(false);
    }

    public void SetSodaReady(bool ready)
    {
        sodaReady = ready;

        UpdateButton();
    }

    private void OnTriggerEnter(
        Collider other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        UpdateButton();
    }

    private void OnTriggerExit(
        Collider other
    )
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        UpdateButton();
    }

    private void UpdateButton()
    {
        if (pickButton == null)
            return;

        pickButton.SetActive(
            sodaReady &&
            playerInside
        );
    }

    public void PickSoda()
    {
        if (!sodaReady)
            return;

        if (playerPickup == null)
            return;

        if (sodaRoot == null)
        {
            Debug.LogError(
                "Soda Root is missing!"
            );

            return;
        }

        if (!playerPickup.HasSpace)
        {
            Debug.Log(
                "No carry space for soda."
            );

            return;
        }

        GameObject sodaClone =
            Instantiate(
                sodaRoot.gameObject
            );

        sodaClone.name =
            "Soda_Carry";

        Item sodaItem =
            sodaClone.GetComponent<Item>();

        if (sodaItem == null)
        {
            Debug.LogError(
                "Soda prefab does not have Item component!"
            );

            Destroy(sodaClone);

            return;
        }

        if (sodaItem.Type != ItemType.Soda)
        {
            Debug.LogError(
                "Soda prefab ItemType is not Soda!"
            );

            Destroy(sodaClone);

            return;
        }

        Collider[] colliders =
            sodaClone.GetComponentsInChildren<Collider>();

        foreach (
            Collider col
            in colliders
        )
        {
            col.enabled = false;
        }

        Rigidbody[] rigidbodies =
            sodaClone.GetComponentsInChildren<Rigidbody>();

        foreach (
            Rigidbody rb
            in rigidbodies
        )
        {
            rb.isKinematic = true;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        bool success =
            playerPickup.TryPickup(
                sodaClone
            );

        if (!success)
        {
            Destroy(sodaClone);
            return;
        }

        sodaReady = false;

        UpdateButton();

        Debug.Log(
            "Soda Picked Successfully!"
        );
    }
}