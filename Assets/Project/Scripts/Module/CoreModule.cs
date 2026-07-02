using System;
using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
public class CoreModule : Module
{
    public event Action OnAttackCommand;

    protected override void Awake()
    {
        base.Awake();
        connectable = true;
        rigid = GetComponent<Rigidbody2D>();
    }
    public virtual void Attack()
    {
        OnAttackCommand?.Invoke();
    }
    public void AddMass(float mass)
    {
        rigid.mass += mass;
    }
}
