using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FiringBehaviors/Projectile")]
public class ProjectileFiring : RangedBehavior
{
    public override void Fire(FireContext ctx)
    {
        var obj = ObjectPoolManager.Instance.GetObject(ctx.ProjectilePrefab, false);

        float rnd = (ctx.Stat.accuracy <= 0f) ? 0f : Random.Range(-ctx.Stat.accuracy * 0.5f, ctx.Stat.accuracy * 0.5f);
        var rot = ctx.FirePoint.rotation * Quaternion.Euler(0, 0, ctx.ExtraAngleDeg + rnd);

        obj.transform.SetPositionAndRotation(ctx.FirePoint.position, rot);

        if(obj.TryGetComponent<Bullet>(out var bullet))
        {
            bullet.SetBulletInfo(ctx.FinalDamage, ctx.Stat.speed, ctx.ShooterGuid);
            obj.SetActive(true);
        }
        else
        {
            Debug.LogError("projectile need Bullet");
            ObjectPoolManager.Instance.ReturnObject(obj);
        }
    }
}