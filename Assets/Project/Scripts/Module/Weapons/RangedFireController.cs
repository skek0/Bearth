using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RangedFireController
{
    MonoBehaviour runner;

    Transform firePoint;
    RangedBehavior rangedBehavior;
    RangedWeaponStat stat;
    GameObject projectilePrefab;

    bool isReady;
    Coroutine readyCo;
    Coroutine firingCo;

    // GC 방지: 재사용 리스트
    readonly List<Shot> shots = new(32);

    public void Bind(
        MonoBehaviour runner,
        Transform firePoint,
        RangedBehavior rangedBehavior,
        RangedWeaponStat stat,
        GameObject projectilePrefab
    )
    {
        this.runner = runner;
        this.firePoint = firePoint;
        this.rangedBehavior = rangedBehavior;
        this.stat = stat;
        this.projectilePrefab = projectilePrefab;
    }

    public void RebindProjectile(GameObject prefab) => this.projectilePrefab = prefab;

    public void OnEnable(bool attackable)
    {
        if (!attackable || stat == null)
        {
            isReady = false;
            return;
        }

        StartPreDelay(stat.preDelay);
    }

    public void OnDisable()
    {
        StopAll();
        isReady = false;
    }

    public void StopAll()
    {
        if (runner == null) return;

        if (readyCo != null) { runner.StopCoroutine(readyCo); readyCo = null; }
        if (firingCo != null) { runner.StopCoroutine(firingCo); firingCo = null; }
    }

    public bool TryAttack(bool attackable, FireBehavior fireBehavior)
    {
        if (!attackable) return false;
        if (!isReady) return false;
        if (fireBehavior == null) return false;
        if (firingCo != null) return false;

        // 의존성 체크
        if (runner == null || firePoint == null || rangedBehavior == null || stat == null || projectilePrefab == null)
            return false;

        firingCo = runner.StartCoroutine(FireRoutine(fireBehavior));
        return true;
    }

    IEnumerator FireRoutine(FireBehavior fireBehavior)
    {
        shots.Clear();
        fireBehavior.BuildShots(shots);

        for (int i = 0; i < shots.Count; i++)
        {
            float d = shots[i].delay;
            if (d > 0f)
                yield return CoroutineCache.WaitforSeconds(d);

            rangedBehavior.Fire(
                firePoint,
                stat,
                stat.damage,
                shots[i].angleDeg,
                projectilePrefab
            );
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
