using System.Collections.Generic;
using UnityEngine;

public class Burger : MonoBehaviour
{
    public List<ItemType> items =
        new List<ItemType>();


    public void AddItem(ItemType type)
    {
        items.Add(type);

        Debug.Log(
            "Burger Added: " + type
        );
    }


    public List<ItemType> GetItems()
    {
        return items;
    }


    public void ResetBurger()
    {
        items.Clear();

        Debug.Log(
            "Burger Reset!"
        );
    }
}