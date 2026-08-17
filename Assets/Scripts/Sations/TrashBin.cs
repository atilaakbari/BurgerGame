using System.Collections;
using UnityEngine;

public class TrashBin : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;


    [Header("Burger")]
    [SerializeField] private Transform burgerRoot;


    [Header("Trash Settings")]
    [SerializeField] private float trashDelay = 0.5f;


    private bool playerInside = false;
    private Coroutine trashCoroutine;



    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        playerInside = true;


        if (trashCoroutine == null)
        {
            trashCoroutine =
                StartCoroutine(EmptyInventory());
        }
    }




    private void OnTriggerExit(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        playerInside = false;


        if (trashCoroutine != null)
        {
            StopCoroutine(trashCoroutine);
            trashCoroutine = null;
        }
    }





    private IEnumerator EmptyInventory()
    {
        while (playerInside)
        {

            // ??? ???? ??? ?????? ???
            if (playerPickup.IsCarrying)
            {
                GameObject item =
                    playerPickup.DropItem();


                if (item != null)
                {
                    Destroy(item);


                    Debug.Log(
                        "Item Destroyed By Trash"
                    );
                }
            }



            // ??? ???? ??? ???? ???
            else if (burgerRoot != null &&
                    burgerRoot.parent != null)
            {
                Destroy(burgerRoot.gameObject);


                playerPickup.SetBurgerCarry(false);


                Debug.Log(
                    "Burger Destroyed By Trash"
                );
            }
            else
            {
                break;
            }



            yield return new WaitForSeconds(
                trashDelay
            );
        }


        trashCoroutine = null;
    }
}