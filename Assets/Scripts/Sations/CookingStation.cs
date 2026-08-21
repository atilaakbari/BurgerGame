using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookingStation : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private PlayerPickup playerPickup;

    [Header("Cooking")]
    [SerializeField] private float cookingTime = 5f;

    [Header("Items")]
    [SerializeField] private GameObject cookedPattyPrefab;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Image timerFill;
    [SerializeField] private GameObject timerTick;
    [SerializeField] private float TickDelay;

    [SerializeField] private Color cookingColor = Color.red;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Animation")]
    [SerializeField] private Animator cookingAnimator;
    [SerializeField] private GameObject animationRawPatty;
    [SerializeField] private GameObject animationCookedPatty;

    private bool isCooking = false;
    private bool cookedReady = false;


    private void Start()
    {
        if (timerObject != null)
            timerObject.SetActive(false);

        if (timerTick != null)
            timerTick.SetActive(false);

        if (animationRawPatty != null)
            animationRawPatty.SetActive(false);

        if (animationCookedPatty != null)
            animationCookedPatty.SetActive(false);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        // ??? ???? ???? ????? ???
        if (cookedReady)
        {
            TakeCookedPatty();
            return;
        }


        // ??? ?? ??? ??? ???
        if (isCooking)
        {
            Debug.Log("Patty is still cooking!");
            return;
        }



        // ????? ???? ????? ???
        GameObject topItem = playerPickup.GetTopItem();


        if (topItem == null)
        {
            Debug.Log("No item in hand");
            return;
        }



        Item itemData =
            topItem.GetComponent<Item>();


        if (itemData == null)
            return;



        // ??? RawPatty
        if (itemData.Type != ItemType.RawPatty)
        {
            Debug.Log("Top item is not Raw Patty!");
            return;
        }



        // ??? ???? ???? ????
        GameObject rawPatty =
            playerPickup.RemoveTopItem();



        if (rawPatty == null)
            return;

        if (isCooking == false)
        {
            Destroy(rawPatty);


            StartCoroutine(CookPatty());
        }
        else { return; }
    }


    private IEnumerator CookPatty()
    {
        isCooking = true;

        if (cookingAnimator != null)
            cookingAnimator.SetBool("IsCooking", true);

        if (timerObject != null)
            timerObject.SetActive(true);

        if (timerTick != null)
            timerTick.SetActive(false);

        if (animationRawPatty != null)
            animationRawPatty.SetActive(true);

        if (timerFill != null)
            timerFill.fillAmount = 0f;

        float timer = 0f;

        while (timer < cookingTime)
        {
            timer += Time.deltaTime;

            // ???? ?????? ???
            float progress = timer / cookingTime;

            // ?? ??? ?????
            if (timerFill != null)
            {
                timerFill.fillAmount = progress;
                timerFill.color = Color.Lerp(Color.red, Color.green, progress);
            }

            yield return null;
        }

        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = Color.green;
        }

        if (cookingAnimator != null)
            cookingAnimator.SetBool("IsCooking", false);

        cookedReady = true;

        if (animationRawPatty != null)
            animationRawPatty.SetActive(false);

        if (animationCookedPatty != null)
            animationCookedPatty.SetActive(true);

        isCooking = false;

        yield return new WaitForSeconds(TickDelay);

        if (timerTick != null)
            timerTick.SetActive(true);
    }



    private void TakeCookedPatty()
    {
        // ??? ???? ????? ????
        if (!cookedReady)
            return;

        // ??? Inventory ?? ???
        if (!playerPickup.HasSpace)
        {
            Debug.Log("Inventory is Full!");
            return;
        }

        // ???? ???? ????
        GameObject cookedItem =
            Instantiate(cookedPattyPrefab);

        // ???? ???? ?????
        bool success =
            playerPickup.TryPickup(cookedItem);

        if (success)
        {
            // Station ???? ??????
            cookedReady = false;

            // ???? ???? ?????
            timerObject.SetActive(false);
            animationCookedPatty.SetActive(false);
            // ?????????? ??? ???? ???? ??? ????
            timerFill.color = cookingColor;

            // ?????????? ????? ?????
            timerFill.fillAmount = 0f;

            Debug.Log("Cooked Patty Picked Up!");
        }
        else
        {
            Destroy(cookedItem);

            Debug.Log("Could not pickup Cooked Patty!");
        }
    }
}