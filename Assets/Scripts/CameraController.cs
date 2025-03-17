using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera cinemachineCamera;

    float maxZoom = 20f;
    float minZoom = 5f;
    Coroutine zoomCoroutine;

    private void Awake()
    {
        TryGetComponent(out cinemachineCamera);
        if (cinemachineCamera == null) Debug.Log("CinemachineCamera is NULL!");
    }

    public void OnZoom(float adjustValue)
    {
        float newZoom = cinemachineCamera.Lens.OrthographicSize + adjustValue;
        newZoom = Mathf.Clamp(newZoom, minZoom, maxZoom);
        if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
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
}
