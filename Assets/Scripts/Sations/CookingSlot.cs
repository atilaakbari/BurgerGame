using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookingSlot : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject rawPatty;
    [SerializeField] private GameObject cookedPatty;

    [Header("Timer UI")]
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Image timerFill;
    [SerializeField] private GameObject timerTick;
    [SerializeField] private float tickDelay = 0.3f;
    [SerializeField] private Color cookingColor = Color.red;
    [SerializeField] private Color readyColor = Color.green;

    [Header("Animation")]
    [SerializeField] private Animator panAnimator;
    [SerializeField] private string cookingBool = "IsCooking";

    [Header("Timing")]
    [SerializeField] private float cookingTime = 5f;

    private bool isCooking;
    private bool isReady;
    private float currentProgress; // 0..1
    private Coroutine cookRoutine;

    public bool IsEmpty => !isCooking && !isReady;
    public bool IsReady => isReady;
    public bool IsCooking => isCooking;
    public float Progress => currentProgress;

    private void Start()
    {
        ResetSlot();
    }

    public bool TryStartCooking()
    {
        if (!IsEmpty)
            return false;

        isCooking = true;
        isReady = false;
        currentProgress = 0f;

        if (rawPatty != null) rawPatty.SetActive(true);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        if (timerObject != null) timerObject.SetActive(true);
        if (timerTick != null) timerTick.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
            timerFill.color = cookingColor;
        }

        if (panAnimator != null)
            panAnimator.SetBool(cookingBool, true);

        cookRoutine = StartCoroutine(CookRoutine(0f));
        return true;
    }

    // ادامه پخت از یک progress مشخص (برای انتقال بین ارتقا)
    public bool ResumeCooking(float progress)
    {
        if (!IsEmpty)
            return false;

        progress = Mathf.Clamp01(progress);

        isCooking = true;
        isReady = false;
        currentProgress = progress;

        if (rawPatty != null) rawPatty.SetActive(true);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        if (timerObject != null) timerObject.SetActive(true);
        if (timerTick != null) timerTick.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = progress;
            timerFill.color = Color.Lerp(cookingColor, readyColor, progress);
        }

        if (panAnimator != null)
            panAnimator.SetBool(cookingBool, true);

        cookRoutine = StartCoroutine(CookRoutine(progress));
        return true;
    }

    public void SetReady()
    {
        StopCookRoutine();

        isCooking = false;
        isReady = true;
        currentProgress = 1f;

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(true);

        if (timerObject != null) timerObject.SetActive(true);
        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = readyColor;
        }

        if (timerTick != null) timerTick.SetActive(true);

        if (panAnimator != null)
            panAnimator.SetBool(cookingBool, false);
    }

    private IEnumerator CookRoutine(float startProgress)
    {
        float timer = startProgress * cookingTime;

        while (timer < cookingTime)
        {
            timer += Time.deltaTime;
            currentProgress = timer / cookingTime;

            if (timerFill != null)
            {
                timerFill.fillAmount = currentProgress;
                timerFill.color = Color.Lerp(cookingColor, readyColor, currentProgress);
            }

            yield return null;
        }

        isCooking = false;
        isReady = true;
        currentProgress = 1f;

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(true);

        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = readyColor;
        }

        if (panAnimator != null)
            panAnimator.SetBool(cookingBool, false);

        yield return new WaitForSeconds(tickDelay);

        if (timerTick != null)
            timerTick.SetActive(true);
    }

    public void Collect()
    {
        ResetSlot();
    }

    public void ResetSlot()
    {
        StopCookRoutine();

        isCooking = false;
        isReady = false;
        currentProgress = 0f;

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        if (timerObject != null) timerObject.SetActive(false);
        if (timerTick != null) timerTick.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
            timerFill.color = cookingColor;
        }

        if (panAnimator != null)
            panAnimator.SetBool(cookingBool, false);
    }

    private void StopCookRoutine()
    {
        if (cookRoutine != null)
        {
            StopCoroutine(cookRoutine);
            cookRoutine = null;
        }
    }

    // برای انتقال وضعیت
    public enum SlotStateType { Empty, Cooking, Ready }

    public struct SlotState
    {
        public SlotStateType type;
        public float progress;
    }

    public SlotState CaptureState()
    {
        if (isReady)
            return new SlotState { type = SlotStateType.Ready, progress = 1f };

        if (isCooking)
            return new SlotState { type = SlotStateType.Cooking, progress = currentProgress };

        return new SlotState { type = SlotStateType.Empty, progress = 0f };
    }

    public void ApplyState(SlotState state)
    {
        ResetSlot();

        if (state.type == SlotStateType.Ready)
            SetReady();
        else if (state.type == SlotStateType.Cooking)
            ResumeCooking(state.progress);
    }
}