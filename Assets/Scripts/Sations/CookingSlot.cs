using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CookingSlot : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject rawPatty;
    [SerializeField] private GameObject cookedPatty;

    [Header("Animation")]
    [SerializeField] private Animator panAnimator;
    [SerializeField] private string cookTrigger = "Cook"; // ??????? ???/??? ??? ???? ?????????

    [Header("Timing")]
    [SerializeField] private float cookTime = 3f;

    [Header("Timer UI (??? ???? ?? - ?????? ?????)")]
    [SerializeField] private GameObject timerObject;
    [SerializeField] private Image timerFill;
    [SerializeField] private GameObject timerTick;
    [SerializeField] private float tickDelay = 0.5f;
    [SerializeField] private Color cookingColor = Color.red;
    [SerializeField] private Color readyColor = Color.green;

    private bool isCooking = false;
    private bool isReady = false;
    private Coroutine cookRoutine;

    public bool IsEmpty => !isCooking && !isReady;
    public bool IsReady => isReady;

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

        if (rawPatty != null) rawPatty.SetActive(true);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        if (panAnimator != null)
            panAnimator.SetTrigger(cookTrigger);

        if (timerObject != null)
            timerObject.SetActive(true);

        if (timerTick != null)
            timerTick.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
            timerFill.color = cookingColor;
        }

        cookRoutine = StartCoroutine(CookRoutine());
        return true;
    }

    private IEnumerator CookRoutine()
    {
        float timer = 0f;

        while (timer < cookTime)
        {
            timer += Time.deltaTime;

            float progress = Mathf.Clamp01(timer / cookTime);

            if (timerFill != null)
            {
                timerFill.fillAmount = progress;
                timerFill.color = Color.Lerp(cookingColor, readyColor, progress);
            }

            yield return null;
        }

        if (timerFill != null)
        {
            timerFill.fillAmount = 1f;
            timerFill.color = readyColor;
        }

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(true);

        isCooking = false;
        isReady = true;
        cookRoutine = null;

        if (tickDelay > 0f)
            yield return new WaitForSeconds(tickDelay);

        if (timerTick != null)
            timerTick.SetActive(true);
    }

    // ??? ?? ?????? CookingStation ???? ????? ?? ?? ?????? ???? ??? ?? ??? ??????
    public bool Collect()
    {
        if (!isReady)
            return false;

        ResetSlot();
        return true;
    }

    public void ResetSlot()
    {
        if (cookRoutine != null)
        {
            StopCoroutine(cookRoutine);
            cookRoutine = null;
        }

        isCooking = false;
        isReady = false;

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        if (panAnimator != null)
            panAnimator.ResetTrigger(cookTrigger);

        if (timerObject != null)
            timerObject.SetActive(false);

        if (timerTick != null)
            timerTick.SetActive(false);

        if (timerFill != null)
        {
            timerFill.fillAmount = 0f;
            timerFill.color = cookingColor;
        }
    }
}