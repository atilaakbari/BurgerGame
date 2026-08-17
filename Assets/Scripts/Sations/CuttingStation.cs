using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CuttingStation : MonoBehaviour
{
    [System.Serializable]
    public class CuttingRecipe
    {
        [Header("Input")]
        public ItemType inputType;

        [Header("Output")]
        public GameObject outputPrefab;

        [Min(1)]
        public int outputCount = 1;

        [Header("Cutting Time")]
        public float cuttingTime = 3f;

        [Header("Input On Board")]
        public Vector3 inputScaleOnBoard = Vector3.one;

        [Header("Output On Board")]
        public Vector3 outputScaleOnBoard = Vector3.one;
    }


    [Header("Player")]
    [SerializeField]
    private PlayerPickup playerPickup;


    [Header("Cutting Point")]
    [SerializeField]
    private Transform cuttingPoint;


    [Header("Recipes")]
    [SerializeField]
    private CuttingRecipe[] recipes;


    [Header("Knife Animation")]
    [SerializeField]
    private Animator cuttingAnimator;


    [Header("Timer")]
    [SerializeField]
    private GameObject timerObject;

    [SerializeField]
    private Image timerFill;


    [Header("Ready Check")]
    [SerializeField]
    private GameObject checkMark;


    // ???? ???? ?? ??? ???? ?? ??? ??? ??? ???
    private GameObject currentCuttingItem;


    // ?????????? ?? ??? ???? ????? ??????? ?????
    private List<GameObject> readyOutputs =
        new List<GameObject>();


    private CuttingRecipe currentRecipe;


    private bool isCutting = false;


    // =========================================
    // START
    // =========================================

    private void Start()
    {
        if (timerObject != null)
        {
            timerObject.SetActive(false);
        }

        if (checkMark != null)
        {
            checkMark.SetActive(false);
        }
    }


    // =========================================
    // PLAYER ENTER
    // =========================================

    private void OnTriggerEnter(Collider other)
    {
        if (!other.CompareTag("Player"))
            return;


        // ??? ?? ??? ??? ???
        if (isCutting)
        {
            Debug.Log(
                "Cutting is still in progress!"
            );

            return;
        }


        // ??? ????? ????? ??? ???? ?????
        if (readyOutputs.Count > 0)
        {
            TakeCutOutput();

            return;
        }


        // ????? ???????? ???? ??? Player
        GameObject topItem =
            playerPickup.GetTopItem();


        if (topItem == null)
        {
            Debug.Log(
                "Player has no item!"
            );

            return;
        }


        // ????? Item Component
        Item itemData =
            topItem.GetComponent<Item>();


        if (itemData == null)
        {
            Debug.Log(
                "Top item has no Item component!"
            );

            return;
        }


        // ???? ???? Recipe
        CuttingRecipe recipe =
            FindRecipe(itemData.Type);


        if (recipe == null)
        {
            Debug.Log(
                "This item cannot be cut!"
            );

            return;
        }


        // ??????? ??? ???? ??????
        GameObject inputItem =
            playerPickup.RemoveTopItem();


        if (inputItem == null)
        {
            return;
        }


        // ????? Recipe
        currentRecipe =
            recipe;


        // ????? ???? ???
        currentCuttingItem =
            inputItem;


        // ???? ???? ???? ??? ????
        currentCuttingItem.transform.SetParent(
            cuttingPoint
        );


        currentCuttingItem.transform.localPosition =
            Vector3.zero;


        currentCuttingItem.transform.localRotation =
            Quaternion.identity;


        // ????? ???? ???? ???
        currentCuttingItem.transform.localScale =
            currentRecipe.inputScaleOnBoard;


        // ????? ???? ?????
        Rigidbody rb =
            currentCuttingItem.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.isKinematic = true;
        }


        // ????? ???? Collider
        Collider col =
            currentCuttingItem.GetComponent<Collider>();


        if (col != null)
        {
            col.enabled = false;
        }


        // ???? ???
        StartCoroutine(
            CutItem()
        );
    }


    // =========================================
    // FIND RECIPE
    // =========================================

    private CuttingRecipe FindRecipe(
        ItemType itemType
    )
    {
        foreach (
            CuttingRecipe recipe
            in recipes
        )
        {
            if (
                recipe.inputType
                ==
                itemType
            )
            {
                return recipe;
            }
        }


        return null;
    }


    // =========================================
    // CUT ITEM
    // =========================================

    private IEnumerator CutItem()
    {
        isCutting = true;


        Debug.Log(
            "Cutting Started!"
        );


        // ???? ??????? ????
        if (cuttingAnimator != null)
        {
            cuttingAnimator.SetBool(
                "IsCutting",
                true
            );
        }


        // ????? ?????
        if (timerObject != null)
        {
            timerObject.SetActive(true);
        }


        // Reset ?????
        if (timerFill != null)
        {
            timerFill.fillAmount =
                0f;

            timerFill.color =
                Color.red;
        }


        float timer = 0f;


        // ????? ?????
        while (
            timer
            <
            currentRecipe.cuttingTime
        )
        {
            timer +=
                Time.deltaTime;


            float progress =
                timer
                /
                currentRecipe.cuttingTime;


            progress =
                Mathf.Clamp01(
                    progress
                );


            // ?? ??? ?????
            if (timerFill != null)
            {
                timerFill.fillAmount =
                    progress;


                // ???? ?? ???
                timerFill.color =
                    Color.Lerp(
                        Color.red,
                        Color.green,
                        progress
                    );
            }


            yield return null;
        }


        // ???? ??? ?????
        if (timerFill != null)
        {
            timerFill.fillAmount =
                1f;

            timerFill.color =
                Color.green;
        }


        // ????? ???? ??????? ????
        if (cuttingAnimator != null)
        {
            cuttingAnimator.SetBool(
                "IsCutting",
                false
            );
        }


        // ??? ???? ???
        if (currentCuttingItem != null)
        {
            Destroy(
                currentCuttingItem
            );

            currentCuttingItem =
                null;
        }


        // ???? ???????? ??? ????
        SpawnOutputsOnBoard();


        // ????? ???
        if (checkMark != null)
        {
            checkMark.SetActive(true);
        }


        isCutting = false;


        Debug.Log(
            "Cutting Complete!"
        );
    }


    // =========================================
    // SPAWN OUTPUTS ON BOARD
    // =========================================

    private void SpawnOutputsOnBoard()
    {
        // ??????? ?? ???? ???? ????
        readyOutputs.Clear();


        for (
            int i = 0;
            i < currentRecipe.outputCount;
            i++
        )
        {
            // ???? ?????
            GameObject output =
                Instantiate(
                    currentRecipe.outputPrefab,
                    cuttingPoint
                );


            // ???? ???? ??? ????
            output.transform.localRotation =
                Quaternion.identity;


            // ????? Scale
            output.transform.localScale =
                currentRecipe.outputScaleOnBoard;


            // ???? ???? ????????
            // ??? ??? ?? ????? ???? ??? ????
            if (
                currentRecipe.outputCount
                ==
                1
            )
            {
                output.transform.localPosition =
                    Vector3.zero;
            }
            else
            {
                // ???? ???????? ???? ??
                float spacing =
                    0.15f;


                float startX =
                    -(
                        (
                            currentRecipe.outputCount
                            - 1
                        )
                        *
                        spacing
                        /
                        2f
                    );


                output.transform.localPosition =
                    new Vector3(
                        startX
                        +
                        (
                            i
                            *
                            spacing
                        ),
                        0f,
                        0f
                    );
            }


            // ????? ???? ?????
            Rigidbody rb =
                output.GetComponent<Rigidbody>();


            if (rb != null)
            {
                rb.isKinematic = true;
            }


            // ????? ???? Collider
            Collider col =
                output.GetComponent<Collider>();


            if (col != null)
            {
                col.enabled = false;
            }


            // ????? ???? ?? ???? ????????? ?????
            readyOutputs.Add(
                output
            );
        }


        Debug.Log(
            "Outputs On Board: "
            +
            readyOutputs.Count
        );
    }


    // =========================================
    // TAKE OUTPUT
    // =========================================

    private void TakeCutOutput()
    {
        if (
            readyOutputs.Count
            <=
            0
        )
        {
            return;
        }


        // ??? Inventory ?? ???
        if (
            !playerPickup.HasSpace
        )
        {
            Debug.Log(
                "Inventory is Full!"
            );

            return;
        }


        // ????? ????? ????? ?? ????
        GameObject outputItem =
            readyOutputs[0];


        // ??? ?? ????
        readyOutputs.RemoveAt(0);


        // ??? ???? ?? ????
        outputItem.transform.SetParent(
            null
        );


        // ???? ???? ?????
        Rigidbody rb =
            outputItem.GetComponent<Rigidbody>();


        if (rb != null)
        {
            rb.isKinematic = false;
        }


        // ???? ???? Collider
        Collider col =
            outputItem.GetComponent<Collider>();


        if (col != null)
        {
            col.enabled = true;
        }


        // ???? ?? ??? Player
        bool success =
            playerPickup.TryPickup(
                outputItem
            );


        if (!success)
        {
            // ??? ??????? ???? Inventory ???
            outputItem.transform.SetParent(
                cuttingPoint
            );


            readyOutputs.Insert(
                0,
                outputItem
            );


            return;
        }


        Debug.Log(
            "Output Picked Up!"
        );


        Debug.Log(
            "Remaining Outputs: "
            +
            readyOutputs.Count
        );


        // ??? ???? ????? ?????
        if (
            readyOutputs.Count
            >
            0
        )
        {
            return;
        }


        // ??? ???????? ??????? ????
        currentRecipe =
            null;


        // ????? ???? ???
        if (checkMark != null)
        {
            checkMark.SetActive(false);
        }


        // ????? ???? ?????
        if (timerObject != null)
        {
            timerObject.SetActive(false);
        }


        // Reset ?????
        if (timerFill != null)
        {
            timerFill.fillAmount =
                0f;

            timerFill.color =
                Color.red;
        }


        Debug.Log(
            "Cutting Station Ready!"
        );
    }
}