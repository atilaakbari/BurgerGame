using UnityEngine;
using System.Collections;

public class CookingSlot : MonoBehaviour
{
    [Header("Visuals")]
    [SerializeField] private GameObject rawPatty;
    [SerializeField] private GameObject cookedPatty;

    [Header("Animation")]
    [SerializeField] private Animator panAnimator;
    [SerializeField] private string cookTrigger = "Cook"; // ??? ????? ???????

    [Header("Timing")]
    [SerializeField] private float cookTime = 3f;

    private bool isCooking = false;
    private bool isReady = false;

    public bool IsEmpty => !isCooking && !isReady;
    public bool IsReady => isReady;

    private void Start()
    {
        ResetSlot();
    }

    public bool TryStartCooking()
    {
        if (!IsEmpty) return false;

        isCooking = true;
        isReady = false;

        if (rawPatty != null) rawPatty.SetActive(true);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        // ??????? ????????
        if (panAnimator != null)
            panAnimator.SetTrigger(cookTrigger);

        StartCoroutine(CookRoutine());
        return true;
    }

    private IEnumerator CookRoutine()
    {
        yield return new WaitForSeconds(cookTime);

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(true);

        isCooking = false;
        isReady = true;
    }

    public GameObject TakeCookedPatty()
    {
        if (!isReady) return null;

        // ????? ??????? ??? ???? ?? ?? ???? ???
        // ????? ??? ???? ???????
        ResetSlot();
        return null; // ????? ??? ????? ?? ????????????
    }

    public void ResetSlot()
    {
        isCooking = false;
        isReady = false;

        if (rawPatty != null) rawPatty.SetActive(false);
        if (cookedPatty != null) cookedPatty.SetActive(false);

        if (panAnimator != null)
            panAnimator.ResetTrigger(cookTrigger);
    }
}