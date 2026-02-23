using UnityEngine;

[RequireComponent(typeof(BaseModule))]
public class RangedWeapon : MonoBehaviour, IWeapon
{
    public Transform AttackPoint => firePoint;
    
    [Header("Refs")]
    [SerializeField] Transform firePoint;
    IRangedBehavior rangedBehavior;
    RangedWeaponStat rangedStat = new();
    IFireBehavior fireBehavior;
    [SerializeField] GameObject projectile;

    [Header("디버깅용")]
    [SerializeField]BaseModule module;
    [SerializeField]CoreModule belongedCore;
    [SerializeField]bool attackable;        // is attached to somewhere

    RangedFireController fireController = new();


    void Awake()
    {
        module = GetComponent<BaseModule>();

        // firePoint 자동 할당
        if (firePoint == null)
        {
            var t = transform.Find("FirePoint");
            if (t != null) firePoint = t;
        }
    }
    private void Start()
    {
        Initialize();
    }
    void OnEnable()
    {
        // 모듈 이벤트 구독
        module.AttachedToCore += OnAttach;
        module.DetachedFromCore += OnDetach;
        module.Died += OnDead;
    }

    void OnDisable()
    {
        if (module != null)
        {
            module.AttachedToCore -= OnAttach;
            module.DetachedFromCore -= OnDetach;
            module.Died -= OnDead;
        }

        fireController.OnDisable();
        UnregisterFromCore();
    }

    void OnAttach(BaseModule _, CoreModule core)
    {
        if (core == null) return;
        if (belongedCore == core && attackable) return;

        belongedCore = core;
        belongedCore.AddWeapon(this);
        attackable = true;
        fireController.OnEnable(true);
    }


    void OnDetach(BaseModule _, CoreModule oldCore)
    {
        // 발사 중단 + 등록 해제
        fireController.StopAll();
        attackable = false;

        if (oldCore != null)
            oldCore.RemoveWeapon(this);

        belongedCore = null;
        fireController.OnDisable();
    }

    void OnDead(BaseModule _)
    {
        fireController.StopAll();
        attackable = false;
        UnregisterFromCore();
        fireController.OnDisable();
    }

    void UnregisterFromCore()
    {
        if (belongedCore != null)
        {
            belongedCore.RemoveWeapon(this);
            belongedCore = null;
        }
    }

    public void Attack()
    {
        if(attackable) fireController.TryAttack(fireBehavior);
    }

    void Initialize()
    {
        if (ModuleSpecDB.WeaponRangedStats[module.ModuleId] == null) return;

        var stat = ModuleSpecDB.WeaponRangedStats[module.ModuleId];
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
            "Burst" => new BurstFire(stat.PelletAmount, stat.Accuracy),
            _ => null
        };
        rangedStat.damage   = stat.Damage;
        rangedStat.speed    = stat.Speed;
        rangedStat.accuracy = stat.Accuracy;
        rangedStat.interval = stat.Interval;
        rangedStat.preDelay = stat.PreDelay;

        fireController.Bind(
            this,
            firePoint,
            rangedBehavior,
            rangedStat,
            projectile,
            module.ModuleGuid.Guid
            );
    }
}
