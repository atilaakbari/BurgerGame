using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BurgerOrder", menuName = "Burger/Create Order")]
public class BurgerOrder : ScriptableObject
{
    [Header("Burger")]
    public List<ItemType> items = new List<ItemType>();

    [Header("Order Price")]
    public int price = 10;

    [Header("Customer Payment")]
    public int eatingMoney = 5;

    [Header("Eating")]
    public float eatingTime = 10f;
}