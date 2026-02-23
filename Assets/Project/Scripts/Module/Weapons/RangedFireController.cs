using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedFireController
{
    MonoBehaviour runner;

    Transform firePoint;
    IRangedBehavior rangedBehavior;
    RangedWeaponStat stat;
    GameObject projectilePrefab;
    string ownerGuid;

    bool isReady;
    Coroutine readyCo;
    Coroutine firingCo;

    bool enableRequested;
    bool requestedAttackable;

    // GC 방지: 재사용 리스트
    readonly List<Shot> shots = new(32);

    FireContext ctx;

    bool IsBound =>
        runner != null &&
        firePoint != null &&
        rangedBehavior != null &&
        stat != null &&
        projectilePrefab != null &&
        !string.IsNullOrEmpty(ownerGuid);

    public void Bind(
        MonoBehaviour runner,
        Transform firePoint,
        IRangedBehavior rangedBehavior,
        RangedWeaponStat stat,
        GameObject projectilePrefab,
        string ownerGuid
    )
    {
        this.runner = runner;
        this.firePoint = firePoint;
        this.rangedBehavior = rangedBehavior;
        this.stat = stat;
        this.projectilePrefab = projectilePrefab;
        this.ownerGuid = ownerGuid;

        // Bind가 늦게 들어온 경우: 이전 enable 요청을 반영
        if (enableRequested)
        {
            ApplyEnable(requestedAttackable);
        }
    }

    public void OnEnable(bool attackable)
    {
        enableRequested = true;
        requestedAttackable = attackable;

        if (!IsBound)
        {
            isReady = false;
            return;
        }

        ApplyEnable(attackable);
    }

    void ApplyEnable(bool attackable)
    {
        // 이전 타이머/발사 정리 (중복 enable 호출 대비)
        StopAll();

        if (!attackable)
        {
            isReady = false;
            return;
        }

        // stat이 null이면 IsBound에서 걸러지지만, 방어적으로
        if (stat == null)
        {
            isReady = false;
            return;
        }

        StartPreDelay(stat.preDelay);
    }

    public void OnDisable()
    {
        enableRequested = false;
        requestedAttackable = false;

        StopAll();
        isReady = false;
    }

    public void StopAll()
    {
        if (runner == null) return;

        if (readyCo != null) { runner.StopCoroutine(readyCo); readyCo = null; }
        if (firingCo != null) { runner.StopCoroutine(firingCo); firingCo = null; }
    }

    public void TryAttack(IFireBehavior fireBehavior)
    {
        if (!isReady) return;
        if (fireBehavior == null) return;
        if (firingCo != null) return;
        if (!IsBound) return;

        firingCo = runner.StartCoroutine(FireRoutine(fireBehavior));
    }

    IEnumerator FireRoutine(IFireBehavior fireBehavior)
    {
        shots.Clear();
        fireBehavior.BuildShots(shots);

        for (int i = 0; i < shots.Count; i++)
        {
            float d = shots[i].delay;
            if (d > 0f)
                yield return CoroutineCache.WaitforSeconds(d);

            ctx = new FireContext(
                firePoint,
                stat,
                stat.damage,
                shots[i].angleDeg,
                projectilePrefab,
                ownerGuid
            );

            rangedBehavior.Fire(ctx);
        }

        isReady = false;
        RestartTimer(stat.interval);
        firingCo = null;
    }

    void StartPreDelay(float preDelay)
    {
        isReady = false;
        RestartTimer(preDelay);
    }

    void RestartTimer(float t)
    {
        if (runner == null) return;

        if (readyCo != null)
        {
            runner.StopCoroutine(readyCo);
            readyCo = null;
        }
        readyCo = runner.StartCoroutine(WaitFor(t));
    }

    IEnumerator WaitFor(float t)
    {
        yield return CoroutineCache.WaitforSeconds(t);
        isReady = true;
        readyCo = null;
    }
}
