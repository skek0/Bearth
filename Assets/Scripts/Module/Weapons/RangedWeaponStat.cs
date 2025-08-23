using UnityEngine;

// 원거리
[CreateAssetMenu(menuName = "Weapon/RangedStat")]
public class RangedWeaponStat : WeaponStat
{
    // displayName, damage, damageType
    public float speed;         // 비행 속도
    public float interval;      // 발사 간격(쿨다운)
    public float preDelay;      // 연결 직후 발사 가능까지 시간
    public float accuracy;      // 탄퍼짐(도 단위)
}
