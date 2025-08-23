using UnityEngine;

public class MeleeWeaponModule : WeaponModule
{
    [SerializeField] MeleeWeaponStat spec;
    float cooldownLeft;

    void Update()
    {
        if (cooldownLeft > 0f) cooldownLeft -= Time.deltaTime;
    }

    public override void Attack()
    {
        if (cooldownLeft > 0f) return;

        // 히트박스/레이캐스트 등으로 타격 판정
        // IDamageable.TakeDamage(DamageData) 호출

        cooldownLeft = spec.cooldown;
    }
}
