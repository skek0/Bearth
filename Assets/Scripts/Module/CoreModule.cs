using NUnit.Framework;
using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class CoreModule : Module
{
    Weapon ownWeapon;
    [SerializeField]List<Weapon> connectedWeapons;

    protected override void Awake()
    {
        isControllable = false;
        base.Awake();
        ownWeapon = GetComponent<Weapon>();
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
        if(IsFrontVacant()) ownWeapon.Attack();

        foreach (var weapon in connectedWeapons)
        {
            weapon.Attack();
        }
    }

    protected override void Die()
    {
        base.Die();
    }

    bool IsFrontVacant()
    {
        if (receivers[0].isOccupied)
        {
            Debug.Log("Occupied");
            return false;
        }
        else
        {
            Debug.Log("Vacant");
            return true;
        }
    }
}
