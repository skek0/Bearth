using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectPool : MonoBehaviour
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;

    public void Initialize(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        for (int i = 0; i < initialSize; i++)
        {
            GameObject obj = CreateNewObject();
            pool.Enqueue(obj);
        }
    }

    private GameObject CreateNewObject()
    {
        GameObject obj = Instantiate(prefab, transform); // 풀 매니저 하위로 생성
        obj.SetActive(false);
        return obj;
    }

    public GameObject GetObject()
    {
        if (pool.Count > 0)
        {
            GameObject obj = pool.Dequeue();
            obj.SetActive(true);
            return obj;
        }

        return CreateNewObject(); // 동적 확장
    }

    public void ReturnObject(GameObject obj)
    {
        obj.SetActive(false);
        pool.Enqueue(obj);
    }
}