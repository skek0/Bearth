using UnityEngine;

// 근접
[CreateAssetMenu(menuName = "Weapon/MeleeStat")]
public class MeleeWeaponStat : WeaponStat
{
    // displayName, damage, damageType
    public float attackRange;
    public float knockback;
    public float cooldown;   // 연타 간격
}
