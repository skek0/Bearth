using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PointerInputHandler : SceneSingleton<PointerInputHandler>
{
    [Header("Refs")]
    [SerializeField] private Camera mainCam;

    [Header("Physics")]
    [SerializeField] private LayerMask layerMask;

    public event Action<GameObject> HoverTargetChanged;

    private PlayerInputActions.PointerControlActions pointerControl;

    private bool dragging;
    private ISelectable controllingObj;
    private IHoverable currentHover;

    protected override void Awake()
    {
        base.Awake();
        if (mainCam == null) mainCam = Camera.main;
        if (mainCam == null)
            Debug.LogError("[PointerInputHandler] mainCam is null");
    }

    private void OnEnable()
    {
        //pointerControl = InputController.Instance.Actions.PointerControl;
        pointerControl = new PlayerInputActions().PointerControl;
        pointerControl.Enable();

        pointerControl.Select.performed += OnSelectStart;
        pointerControl.Select.canceled += OnSelectEnd;
    }

    private void OnDisable()
    {
        pointerControl.Disable();
        pointerControl.Select.performed -= OnSelectStart;
        pointerControl.Select.canceled -= OnSelectEnd;
        
        dragging = false;
        ClearCurrentControl();
    }

    private void Update()
    {
        UpdateHover();
        UpdateDrag();
    }

    private void UpdateHover()
    {
        Vector2 screenPos = pointerControl.PointerPos.ReadValue<Vector2>();

        var sp = new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z);
        Vector2 worldPos = mainCam.ScreenToWorldPoint(sp);

        Collider2D col = Physics2D.OverlapPoint(worldPos, layerMask);

        IHoverable nextHover = null;

        if (col != null)
            col.TryGetComponent<IHoverable>(out nextHover);

        if (ReferenceEquals(currentHover, nextHover))
        {
            currentHover?.OnHoverStay();
            return;
        }

        currentHover?.OnHoverExit();
        currentHover = nextHover;
        if(currentHover != null)
        {
            currentHover.OnHoverEnter();
            HoverTargetChanged?.Invoke(col.gameObject);
        }
    }

    private void UpdateDrag()
    {
        // 드래그중인 오브젝트 파괴 고려
        if (controllingObj.IsNull()) return;
        if (!dragging) return;

        Vector2 screenPos = pointerControl.PointerPos.ReadValue<Vector2>();
        controllingObj.OnDrag(screenPos);
    }


    private void OnSelectStart(InputAction.CallbackContext ctx)
    {
        ClearCurrentControl();
        dragging = false;

        if (mainCam == null) return;

        Vector2 screenPos = pointerControl.PointerPos.ReadValue<Vector2>();

        var sp = new Vector3(screenPos.x, screenPos.y, -mainCam.transform.position.z);
        Vector2 worldPos = mainCam.ScreenToWorldPoint(sp);

        Collider2D col = Physics2D.OverlapPoint(worldPos, layerMask);
        if (col == null) return;

        if (!col.TryGetComponent<ISelectable>(out var selectable) || selectable == null)
            return;

        controllingObj = selectable;

        dragging = true;
        controllingObj.OnSelected();

    }

    private void OnSelectEnd(InputAction.CallbackContext ctx)
    {
        if (!dragging) return;

        dragging = false;

        if (controllingObj.IsNull()) return;
        controllingObj.OnDeselected();

        ClearCurrentControl();
    }

    private void ClearCurrentControl()
    {
        if (controllingObj == null) return;
        
        controllingObj = null;
    }
}