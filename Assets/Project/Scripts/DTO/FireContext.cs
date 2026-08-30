using UnityEngine;

public readonly struct FireContext
{
    public readonly Transform FirePoint;
    public readonly RangedWeaponStat Stat;
    public readonly int FinalDamage;
    public readonly float ExtraAngleDeg;
    public readonly GameObject ProjectilePrefab;
    public readonly Module Owner;

    public FireContext(
        Transform firePoint,
        RangedWeaponStat stat,
        int finalDamage,
        float extraAngleDeg,
        GameObject projectilePrefab,
        Module owner)
    {
        FirePoint = firePoint;
        Stat = stat;
        FinalDamage = finalDamage;
        ExtraAngleDeg = extraAngleDeg;
        ProjectilePrefab = projectilePrefab;
        Owner = owner;
    }
}