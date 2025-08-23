using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }

    // ★ 프리팹(GameObject) 자체를 키로 사용
    private readonly Dictionary<GameObject, ObjectPool> pools = new();

    public int defaultSize = 10;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }
        DontDestroyOnLoad(gameObject);
    }

    public void CreatePool(GameObject prefab, int initialSize)
    {
        if (!prefab)
        {
            Debug.LogWarning("CreatePool: prefab is null");
            return;
        }
        if (pools.ContainsKey(prefab)) return;

        GameObject poolObj = new GameObject($"Pool_{prefab.name}");
        poolObj.transform.parent = transform;

        var pool = poolObj.AddComponent<ObjectPool>();
        pool.Initialize(prefab, initialSize);

        pools[prefab] = pool;
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (!prefab)
        {
            Debug.LogWarning("GetObject: prefab is null");
            return null;
        }
        if (!pools.TryGetValue(prefab, out var pool))
        {
            CreatePool(prefab, defaultSize);
            pool = pools[prefab];
        }
        return pool.GetObject();
    }
    public void ReturnObject(GameObject obj)
    {
        if (!obj) return;
        var pool = obj.transform.parent ? obj.transform.parent.GetComponent<ObjectPool>() : null;
        if (pool != null) pool.ReturnObject(obj);
        else Debug.LogWarning($"No pool for {obj}!");
    }

}
