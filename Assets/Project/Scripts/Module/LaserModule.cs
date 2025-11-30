using UnityEngine;

public class LaserModule : RangedWeaponModule
{
    protected override void SpawnBullet(float extraAngleDeg)
    {
        GameObject _laser = ObjectPoolManager.Instance.GetObject(projectile, false);

        // 정확도(랜덤) + 패턴 각도 보정
        float rnd = (rangedStat.accuracy <= 0f) ? 0f : Random.Range(-rangedStat.accuracy * 0.5f, rangedStat.accuracy * 0.5f);
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, extraAngleDeg + rnd);

        _laser.SetActive(true);

        _laser.transform.SetPositionAndRotation(firePoint.position, rot);
        // 1) 레이저 프리팹인 경우(ILaserProjectile을 구현)
        if (_laser.TryGetComponent<ILaserProjectile>(out var laser))
        {
            // Damage를 "접촉 시 피해량"으로 사용, duration은 rangedStat.interval
            laser.SetInfo(Damage / 20, rangedStat.interval, firePoint);
        }
        else { Debug.Log("needs laserProjectile"); }
    }
}
