using UnityEngine;

public class WeaponModule : Module
{
    Weapon weapon;

    protected override void Awake()
    {
        base.Awake();
        if(!TryGetComponent(out weapon))
        {
            Debug.Log($"{gameObject.name} Needs Weapon Component!");
        }
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
        if (transform.root.TryGetComponent(out CoreModule core))
        {
            if (connect_disConnect) core.AddConnectedWeapon(weapon);
            else core.RemoveConnectedWeapon(weapon);
        }
        else { Debug.Log("Weapon Not Attached To Core!"); }
    }
}
