using Unity.Cinemachine;
using UnityEngine;

public static class CameraRebinder
{
    public static void BindTo(Transform followTarget)
    {
        var cam = Object.FindFirstObjectByType<CinemachineCamera>();
        if (cam == null || followTarget == null)
        {
            Debug.LogWarning("Camera or follow target is null. Cannot bind camera.");
            return;
        }

        cam.Follow = followTarget;
    }
}
