using UnityEngine;

public class Item : MonoBehaviour
{
    [Header("Item Type")]
    [SerializeField] private ItemType itemType;


    [Header("Assembly")]
    public float AssemblyOffsetY = 0f;

    public bool CanAssemble = true;


    public ItemType Type => itemType;
}