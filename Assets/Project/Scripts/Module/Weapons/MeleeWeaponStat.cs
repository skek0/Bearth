using UnityEngine;

// 근접
[CreateAssetMenu(menuName = "ModuleStat/Weapon/MeleeStat")]
public class MeleeWeaponStat : WeaponStat
{
    // displayName, damage, damageType
    public float length;
    public float knockbackForce;
    public float cooldown;   // 연타 간격
}
