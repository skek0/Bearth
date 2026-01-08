using UnityEngine;

public readonly struct FireContext
{
    public readonly Transform FirePoint;
    public readonly RangedWeaponStat Stat;
    public readonly int FinalDamage;
    public readonly float ExtraAngleDeg;
    public readonly GameObject ProjectilePrefab;
    public readonly string ShooterGuid;

    public FireContext(
        Transform firePoint,
        RangedWeaponStat stat,
        int finalDamage,
        float extraAngleDeg,
        GameObject projectilePrefab,
        string shooterGuid)
    {
        FirePoint = firePoint;
        Stat = stat;
        FinalDamage = finalDamage;
        ExtraAngleDeg = extraAngleDeg;
        ProjectilePrefab = projectilePrefab;
        ShooterGuid = shooterGuid;
    }
}