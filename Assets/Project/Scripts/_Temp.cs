using UnityEngine;

public class Temp : MonoBehaviour
{
    private void Start()
    {       
        ModuleMaker.CreateModule("core_mk1", GameSettings.Instance.ShipContainer);

        ModuleMaker.CreateModule("beam_mk1", GameSettings.Instance.ModuleContainer);
        
        ModuleMaker.CreateModule("shotgun_mk1", GameSettings.Instance.ModuleContainer);

        ModuleMaker.CreateModule("gun_mk1", GameSettings.Instance.ModuleContainer);

        GameObject player = ModuleMaker.CreateModule("player_mk1", GameSettings.Instance.ShipContainer);

        CameraRebinder.BindTo(player.transform);
    }
}
