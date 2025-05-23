using UnityEngine;

[RequireComponent(typeof(Weapon))]
public class WeaponModule : Module
{
    Weapon weapon;
    CoreModule core;

    protected override void Awake()
    {
        base.Awake();
        weapon = GetComponent<Weapon>();
    }
    private void Start()
    {
        ConnectCore(true);
    }
    protected override void TryDetach()
    {
        ConnectCore(false);
        base.TryDetach();
    }
    protected override void TryAttach()
    {
        base.TryAttach();
        ConnectCore(true);
    }

    public void ConnectCore(bool connect_disConnect)
    {
        if(connect_disConnect && core == null)
        {
            if(transform.root.TryGetComponent(out CoreModule _core))
            {
                core = _core;
                core.AddConnectedWeapon(weapon);
            }
        }
        else if(core != null) // || connect_disConnect = false;
        {
            core.RemoveConnectedWeapon(weapon);
            core = null;
        }
    }
}
