using System.Collections;
using UnityEngine;

public abstract class RangedWeaponModule : WeaponModule
{
    [SerializeField] protected Transform firePoint;

    [Header("Ranged Specs")]
    [SerializeField] protected RangedWeaponStat rangedStat;

    [Header("Fire Strategy")]
    [SerializeField] protected FireBehavior fireBehavior;

    [Header("Projectile In Use")]
    [SerializeField] protected GameObject projectile;

    bool isReady = false;
    Coroutine readyCoroutine;
    Coroutine firingCoroutine;  // 점사/패턴 코루틴

    private void OnEnable()
    {
        // 시작시 선딜레이 적용
        if (attackable)
            StartPreDelay();
        else
            isReady = false;
    }
    protected abstract void SpawnBullet(float extraAngleDeg);
    public override void Attack()
    {
        if (!attackable) return;
        if (!isReady) return;
        if (fireBehavior == null) return;
        if (firingCoroutine != null) return;

        firingCoroutine = StartCoroutine(FireRoutine());
    }
    IEnumerator FireRoutine()
    {
        yield return fireBehavior.Fire((extraAngleDeg) => SpawnBullet(extraAngleDeg));

        // 패턴이 끝난 뒤에 interval 재장전
        isReady = false;
        RestartTimer(rangedStat.interval);
        firingCoroutine = null;
    }


    private IEnumerator WaitforInterval(float time)
    {
        yield return CoroutineCache.WaitforSeconds(time);
        isReady = true;
        readyCoroutine = null;
    }
    private void RestartTimer(float t)
    {
        if (readyCoroutine != null) { StopCoroutine(readyCoroutine); readyCoroutine = null; }

        readyCoroutine = StartCoroutine(WaitforInterval(t));
    }
    public override void OnSelected()
    {
        base.OnSelected();
        if (readyCoroutine != null) { StopCoroutine(readyCoroutine); readyCoroutine = null; }
        if (firingCoroutine != null) { StopCoroutine(firingCoroutine); firingCoroutine = null; }
        isReady = false;
    }
    public override void OnDeselected()
    {
        if (readyCoroutine != null) { StopCoroutine(readyCoroutine); readyCoroutine = null; }
        if (firingCoroutine != null) { StopCoroutine(firingCoroutine); firingCoroutine = null; }
        isReady = false;

        base.OnDeselected();

        if (attackable) StartPreDelay();
    }
    private void StartPreDelay()
    {
        isReady = false;
        RestartTimer(rangedStat.preDelay);
    }
    private void OnDisable()
    {
        if (readyCoroutine != null) { StopCoroutine(readyCoroutine); readyCoroutine = null; }
        if (firingCoroutine != null) { StopCoroutine(firingCoroutine); firingCoroutine = null; }
        isReady = false;
    }
}
