using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInputHandler : MonoBehaviour
{
    private PlayerInputActions.ObjectControlActions objectControl;

    private bool dragging = false;

    private void OnEnable()
    {
        objectControl = InputController.Instance.Actions.ObjectControl;

        objectControl.SelectHold.performed += OnSelectHoldStart;
        objectControl.SelectHold.canceled += OnSelectHoldEnd;
    }

    private void OnDisable()
    {
        objectControl.SelectHold.performed -= OnSelectHoldStart;
        objectControl.SelectHold.canceled -= OnSelectHoldEnd;
    }

    private void Update()
    {
        if (dragging)
        {
            Vector2 delta = objectControl.Drag.ReadValue<Vector2>();
            // 선택한 오브젝트 이동
        }
    }

    private void OnSelectHoldStart(InputAction.CallbackContext ctx)
    {
        dragging = true;
    }

    private void OnSelectHoldEnd(InputAction.CallbackContext ctx)
    {
        dragging = false;
    }
}