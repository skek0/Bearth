using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private readonly object _lock = new object(); // ����ȭ�� ���� �� ��ü

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
        GameObject obj = Instantiate(prefab, transform); // Ǯ �Ŵ��� ������ ����
        obj.SetActive(false); // ó������ ��Ȱ��ȭ�� ���·� �߰�
        return obj;
    }

    public GameObject GetObject()
    {
        lock (_lock) // ���� ���� ����
        {
            if (pool.Count > 0)
            {
                GameObject obj = pool.Dequeue();
                obj.SetActive(true);
                return obj;
            }

            // �����ϸ� ���� ����
            GameObject newObj = CreateNewObject();
            newObj.SetActive(true);
            return newObj;
        }
    }

    public void ReturnObject(GameObject obj)
    {
        lock (_lock) // ���� ���� ����
        {
            obj.SetActive(false);
            pool.Enqueue(obj);
        }
    }
}
