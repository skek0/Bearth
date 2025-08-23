using UnityEngine;

public class BulletPool : MonoBehaviour
{
    [SerializeField] private GameObject bullet;

    private void Start()
    {
        ObjectPoolManager.Instance.CreatePool(bullet, 5);
    }
}