using UnityEngine.InputSystem;
using UnityEngine;

public class CameraInputHandler : SceneSingleton<CameraInputHandler>
{
    private PlayerInputActions.CameraActions cameraAction;
    private ICamera curCamera;

    public void SetCamera(ICamera curCamera)
    {
        this.curCamera = curCamera; 
    }

    private void OnEnable()
    {
        cameraAction = InputController.Instance.Actions.Camera;
        cameraAction.Zoom.performed += OnZoom;
    }

    private void OnDisable()
    {
        cameraAction.Zoom.performed -= OnZoom;
    }

    private void OnZoom(InputAction.CallbackContext ctx)
    {
        curCamera.Zoom(ctx.ReadValue<float>());
    }
}
