using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "BurgerOrder", menuName = "Burger/Create Order")]
public class BurgerOrder : ScriptableObject
{
    public List<ItemType> items = new List<ItemType>();

    public int price = 10;
}