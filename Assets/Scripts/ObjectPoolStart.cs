using UnityEngine;

public class ObjectPoolStart : MonoBehaviour
{
    public GameObject bulletPrefab;

    private void Start()
    {
        ObjectPoolManager.Instance.CreatePool("Bullet", bulletPrefab, 100);
    }
}
