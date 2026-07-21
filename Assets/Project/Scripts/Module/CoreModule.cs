using System;
using UnityEngine;
using System.Collections.Generic;

public class CoreModule : Module
{
    public event Action OnAttackCommand;

    [SerializeField] Transform frontConnector;
    public Transform FrontConnector => frontConnector;


    protected override void Awake()
    {
        base.Awake();
        connectable = true;
        rigid = GetComponent<Rigidbody2D>();
    }
    protected override void Start()
    {
        base.Start();
        frontConnector = FindCoreWeaponPort();
        var coreWeapon = gameObject.AddComponent<CoreWeapon>();
        coreWeapon.Initialize();
    }
    public virtual void Attack()
    {
        OnAttackCommand?.Invoke();
    }
    public void AddMass(float mass)
    {
        rigid.mass += mass;
    }
    private Transform FindCoreWeaponPort()
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent<ConnectorPort>(out var port) && port.IsCoreWeaponPort)
            {
                return child;
            }
        }
        Debug.LogWarning($"[CoreModule] IsCoreWeaponPort로 지정된 커넥터를 찾지 못했습니다: {name}", this);
        return null;
    }
}
