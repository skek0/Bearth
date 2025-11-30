using UnityEngine;

public class GunModule : RangedWeaponModule
{

    protected override void SpawnBullet(float extraAngleDeg)
    {
        GameObject _bullet = ObjectPoolManager.Instance.GetObject(projectile, false);

        // 정확도(랜덤) + 패턴 각도 보정
        float rnd = (rangedStat.accuracy <= 0f) ? 0f : Random.Range(-rangedStat.accuracy * 0.5f, rangedStat.accuracy * 0.5f);
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, extraAngleDeg + rnd);

        _bullet.transform.SetPositionAndRotation(firePoint.position, rot);

        if (_bullet.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetBulletInfo(Damage, rangedStat.speed);
            _bullet.SetActive(true);
        }
    }

}
