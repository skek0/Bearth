using UnityEngine;

public class GameSettings : GlobalSingleton<GameSettings>
{
    public float moduleDragSpeed;
    public ModuleRigidSettings Rigidbody2DSettings;
    public float moduleTorqueOnExplosion;

    public Transform ShipContainer;
    public Transform ModuleContainer;

    private void Start()
    {
        if(ShipContainer == null)
            ShipContainer = Util.FindOrCache("ShipContainer").transform;
        if(ModuleContainer == null)
            ModuleContainer = Util.FindOrCache("ModuleContainer").transform;
    }
}
