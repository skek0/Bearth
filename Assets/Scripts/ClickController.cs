using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickController : MonoBehaviour
{
    [SerializeField] CameraController cameraController;
    InputAction clickAction;
    InputAction zoomAction;

    private IClickable clickedObject;  // 현재 선택된 오브젝트


    private void Awake()
    {
        clickAction = InputSystem.actions.FindAction("Shift");
        clickAction.performed += ctx => OnClickStart();
        clickAction.canceled += ctx => OnClickEnd();
        zoomAction = InputSystem.actions.FindAction("Zoom");
        zoomAction.performed += ctx => OnZoom();
    }
    private void Start()
    {
        if (cameraController == null) { Debug.Log("cameraController is Missing!"); }
    }

    private void OnEnable()
    {
        clickAction.Enable();
        zoomAction.Enable();
    }

    private void OnClickStart()
    {
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        RaycastHit2D hit = Physics2D.Raycast(mousePosition, Vector2.zero);

        if (hit.collider != null)
        {
            IClickable selectable = hit.collider.GetComponent<IClickable>();
            if (selectable != null)
            {
                // 이전 오브젝트 선택 해제
                clickedObject?.OnDeselected();
                // 새로운 오브젝트 선택
                clickedObject = selectable;
                clickedObject.OnSelected();
            }
        }
    }
    private void OnClickEnd()
    {
        // 클릭을 놓으면 선택 해제
        clickedObject?.OnDeselected();
        clickedObject = null;
    }

    private void OnZoom()
    {
        cameraController.OnZoom(zoomAction.ReadValue<float>());
    }

    private void OnDisable()
    {
        clickAction.Disable();
        zoomAction.Disable();
    }   
}