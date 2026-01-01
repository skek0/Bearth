using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FiringBehaviors/Projectile")]
public class ProjectileFiring : RangedBehavior
{
    public override void Fire(Transform firePoint, RangedWeaponStat stat, int finalDamage, float extraAngleDeg, GameObject projectile)
    {
        var obj = ObjectPoolManager.Instance.GetObject(projectile, false);

        float rnd = (stat.accuracy <= 0f) ? 0f : Random.Range(-stat.accuracy * 0.5f, stat.accuracy * 0.5f);
        var rot = firePoint.rotation * Quaternion.Euler(0, 0, extraAngleDeg + rnd);

        obj.transform.SetPositionAndRotation(firePoint.position, rot);

        if(obj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetBulletInfo(finalDamage, stat.speed);
            obj.SetActive(true);
        }
        else
        {
            Debug.LogError("projectile need Bullet");
            ObjectPoolManager.Instance.ReturnObject(obj);
        }
    }
}