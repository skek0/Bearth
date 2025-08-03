using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInputHandler : MonoBehaviour
{
    [SerializeField] LayerMask layerMask; // 클릭가능한 모듈

    private PlayerInputActions.ObjectControlActions objectControl;

    private bool dragging = false;
    IControllable controllingObj;

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
            controllingObj?.OnDrag(objectControl.Drag.ReadValue<Vector2>());
        }
    }

    private void OnSelectHoldStart(InputAction.CallbackContext ctx)
    {
        dragging = true; 
        Vector2 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero, 100f, layerMask);

        if (hit.collider != null)
        {
            controllingObj = hit.collider.GetComponent<IControllable>();
            controllingObj?.OnSelected();
        }
    }

    private void OnSelectHoldEnd(InputAction.CallbackContext ctx)
    {
        dragging = false;

        controllingObj?.OnDeselected();
        controllingObj = null;
    }
}