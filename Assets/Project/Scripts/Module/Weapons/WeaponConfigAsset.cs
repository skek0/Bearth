using UnityEngine;

public enum CombatKind { Melee, Ranged }

[System.Serializable] public abstract class CombatSettingsBase
{
    [Min(0f)] public int damage;
    [Min(0f)] public float range = 10f;
    [Min(0f)] public float preDelay = 0f;
    [Min(0f)] public float cooldown = 1f;
}

[System.Serializable] public abstract class MeleeSettings  : CombatSettingsBase { }
[System.Serializable] public abstract class RangedSettings : CombatSettingsBase { [Min(0f)] public float accuracy = 0f; }

// 예시 구현들 (필요한 만큼 더 추가 가능)
[System.Serializable] public class SwingWeapon      : MeleeSettings  { public GameObject swingPrefab; [Range(0,360)] public float swingArc = 90f; }
[System.Serializable] public class ProjectileWeapon : RangedSettings { public float projectileSpeed = 30f; public GameObject projectilePrefab; }
[System.Serializable] public class LaserWeapon      : RangedSettings { public GameObject laserPrefab; }

// 폴리모픽 컨테이너
[System.Serializable]
public class WeaponCombatSettings
{
    public CombatKind kind = CombatKind.Melee;
    [SerializeReference] public CombatSettingsBase stats;
}

// ScriptableObject 컨테이너 (인스펙터에서 이 에셋을 편집)
[CreateAssetMenu(menuName = "Game/Weapon Config")]
public class WeaponConfigAsset : ScriptableObject
{
    public WeaponCombatSettings stats;
}
