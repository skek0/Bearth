using UnityEngine;

public enum DamageType 
{ 
    PHYSICS, 
    ENERGY 
}

public abstract class WeaponStat : ScriptableObject
{
    [Tooltip("is Dps for laser")]
    public int damage;
    public DamageType damageType;
}
