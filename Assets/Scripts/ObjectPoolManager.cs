using System.Collections.Generic;
using UnityEngine;

public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager Instance { get; private set; }
    private Dictionary<string, ObjectPool> pools = new Dictionary<string, ObjectPool>();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    public void CreatePool(string key, GameObject prefab, int initialSize)
    {
        if (!pools.ContainsKey(key))
        {
            GameObject poolObj = new GameObject($"Pool_{key}"); // 개별 풀을 위한 오브젝트 생성
            poolObj.transform.parent = transform;

            ObjectPool pool = poolObj.AddComponent<ObjectPool>(); // MonoBehaviour 추가
            pool.Initialize(prefab, initialSize);

            pools[key] = pool;
        }
        else
        {
            Debug.LogWarning($"Pool with key {key} already exists!");
        }
    }

    public GameObject GetObject(string key)
    {
        if (pools.ContainsKey(key))
        {
            return pools[key].GetObject();
        }

        Debug.LogWarning($"Pool with key {key} does not exist!");
        return null;
    }

    public void ReturnObject(string key, GameObject obj)
    {
        if (pools.ContainsKey(key))
        {
            pools[key].ReturnObject(obj);
        }
        else
        {
            Debug.LogWarning($"Pool with key {key} does not exist!");
        }
    }
}
