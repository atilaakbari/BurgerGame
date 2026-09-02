using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CuttingSlot : MonoBehaviour
{
    [Header("Board Point")]
    [SerializeField] private Transform boardPoint;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Image timerFill;
    [SerializeField] private GameObject checkMark;
    [SerializeField] private Color cuttingColor = Color.red;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Animation")]
    [SerializeField] private Animator knifeAnimator;
    [SerializeField] private string cuttingBool = "IsCutting";

    private GameObject currentInputItem;
    private List<GameObject> readyOutputs = new List<GameObject>();
    private CuttingStation.CuttingRecipe currentRecipe;

    private bool isCutting;
    private bool isReady;
    private float currentProgress;
    private Coroutine cutRoutine;

    public bool IsEmpty => !isCutting && !isReady && readyOutputs.Count == 0;
    public bool IsReady => isReady && readyOutputs.Count > 0;
    public bool IsCutting => isCutting;
    public float Progress => currentProgress;
    public int ReadyCount => readyOutputs.Count;

    private void Start()
    {
        ResetSlot();
    }

    // اضافه شد: نوع آیتمِ خروجیِ آماده رو برمی‌گردونه (برای آیکون دکمه‌ی پیکاپ)
    public ItemType GetReadyItemType()
    {
        if (readyOutputs.Count == 0)
            return ItemType.None;

        GameObject item = readyOutputs[0];
        if (item == null)
            return ItemType.None;

        Item itemData = item.GetComponent<Item>();
        return itemData != null ? itemData.Type : ItemType.None;
    }

    public bool TryStartCutting(GameObject inputItem, CuttingStation.CuttingRecipe recipe)
    {
        if (!IsEmpty || inputItem == null || recipe == null) return false;

        currentRecipe = recipe;
        currentInputItem = inputItem;
        isCutting = true;
        isReady = false;
        currentProgress = 0f;

        PlaceInputOnBoard(inputItem, recipe);

        if (timerObject != null) timerObject.SetActive(true);
        if (checkMark != null) checkMark.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
            timerFill.color = cuttingColor;
        }

        if (knifeAnimator != null)
            knifeAnimator.SetBool(cuttingBool, true);

        cutRoutine = StartCoroutine(CutRoutine(0f));
        return true;
    }

    public bool ResumeCutting(CuttingStation.CuttingRecipe recipe, float progress, GameObject inputItem)
    {
        if (!IsEmpty || recipe == null) return false;

        progress = Mathf.Clamp01(progress);
        currentRecipe = recipe;
        currentInputItem = inputItem;
        isCutting = true;
        isReady = false;
        currentProgress = progress;

        if (inputItem != null)
            PlaceInputOnBoard(inputItem, recipe);

        if (timerObject != null) timerObject.SetActive(true);
        if (checkMark != null) checkMark.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = progress;
            timerFill.color = Color.Lerp(cuttingColor, readyColor, progress);
        }

        if (knifeAnimator != null)
            knifeAnimator.SetBool(cuttingBool, true);

        cutRoutine = StartCoroutine(CutRoutine(progress));
        return true;
    }

    private void PlaceInputOnBoard(GameObject inputItem, CuttingStation.CuttingRecipe recipe)
    {
        inputItem.transform.SetParent(boardPoint, false);
        inputItem.transform.localPosition = Vector3.zero;
        inputItem.transform.localRotation = Quaternion.identity;
        inputItem.transform.localScale = recipe.inputScaleOnBoard;

        Rigidbody rb = inputItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = true;
        Collider col = inputItem.GetComponent<Collider>();
        if (col != null) col.enabled = false;
    }

    public void SetReady(CuttingStation.CuttingRecipe recipe)
    {
        StopCutRoutine();
        currentRecipe = recipe;
        isCutting = false;
        isReady = true;
        currentProgress = 1f;

        if (currentInputItem != null) { Destroy(currentInputItem); currentInputItem = null; }

        SpawnOutputsOnBoard();

        if (timerObject != null) timerObject.SetActive(true);
        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = readyColor;
        }
        if (checkMark != null) checkMark.SetActive(true);

        if (knifeAnimator != null)
            knifeAnimator.SetBool(cuttingBool, false);
    }

    private IEnumerator CutRoutine(float startProgress)
    {
        float timer = startProgress * currentRecipe.cuttingTime;

        while (timer < currentRecipe.cuttingTime)
        {
            timer += Time.deltaTime;
            currentProgress = Mathf.Clamp01(timer / currentRecipe.cuttingTime);

            if (timerFill != null)
            {
                timerFill.fillAmount = currentProgress;
                timerFill.color = Color.Lerp(cuttingColor, readyColor, currentProgress);
            }

            yield return null;
        }

        isCutting = false;
        isReady = true;
        currentProgress = 1f;

        if (currentInputItem != null) { Destroy(currentInputItem); currentInputItem = null; }

        SpawnOutputsOnBoard();

        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = readyColor;
        }

        if (knifeAnimator != null)
            knifeAnimator.SetBool(cuttingBool, false);

        if (checkMark != null)
            checkMark.SetActive(true);
    }

    private void SpawnOutputsOnBoard()
    {
        ClearReadyOutputs();

        if (currentRecipe == null || currentRecipe.outputPrefab == null)
            return;

        for (int i = 0; i < currentRecipe.outputCount; i++)
        {
            GameObject output = Instantiate(currentRecipe.outputPrefab, boardPoint);
            output.transform.localRotation = Quaternion.identity;
            output.transform.localScale = currentRecipe.outputScaleOnBoard;

            if (currentRecipe.outputCount == 1)
                output.transform.localPosition = Vector3.zero;
            else
            {
                float spacing = 0.15f;
                float startX = -((currentRecipe.outputCount - 1) * spacing * 0.5f);
                output.transform.localPosition = new Vector3(startX + i * spacing, 0f, 0f);
            }

            Rigidbody rb = output.GetComponent<Rigidbody>();
            if (rb != null) rb.isKinematic = true;
            Collider col = output.GetComponent<Collider>();
            if (col != null) col.enabled = false;

            readyOutputs.Add(output);
        }
    }

    public bool TryTakeOneOutput(out GameObject outputItem)
    {
        outputItem = null;

        if (readyOutputs.Count <= 0) return false;

        outputItem = readyOutputs[0];
        readyOutputs.RemoveAt(0);

        outputItem.transform.SetParent(null);

        Rigidbody rb = outputItem.GetComponent<Rigidbody>();
        if (rb != null) rb.isKinematic = false;
        Collider col = outputItem.GetComponent<Collider>();
        if (col != null) col.enabled = true;

        if (readyOutputs.Count == 0)
            ResetSlot();

        return true;
    }

    public void CollectAll()
    {
        ResetSlot();
    }

    public void ResetSlot()
    {
        StopCutRoutine();

        isCutting = false;
        isReady = false;
        currentProgress = 0f;
        currentRecipe = null;

        if (currentInputItem != null) { Destroy(currentInputItem); currentInputItem = null; }

        ClearReadyOutputs();

        if (timerObject != null) timerObject.SetActive(false);
        if (checkMark != null) checkMark.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
            timerFill.color = cuttingColor;
        }

        if (knifeAnimator != null)
            knifeAnimator.SetBool(cuttingBool, false);
    }

    private void ClearReadyOutputs()
    {
        for (int i = 0; i < readyOutputs.Count; i++)
        {
            if (readyOutputs[i] != null)
                Destroy(readyOutputs[i]);
        }
        readyOutputs.Clear();
    }

    private void StopCutRoutine()
    {
        if (cutRoutine != null) { StopCoroutine(cutRoutine); cutRoutine = null; }
    }

    // ==================== انتقال وضعیت ====================
    public enum SlotStateType { Empty, Cutting, Ready }

    public struct SlotState
    {
        public SlotStateType type;
        public float progress;
        public ItemType inputType;
        public GameObject inputItem;
    }

    public SlotState CaptureState()
    {
        if (isReady)
            return new SlotState
            {
                type = SlotStateType.Ready,
                progress = 1f,
                inputType = currentRecipe != null ? currentRecipe.inputType : ItemType.None
            };

        if (isCutting)
        {
            SlotState state = new SlotState
            {
                type = SlotStateType.Cutting,
                progress = currentProgress,
                inputType = currentRecipe != null ? currentRecipe.inputType : ItemType.None,
                inputItem = currentInputItem
            };

            currentInputItem = null;

            return state;
        }

        return new SlotState { type = SlotStateType.Empty, progress = 0f, inputType = ItemType.None };
    }

    public void ApplyState(SlotState state, CuttingStation.CuttingRecipe recipe)
    {
        ResetSlot();
        if (recipe == null) return;

        if (state.type == SlotStateType.Ready)
            SetReady(recipe);
        else if (state.type == SlotStateType.Cutting)
            ResumeCutting(recipe, state.progress, state.inputItem);
    }
}