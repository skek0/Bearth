using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] int damage;
    [Tooltip("Time between attacks")]
    [SerializeField] float interval;
    [Tooltip("Flying speed of Projectile")]
    [SerializeField] float speed;

    float cooldown = 0f;

    [Header("Not SerializeField")]
    [SerializeField] Transform offsetTransform;

    private void Awake()
    {
        offsetTransform = transform.Find("Offset");
        if (offsetTransform == null) Debug.Log($"{gameObject.name} has no offset!");
    }

    private void Update()
    {
        if(cooldown > 0f)
        {
            cooldown -= Time.deltaTime;
        }
    }

    public virtual void Attack()
    {
        if (cooldown <= 0f)
        {
            cooldown = interval;
            GameObject projectile = ObjectPoolManager.Instance.GetObject("Bullet");
            if (projectile != null && projectile.TryGetComponent(out Bullet bullet))
            {
                bullet.transform.SetPositionAndRotation(
                    offsetTransform.position,
                    Quaternion.LookRotation(Vector3.forward, transform.right)
                    );
                bullet.damage = damage;
                bullet.speed = speed;
            }
        }
    }

}
