using System;
using System.Collections;
using UnityEditor;
using UnityEngine;

public class Weapon : MonoBehaviour
{
    [Header("Weapon Stats")]
    [SerializeField] int damage;
    [Tooltip("Time between attacks")]
    [SerializeField] float coolTime;
    [Tooltip("Flying speed of Projectile")]
    [SerializeField] float speed;

    bool OnCooldown = false;
    Transform offsetTransform;

    //[Header("Not SerializeField")]

    private void Awake()
    {
        offsetTransform = transform.Find("Offset");
        if (offsetTransform == null) Debug.Log($"{gameObject.name} has no offset!");
    }

    public virtual void Attack()
    {
        if (!OnCooldown)
        {
            OnCooldown = true;
            GameObject projectile = ObjectPoolManager.Instance.GetObject("Bullet");
            if (projectile != null && projectile.TryGetComponent(out Bullet bullet))
            {
                bullet.transform.SetPositionAndRotation(
                    offsetTransform.position,
                    offsetTransform.rotation * Quaternion.Euler(0, 0, 90)
                    );
                bullet.damage = damage;
                bullet.speed = speed;
            }
            StartCoroutine(StartCoolDown());
        }
    }

    IEnumerator StartCoolDown()
    {
        yield return new WaitForSeconds(coolTime);
        OnCooldown = false;
    }
}