using UnityEngine;

public interface IDamageable
{
    void TakeDamage(DamageData data);
}

public struct DamageData
{
    public int Amount;
    //public float CriticalMultiplier;
}
