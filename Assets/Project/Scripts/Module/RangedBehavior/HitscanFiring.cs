using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FiringBehaviors/Hitscan")]
public class HitscanFiring : RangedBehavior
{
    public override void Fire(Transform firePoint, RangedWeaponStat stat, int finalDamage, float extraAngleDeg, GameObject projectile)
    {
        var obj = ObjectPoolManager.Instance.GetObject(projectile, false);

        float rnd = (stat.accuracy <= 0f) ? 0f : Random.Range(-stat.accuracy * 0.5f, stat.accuracy * 0.5f);
        var rot = firePoint.rotation * Quaternion.Euler(0, 0, extraAngleDeg + rnd);

        obj.SetActive(true);

        obj.transform.SetPositionAndRotation(firePoint.position, rot);
        // 1) 레이저 프리팹인 경우(ILaserProjectile을 구현)
        if (obj.TryGetComponent<ILaserProjectile>(out var laser))
        {
            // Damage를 "접촉 시 피해량"으로 사용, duration은 rangedStat.interval
            laser.SetInfo(finalDamage, stat.interval, firePoint);
        }
        else
        {
            Debug.LogError("projectile need Bullet");
            ObjectPoolManager.Instance.ReturnObject(obj);
        }
    }
}
