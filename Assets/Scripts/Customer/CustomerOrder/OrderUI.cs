using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OrderUI : MonoBehaviour
{
    [Header("Order")]
    [SerializeField] private Transform itemsParent;

    [Header("Item Icons")]
    [SerializeField] private Image itemIconPrefab;

    [Header("Item Sprites")]
    [SerializeField] private List<ItemIconData> itemIcons;

    [Header("Background")]
    [SerializeField] private RectTransform background;
    [SerializeField] private float backgroundPadding = 10f;

    private float originalBackgroundWidth;

    private void Awake()
    {
        if (background != null)
        {
            originalBackgroundWidth =
                background.sizeDelta.x;
        }
    }
    public void ShowOrder(BurgerOrder order)
    {
        if (order == null)
            return;

        // ????? ?? ????? ?????? UI ???? ???
        gameObject.SetActive(true);

        ClearUI();


        // =====================================================
        // ORDER
        // =====================================================

        List<ItemType> displayOrder =
            new List<ItemType>(order.items);


        // =====================================================
        // UI ?? ????? ?? ???? ????? ??????
        //
        // ????? ???? ????? = ?????
        // ????? ???? ????? = ????
        //
        // ????????:
        //
        // BunBottem
        // Patty
        // Cheese
        // BunTop
        //
        // ??? UI:
        //
        // BunTop      ? ????
        // Cheese
        // Patty
        // BunBottem   ? ?????
        // =====================================================


        for (int i = 0; i < displayOrder.Count; i++)
        {
            ItemType itemType =
                displayOrder[i];


            Sprite sprite =
                GetSprite(itemType);


            if (sprite == null)
            {
                Debug.LogWarning(
                    "No sprite found for: " +
                    itemType
                );

                continue;
            }


            Image icon =
                Instantiate(
                    itemIconPrefab,
                    itemsParent
                );

            RectTransform iconRect = icon.GetComponent<RectTransform>();

            // ?????? ???
            iconRect.sizeDelta = new Vector2(
                0.5f,
                0.5f
            );

            iconRect.localScale = Vector3.one;

            // ????? ??? ??????
            iconRect.localPosition = new Vector3(
                0f,
                i * 0.2f,
                0f
            );


            icon.sprite = sprite;

            icon.gameObject.SetActive(true);
        }

        UpdateBackgroundSize();

    }


    // =====================================================
    // CLEAR UI
    // =====================================================

    public void ClearUI()
    {
        if (itemsParent == null)
            return;


        for (
            int i = itemsParent.childCount - 1;
            i >= 0;
            i--
        )
        {
            Destroy(
                itemsParent.GetChild(i).gameObject
            );
        }
    }


    // =====================================================
    // GET SPRITE
    // =====================================================

    private Sprite GetSprite(ItemType type)
    {
        foreach (
            ItemIconData data
            in itemIcons
        )
        {
            if (data.type == type)
                return data.sprite;
        }


        return null;
    }

    private void UpdateBackgroundSize()
    {
        if (background == null)
            return;

        if (itemsParent == null)
            return;

        if (itemsParent.childCount == 0)
            return;


        RectTransform firstIcon =
            itemsParent.GetChild(0)
            .GetComponent<RectTransform>();


        RectTransform lastIcon =
            itemsParent.GetChild(
                itemsParent.childCount - 1
            )
            .GetComponent<RectTransform>();


        if (firstIcon == null ||
            lastIcon == null)
            return;


        float top =
            firstIcon.localPosition.y +
            firstIcon.rect.height / 2f;


        float bottom =
            lastIcon.localPosition.y -
            lastIcon.rect.height / 2f;


        float newSize =
            Mathf.Abs(top - bottom) +
            (backgroundPadding * 2f);


        // ?????? ????
        float oldSize =
            background.sizeDelta.x;


        // ?????? ??????
        float difference =
            newSize - oldSize;


        // ??? Rotation Z = 90 ????
        // ???? X ?? ??? ????? ?????? Background ???
        background.sizeDelta =
            new Vector2(
                newSize,
                background.sizeDelta.y
            );


        // ??? ?? ?????? ??? ?????? ??????? ??????
        // ?? ????? ???? ????? ? ??? ?? ??? ???? ????
        background.anchoredPosition +=
            new Vector2(
                0f,
                difference / 2f
            );
    }
}



[System.Serializable]
public class ItemIconData
{
    public ItemType type;
    public Sprite sprite;
}