using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameButtonsLayoutManager : MonoBehaviour
{
    [Header("دکمه‌هایی که خودت دستی می‌دی")]
    [SerializeField] private RectTransform[] buttons;

    [Header("تنظیمات")]
    [SerializeField] private float spacing = 20f;
    [SerializeField] private bool useButtonWidth = true;
    [SerializeField] private float fixedButtonWidth = 100f;

    private int lastActiveCount = -1;
    private bool[] lastActiveStates;

    private void Awake()
    {
        if (buttons != null)
            lastActiveStates = new bool[buttons.Length];
    }

    // نکته‌ی مهم: این باید LateUpdate باشه نه Update.
    // اسکریپت‌های استیشن‌ها (CookingStationUpgrade, CookingStation, ...) دکمه‌ها رو
    // داخل Update() خودشون SetActive می‌کنن. اگه اینجا هم از Update استفاده کنیم،
    // بسته به ترتیب اجرای اسکریپت‌ها (که Unity تضمینش نمی‌ده) ممکنه این کد قبل از
    // اونا اجرا بشه و وضعیت یه فریم قدیمی رو ببینه - همون چیزی که باعث ناپدید شدن
    // لحظه‌ای دکمه‌ها می‌شد. با LateUpdate مطمئنیم همه‌ی Update() های اون فریم
    // (از جمله اونایی که دکمه‌ها رو فعال/غیرفعال می‌کنن) قبلش تموم شدن.
    private void LateUpdate()
    {
        if (buttons == null || buttons.Length == 0)
            return;

        bool changed = false;
        int activeCount = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            bool isActive = buttons[i] != null && buttons[i].gameObject.activeSelf;

            if (lastActiveStates == null || lastActiveStates.Length != buttons.Length)
            {
                lastActiveStates = new bool[buttons.Length];
                changed = true;
            }

            if (lastActiveStates[i] != isActive)
            {
                lastActiveStates[i] = isActive;
                changed = true;
            }

            if (isActive)
                activeCount++;
        }

        if (changed || activeCount != lastActiveCount)
        {
            lastActiveCount = activeCount;
            LayoutActiveButtons();
        }
    }

    public void Refresh()
    {
        LayoutActiveButtons();
    }

    private void LayoutActiveButtons()
    {
        List<RectTransform> active = new List<RectTransform>();

        foreach (RectTransform btn in buttons)
        {
            if (btn != null && btn.gameObject.activeSelf)
                active.Add(btn);
        }

        int count = active.Count;
        if (count == 0)
            return;

        float width = fixedButtonWidth;

        if (useButtonWidth && active[0] != null)
        {
            Canvas.ForceUpdateCanvases();
            width = active[0].rect.width;

            if (width < 1f)
                width = fixedButtonWidth;
        }

        float totalWidth = (count * width) + ((count - 1) * spacing);
        float startX = -totalWidth / 2f + (width / 2f);

        for (int i = 0; i < count; i++)
        {
            Vector2 pos = active[i].anchoredPosition;
            pos.x = startX + i * (width + spacing);
            active[i].anchoredPosition = pos;
        }
    }
}