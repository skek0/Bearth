using UnityEngine;
using UnityEngine.InputSystem;

public class ObjectInputHandler : MonoBehaviour
{
    [SerializeField] LayerMask layerMask;

    private PlayerInputActions.ObjectControlActions objectControl;

    private bool dragging = false;
    private IControllable controllingObj;

    private void OnEnable()
    {
        objectControl = InputController.Instance.Actions.ObjectControl;

        objectControl.SelectHold.performed += OnSelectHoldStart;
        objectControl.SelectHold.canceled += OnSelectHoldEnd;

        objectControl.Enable();
    }

    private void OnDisable()
    {
        objectControl.SelectHold.performed -= OnSelectHoldStart;
        objectControl.SelectHold.canceled -= OnSelectHoldEnd;

        objectControl.Disable();

        ClearCurrentControl();
    }

    private void Update()
    {
        if (dragging && controllingObj != null)
        {
            Vector2 delta = objectControl.Drag.ReadValue<Vector2>();
            controllingObj.OnDrag(delta);
        }
    }

    private void OnSelectHoldStart(InputAction.CallbackContext ctx)
    {
        dragging = true;

        Vector2 screenPos = objectControl.PointerPos.ReadValue<Vector2>();
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        RaycastHit2D hit = Physics2D.Raycast(worldPos, Vector2.zero, 100f, layerMask);

        ClearCurrentControl();

        if (hit.collider == null)
        {
            dragging = false;
            return;
        }

        var controllable = hit.collider.GetComponent<IControllable>();
        if (controllable == null)
        {
            dragging = false;
            return;
        }

        controllingObj = controllable;
        controllingObj.OnDestroyed += OnControllableDestroyed;
        controllingObj.OnSelected();
    }

    private void OnSelectHoldEnd(InputAction.CallbackContext ctx)
    {
        dragging = false;

        if (controllingObj != null)
        {
            controllingObj.OnDeselected();
        }

        ClearCurrentControl();
    }

    private void ClearCurrentControl()
    {
        if (controllingObj != null)
        {
            controllingObj.OnDestroyed -= OnControllableDestroyed;
            controllingObj = null;
        }
    }

    private void OnControllableDestroyed(IControllable obj)
    {
        if (controllingObj == obj)
        {
            ClearCurrentControl();
        }

        dragging = false;
    }
}
