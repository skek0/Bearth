using UnityEngine;

public enum DamageType { Physics, Energy }

public abstract class WeaponStat : ScriptableObject
{
    public string displayName;
    [Tooltip("is Dps for laser")]
    public float damage;
    public DamageType damageType;
}

