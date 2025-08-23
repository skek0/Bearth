using System.Collections;
using UnityEngine;

public class RangedWeaponModule : WeaponModule
{
    [SerializeField] protected Transform firePoint;

    [Header("Common Specs")]
    [SerializeField] protected RangedWeaponStat stat;

    [Header("Fire Strategy")]
    [SerializeField] protected FireBehavior fireBehavior;

    [Header("Projectile In Use")]
    [SerializeField] protected GameObject projectile;

    bool isReady = false;
    Coroutine readyCoroutine;
    Coroutine firingCoroutine;

    // (선택) 시작 시 장착 상태면 preDelay부터 걸고 싶다면 활성화
    private void OnEnable()
    {
        if (attackable)
            StartPreDelay();
        else
            isReady = false;
    }
    public override void Attack()
    {
        if (!attackable) return;
        if (!isReady) return;
        if (fireBehavior == null) return;
        if (firingCoroutine != null) return; // 점사/패턴 중 재진입 금지

        firingCoroutine = StartCoroutine(FireRoutine());
    }
    IEnumerator FireRoutine()
    {
        yield return fireBehavior.Fire((extraAngleDeg) => SpawnBullet(extraAngleDeg));

        // 패턴이 끝난 뒤에 interval 재장전
        isReady = false;
        RestartTimer(stat.interval);
        firingCoroutine = null;
    }
    void SpawnBullet(float extraAngleDeg)
    {
        GameObject bullet = ObjectPoolManager.Instance.GetObject(projectile);

        // 정확도(랜덤) + 패턴 각도 보정
        float rnd = (stat.accuracy <= 0f) ? 0f : Random.Range(-stat.accuracy * 0.5f, stat.accuracy * 0.5f);
        Quaternion rot = firePoint.rotation * Quaternion.Euler(0, 0, extraAngleDeg + rnd);

        bullet.transform.SetPositionAndRotation(firePoint.position, rot);
        bullet.GetComponent<Bullet>().SetBulletInfo(Damage, stat.speed);
        bullet.SetActive(true);
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
        RestartTimer(stat.preDelay);
    }
    private void OnDisable()
    {
        if (readyCoroutine != null) { StopCoroutine(readyCoroutine); readyCoroutine = null; }
        if (firingCoroutine != null) { StopCoroutine(firingCoroutine); firingCoroutine = null; }
        isReady = false;
    }
}
