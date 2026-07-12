using System;
using UnityEngine;

public class CoreWeapon : MonoBehaviour
{
    [SerializeField] ConnectorPort frontConnector;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject projectile;

    RangedWeaponStat rangedStat = new();
    IRangedBehavior rangedBehavior = new ProjectileFiring();
    IFireBehavior fireBehavior = new SingleFire();
    RangedFireController fireController = new();

    CoreModule core;

    private void Awake()
    {
        if (firePoint == null)
        {
            var t = transform.Find("FirePoint");
            if (t != null) firePoint = t;
        }
    }
    public void Initialize()
    {
        core = GetComponent<CoreModule>();
        core.OnAttackCommand += Attack;
        if (frontConnector == null) frontConnector = FindFrontConnector();
        InitWeapon();
    }


    void OnDisable()
    {
        core.OnAttackCommand -= Attack;
        fireController.OnDisable();
    }

    void Attack()
    {
        if (!IsFrontConnectorEmpty()) return;
        fireController.TryAttack(fireBehavior);
    }

    bool IsFrontConnectorEmpty()
    {
        foreach (var m in core.ConnectedModules)
        {
            if (m.AttachedParentPortId == frontConnector.PortId)
                return false;
        }
        return true;
    }
    private ConnectorPort FindFrontConnector()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<ConnectorPort>(out var port) && port.IsCoreWeaponPort)
            {
                return port;
            }
        }
        Debug.LogWarning($"[CoreModule] IsCoreWeaponPort로 지정된 커넥터를 찾지 못했습니다: {name}", this);
        return null;
    }

    void InitWeapon()
    {
        if (!ModuleSpecDB.WeaponRangedStats.TryGetValue(core.ModuleId, out var stat)) return;

        rangedBehavior = stat.FireType switch
        {
            "Projectile" => new ProjectileFiring(),
            "Hitscan" => new HitscanFiring(),
            _ => null
        };
        fireBehavior = stat.FireMode switch
        {
            "Single" => new SingleFire(),
            "Shotgun" => new ShotgunFire(stat.PelletAmount, stat.Accuracy),
            "Burst" => new BurstFire(stat.PelletAmount, stat.BurstInterval),
            _ => null
        };
        rangedStat.damage = stat.Damage;
        rangedStat.speed = stat.Speed;
        rangedStat.accuracy = stat.Accuracy;
        rangedStat.interval = stat.Interval;
        rangedStat.preDelay = stat.PreDelay;

        projectile = Resources.Load("Projectiles/" + stat.ProjectileID) as GameObject;

        fireController.Bind(this, firePoint, rangedBehavior, rangedStat, projectile, core.ModuleGuid.Guid);
        fireController.OnEnable(true); // 항상 준비 상태 — 실제 발사 가능 여부는 Attack()에서 판정
    }
}
