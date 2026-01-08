using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FiringBehaviors/Hitscan")]
public class HitscanFiring : RangedBehavior
{
    public override void Fire(FireContext ctx)
    {
        var obj = ObjectPoolManager.Instance.GetObject(ctx.ProjectilePrefab, false);

        float rnd = (ctx.Stat.accuracy <= 0f) ? 0f : Random.Range(-ctx.Stat.accuracy * 0.5f, ctx.Stat.accuracy * 0.5f);
        var rot = ctx.FirePoint.rotation * Quaternion.Euler(0, 0, ctx.ExtraAngleDeg + rnd);

        obj.SetActive(true);

        obj.transform.SetPositionAndRotation(ctx.FirePoint.position, rot);
        // 1) 레이저 프리팹인 경우(ILaserProjectile을 구현)
        if (obj.TryGetComponent<ILaserProjectile>(out var laser))
        {
            // Damage를 "접촉 시 피해량"으로 사용, duration은 rangedStat.interval
            laser.SetInfo(ctx.FinalDamage, ctx.Stat.interval, ctx.FirePoint);
        }
        else
        {
            Debug.LogError("projectile need Bullet");
            ObjectPoolManager.Instance.ReturnObject(obj);
        }
    }
}
