using UnityEngine;

public enum DamageType 
{ 
    PHYSICS, 
    ENERGY 
}

public abstract class WeaponStat
{
    public int damage;
    public DamageType damageType;
}
