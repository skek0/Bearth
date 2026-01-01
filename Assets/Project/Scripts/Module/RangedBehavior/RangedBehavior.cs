using UnityEngine;

public abstract class RangedBehavior : ScriptableObject
{
    public abstract void Fire(
        Transform firePoint,
        RangedWeaponStat stat,
        int finalDamage,
        float extraAngleDeg,
        GameObject projectile
        );
}
