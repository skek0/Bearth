using UnityEngine;

[RequireComponent(typeof(CoreModule))]

public class CoreWeapon : MonoBehaviour
{
    [SerializeField] ConnectorPort frontConnector;
    [SerializeField] Transform firePoint;
    [SerializeField] GameObject projectilePrefab;

    RangedWeaponStat rangedStat = new();
    IRangedBehavior rangedBehavior = new ProjectileFiring();
    IFireBehavior fireBehavior = new SingleFire();
    RangedFireController fireController = new();

    CoreModule core;

    void Awake()
    {
        core = GetComponent<CoreModule>();
    }

    void Start()
    {
        Initialize();
    }

    void OnEnable()
    {
        core.OnAttackCommand += Attack;
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

    void Initialize()
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

        fireController.Bind(this, firePoint, rangedBehavior, rangedStat, projectilePrefab, core.ModuleGuid.Guid);
        fireController.OnEnable(true); // 항상 준비 상태 — 실제 발사 가능 여부는 Attack()에서 판정
    }
}
