using System.Collections;
using UnityEngine;

public class ItemSpawnerStation : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private GameObject itemPrefab;

    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Give Settings")]
    [SerializeField] private bool fillInventory = false;

    [Min(1)]
    [SerializeField] private int giveCount = 1;

    [Header("Spawn Delay")]
    [SerializeField] private float spawnDelay = 0.5f;

    private bool playerInside = false;
    private Coroutine giveCoroutine;


    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = true;

        Debug.Log("Player entered station!");

        // ???? ???? ????
        if (giveCoroutine == null)
        {
            giveCoroutine = StartCoroutine(GiveItems());
        }
    }


    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;

        playerInside = false;

        Debug.Log("Player left station!");

        // ????? ???? ???? ????
        if (giveCoroutine != null)
        {
            StopCoroutine(giveCoroutine);
            giveCoroutine = null;
        }
    }


    private IEnumerator GiveItems()
    {
        // ?? ???? ???? ???? Station ???
        while (playerInside)
        {
            // ??? Prefab ????? ????
            if (itemPrefab == null)
            {
                Debug.LogError(
                    "Item Prefab is not assigned!",
                    this
                );

                giveCoroutine = null;
                yield break;
            }


            // ??? PlayerPickup ????? ????
            if (playerPickup == null)
            {
                Debug.LogError(
                    "Player Pickup is not assigned!",
                    this
                );

                giveCoroutine = null;
                yield break;
            }


            // ??? Inventory ?? ???
            if (!playerPickup.HasSpace)
            {
                // ??? Fill Inventory ???? ???
                // ????? ???????? ?? Player ????? ??????
                if (fillInventory)
                {
                    yield return null;
                    continue;
                }

                break;
            }


            // ????? ????? ????
            int amountToGive;


            if (fillInventory)
            {
                // ??? ?? ???? ???
                // ??? ?????? ????? ??
                amountToGive = 1;
            }
            else
            {
                // ????? ????
                amountToGive = Mathf.Min(
                    giveCount,
                    playerPickup.AvailableSpace
                );
            }


            // ???? ????
            for (int i = 0; i < amountToGive; i++)
            {
                // ??? Player ?? Station ???? ??
                if (!playerInside)
                {
                    giveCoroutine = null;
                    yield break;
                }


                // ??? ???? ??? ???? ????
                if (!playerPickup.HasSpace)
                    break;


                // ???? ????
                GameObject item =
                    Instantiate(itemPrefab);


                // ???? ???? Pickup
                bool success =
                    playerPickup.TryPickup(item);


                if (!success)
                {
                    Destroy(item);
                    break;
                }


                Debug.Log(
                    "Picked up: " + item.name
                );


                // ??? Fill Inventory ????? ???
                // ??? ????? ???? ?? ???
                if (!fillInventory)
                {
                    if (i < amountToGive - 1)
                    {
                        yield return new WaitForSeconds(
                            spawnDelay
                        );
                    }
                }
                else
                {
                    // ???? ???? ????
                    yield return new WaitForSeconds(
                        spawnDelay
                    );
                }
            }


            // ??? Fill Inventory ????? ???
            // ??? ???? ???
            if (!fillInventory)
                break;
        }


        giveCoroutine = null;
    }
}