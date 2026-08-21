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
        if (tables == null)
            return null;

        foreach (RestaurantTable table in tables)
        {
            if (table != null && !table.IsOccupied)
                return table;
        }

        return null;
    }

    public RestaurantTable GetNearestOccupiedTable(Vector3 from)
    {
        if (tables == null)
            return null;

        RestaurantTable nearest = null;
        float best = float.MaxValue;

        foreach (RestaurantTable table in tables)
        {
            if (table == null || !table.IsOccupied)
                continue;

            float distance = Vector3.Distance(from, table.transform.position);

            if (distance < best)
            {
                best = distance;
                nearest = table;
            }
        }

        return nearest;
    }
}
