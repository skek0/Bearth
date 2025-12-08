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

        if (controllingObj != null)
        {
            controllingObj.OnDestroyed -= OnControllableDestroyed;
            controllingObj = null;
        }
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

        if (controllingObj != null)
        {
            controllingObj.OnDestroyed -= OnControllableDestroyed;
            controllingObj = null;
        }

        if (hit.collider != null)
        {
            var controllable = hit.collider.GetComponent<IControllable>();
            if (controllable != null)
            {
                controllingObj = controllable;
                controllingObj.OnDestroyed += OnControllableDestroyed;
                controllingObj.OnSelected();
            }
            else
            {
                // IControllable이 아니면 드래그 취소
                dragging = false;
            }
        }
        else
        {
            dragging = false;
        }
    }

    private void OnSelectHoldEnd(InputAction.CallbackContext ctx)
    {
        dragging = false;

        if(controllingObj != null)
        {
            controllingObj.OnDestroyed -= OnControllableDestroyed;
            controllingObj?.OnDeselected();
            controllingObj = null;
        }
    }

    void OnControllableDestroyed(IControllable obj)
    {
        if (controllingObj == obj)
        {
            controllingObj.OnDestroyed -= OnControllableDestroyed;
            controllingObj = null;
        }

        dragging = false;
    }    
}