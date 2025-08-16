using UnityEngine;
using System.Collections.Generic;

public class CoreModule : Module
{
    protected List<IWeapon> weapons = new List<IWeapon>();
    protected virtual void Awake()
    {
        connectable = true;
    }
    public virtual void Attack()
    {
        foreach (var weapon in weapons)
        {
            weapon.Attack();
        }
    }

    public void AddWeapon(IWeapon weapon)
    {
        weapons.Add(weapon);
    }
    public void RemoveWeapon(IWeapon weapon)
    {
        weapons.Remove(weapon);
    }
}
