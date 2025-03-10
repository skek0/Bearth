using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private readonly object _lock = new object(); // 동기화를 위한 락 객체

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
        obj.SetActive(false); // 처음에는 비활성화된 상태로 추가
        return obj;
    }

    public GameObject GetObject()
    {
        lock (_lock) // 동시 접근 방지
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            // 부족하면 새로 생성
            GameObject newObj = CreateNewObject();
            newObj.SetActive(true);
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        lock (_lock) // 동시 접근 방지
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
