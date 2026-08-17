using UnityEngine;

public class TableManager : MonoBehaviour
{
    public static TableManager Instance { get; private set; }

    private RestaurantTable[] tables;

    private void Awake()
    {
        Instance = this;

        tables = FindObjectsByType<RestaurantTable>(
            FindObjectsSortMode.None
        );
    }

    public RestaurantTable GetFreeTable()
    {
        foreach (RestaurantTable table in tables)
        {
            if (!table.IsOccupied)
            {
                return table;
            }
        }

        return null;
    }
}
