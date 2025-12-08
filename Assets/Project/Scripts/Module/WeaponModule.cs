using UnityEngine;

public abstract class WeaponModule : BlankModule, IWeapon
{
    public int FinalDamage { get; protected set; }
    protected bool attackable;
    protected CoreModule belongedCore;

    protected override void OnDeath()
    {
        base.OnDeath();
        attackable = false;

        if (belongedCore != null)
        {
            belongedCore.RemoveWeapon(this);
            belongedCore = null;
        }
    }
    public abstract void Attack();
    public override void Detach(Vector3 detachedFromPos, bool byDemolition = false)
    {
        base.Detach(detachedFromPos, byDemolition);
        SetUnAttakable();
    }
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

    private void SetUnAttakable()
    {
        attackable = false;

        if (belongedCore != null)
        {
            belongedCore.RemoveWeapon(this);
            belongedCore = null;
        }
    }
}
