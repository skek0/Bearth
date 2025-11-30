using UnityEngine;

// 원거리
[CreateAssetMenu(menuName = "Weapon/RangedStat")]
public class RangedWeaponStat : WeaponStat
{
    // displayName, damage, damageType
    [Tooltip("Projectile's speed")]
    public float speed;         // 비행 속도
    [Tooltip("Time between each fire\nWorks as channeling time for laser")]
    public float interval;      // 발사 간격(쿨다운)
    [Tooltip("Time before first shot after attach\nWorks as charge time for laser")]
    public float preDelay;      // 연결 직후 발사 가능까지 시간
    [Tooltip("Random degrees from targeting direction.\n0 is 100% accuracy")]
    public float accuracy;      // 탄퍼짐(도 단위)

    public int penetration;     // 관통
    [Tooltip("Prefab to Fire - Projectile, Laser...")]
    [SerializeField] GameObject firingPrefab;   // 발사할 물체 - 투사체, 레이저 등


}
