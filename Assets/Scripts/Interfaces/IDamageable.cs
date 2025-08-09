using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageData data);
}

public struct DamageData
{
    public int Amount;
    public DamageType Type;
    //public float CriticalMultiplier;
}

public enum DamageType
{
    Physics,
    Energy,
}