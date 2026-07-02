using UnityEngine;

public class Temp : MonoBehaviour
{
    private void Start()
    {       
        ModuleMaker.CreateModule("core_mk1");

        ModuleMaker.CreateModule("beam_mk1");

        ModuleMaker.CreateModule("shotgun_mk1");

        ModuleMaker.CreateModule("gun_mk1");
    }
}
