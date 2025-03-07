using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

public class CoreModule : Module
{
    [SerializeField]List<Weapon> connectedWeapons;

    protected override void Awake()
    {
        isControllable = false;
        base.Awake();
    }
    public void AddConnectedWeapon(Weapon weapon)
    {
        connectedWeapons.Add(weapon);
    }
    public void RemoveConnectedWeapon(Weapon weapon)
    {
        connectedWeapons.Remove(weapon);
    }

    public void CommandAttack()
    {
        foreach (var weapon in connectedWeapons)
        {
            weapon.Attack();
        }
    }

    protected override void Die()
    {
        
    }

    private void BeFragile()
    {

    }
}
