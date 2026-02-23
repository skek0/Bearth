using UnityEngine;

public class RangedWeaponStat : WeaponStat
{
    // displayName, damage, damageType
    public float speed;         // 비행 속도
    public float interval;      // 발사 간격(쿨타임)
    public float preDelay;      // 연결 직후 발사 가능까지 시간
    public float accuracy;      // 탄퍼짐(도 단위)

    [SerializeField] GameObject firingPrefab;   // 발사할 물체 - 투사체, 레이저 등
}
