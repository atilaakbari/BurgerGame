// فایل جدید: GameFoodEvents.cs
using System;
using UnityEngine;

public static class GameFoodEvents
{
    public static event Action<ItemType> OnItemTrashed;

    public static void RaiseItemTrashed(ItemType type)
    {
        OnItemTrashed?.Invoke(type);
    }
}