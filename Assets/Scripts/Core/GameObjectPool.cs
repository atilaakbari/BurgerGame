using System.Collections.Generic;
using UnityEngine;

public class GameObjectPool
{
    private readonly GameObject prefab;
    private readonly Transform parent;
    private readonly Stack<GameObject> stack = new Stack<GameObject>(32);

    public GameObjectPool(GameObject prefab, Transform parent, int prewarm = 0)
    {
        this.prefab = prefab;
        this.parent = parent;

        for (int i = 0; i < prewarm; i++)
            Release(Create());
    }

    public GameObject Get(Vector3 position, Quaternion rotation)
    {
        GameObject instance = stack.Count > 0 ? stack.Pop() : Create();
        instance.transform.SetPositionAndRotation(position, rotation);
        instance.SetActive(true);
        return instance;
    }

    public void Release(GameObject instance)
    {
        if (instance == null)
            return;

        instance.SetActive(false);
        instance.transform.SetParent(parent, false);
        stack.Push(instance);
    }

    private GameObject Create()
    {
        GameObject instance = Object.Instantiate(prefab, parent);
        instance.SetActive(false);
        return instance;
    }
}
