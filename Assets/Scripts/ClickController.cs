using System;
using System.Collections;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClickController : MonoBehaviour
{
    [SerializeField] CinemachineCamera cinemachineCamera;
    InputAction clickAction;
    InputAction zoomAction;

    private IClickable clickedObject;  // 현재 선택된 오브젝트

    float maxZoom = 20f;
    float minZoom = 5f;
    Coroutine zoomCoroutine;

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
        if (cinemachineCamera == null) { Debug.Log("cinemachineCamera is Missing!"); }
    }

    private void OnEnable()
    {
        clickAction.Enable();
        zoomAction.Enable();
    }

    //public void UpgradeZoom()
    //{
    //    maxZoom += 5f;
    //}

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
        float newZoom = cinemachineCamera.Lens.OrthographicSize + zoomAction.ReadValue<float>();
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        if(zoomCoroutine != null) StopCoroutine(zoomCoroutine);
        zoomCoroutine = StartCoroutine(ZoomCamera(newZoom));

    }

    IEnumerator ZoomCamera(float targetSize)
    {
        float startSize = cinemachineCamera.Lens.OrthographicSize;
        float elapsedTime = 0f;
        float duration = 0.15f;

        while (elapsedTime < duration)
        {
            elapsedTime += Time.deltaTime;
            cinemachineCamera.Lens.OrthographicSize = Mathf.Lerp(startSize, targetSize, elapsedTime / duration);
            yield return null;
        }

        cinemachineCamera.Lens.OrthographicSize = targetSize;
    }

    private void OnDisable()
    {
        clickAction.Disable();
        zoomAction.Disable();
    }
}