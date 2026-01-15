using Unity.Cinemachine;
using UnityEngine;

public static class CameraRebinder
{
    public static void BindTo(Transform followTarget)
    {
        var cam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (cam == null || followTarget == null)
        { 
            return;
        }

        cam.Follow = followTarget;
    }
}
