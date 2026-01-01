using UnityEngine;
using System.Collections.Generic;

public class CoreModule : Module
{
    protected List<IWeapon> weapons = new();
    protected override void Awake()
    {
        base.Awake();
        connectable = true;
    }
    public virtual void Attack()
    {
        for (int i = weapons.Count - 1; i >= 0; i--)
        {
            var weapon = weapons[i];
            if (weapon == null) { weapons.RemoveAt(i); continue; }
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
