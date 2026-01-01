using UnityEngine;

public interface IDamageable
{
    void ApplyDamage(DamageData data);
}

public struct DamageData
{
    public int Amount;
    public DamageType Type;
    //public float CriticalMultiplier;
}
