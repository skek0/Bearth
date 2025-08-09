using System.Collections;
using UnityEngine;

public class GunModule : WeaponModule
{
    [SerializeField] protected int damage;
    [SerializeField] protected float speed;
    [SerializeField] protected float interval;
    [SerializeField] Transform firePoint;
    public override void Attack()
    {
        if(attackable)
        {
            GameObject bullet = ObjectPoolManager.Instance.GetObject("Bullet");
            bullet.transform.SetPositionAndRotation(firePoint.position, transform.rotation);

            bullet.GetComponent<Bullet>().SetBulletInfo(damage, speed);
            bullet.SetActive(true);
            attackable = false;
            StartCoroutine(WaitforInterval());
        }
    }
    private IEnumerator WaitforInterval()
    {
        yield return CoroutineCache.WaitforSeconds(interval);
        attackable = true;
    }
}
