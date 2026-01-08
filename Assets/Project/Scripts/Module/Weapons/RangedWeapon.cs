using UnityEngine;

[RequireComponent(typeof(BaseModule))]
public class RangedWeapon : MonoBehaviour, IWeapon
{
    public Transform AttackPoint => firePoint;
    
    [Header("Refs")]
    [SerializeField] Transform firePoint;
    [SerializeField] RangedBehavior rangedBehavior;
    [SerializeField] RangedWeaponStat rangedStat;
    [SerializeField] FireBehavior fireBehavior;
    [SerializeField] GameObject projectile;

    BaseModule module;
    CoreModule belongedCore;
    bool attackable;

    readonly RangedFireController fireController = new();


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
        fireController.Bind(
            this, 
            firePoint, 
            rangedBehavior, 
            rangedStat, 
            projectile, 
            module.ModuleGuid.Guid
            );
        
    }

    void OnEnable()
    {
        // 모듈 이벤트 구독
        module.AttachedToCore += OnAttach;
        module.DetachedFromCore += OnDetach;
        module.Died += OnDead;

        // 이미 코어에 붙어있는 상태로 Enable 될 수도 있으니 반영
        if (module.BelongedCore != null)
            OnAttach(module, module.BelongedCore);
        else
            fireController.OnDisable();
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
        fireController.TryAttack(attackable, fireBehavior);
    }
}
