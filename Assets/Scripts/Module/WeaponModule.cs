using UnityEngine;

public abstract class WeaponModule : BlankModule, IWeapon
{
    [SerializeField] protected bool attackable;
    [SerializeField] protected CoreModule belongedCore;
    public abstract void Attack();
    public override void OnSelected()
    {
        base.OnSelected();
        attackable = false;

        if(belongedCore != null)
        {
            belongedCore.RemoveWeapon(this);
            belongedCore = null;
        }
    }
    public override void OnDeselected()
    {
        base.OnDeselected();
        if (transform.parent.TryGetComponent(out CoreModule core))
        {
            belongedCore = core;
            belongedCore.AddWeapon(this);
            attackable = true;
        }
    }
}
