using UnityEngine;

public enum DamageType { Physics, Energy }

public abstract class WeaponStat : ScriptableObject
{
    public string displayName;
    public int damage;
    public DamageType damageType;
}

