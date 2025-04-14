using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour
{
    [SerializeField] CinemachineCamera cinemachineCamera;
    [SerializeField] ParallexBackgroundManager backgroundManager;

    float maxZoom = 20f;
    float minZoom = 5f;
    Coroutine zoomCoroutine;
    CinemachineFollow cinemachinFollow;
    Vector3 damping = new(1f, 1f, 0f);

    private void Awake()
    {
        if (cinemachineCamera == null) Debug.Log("CinemachineCamera is NULL!");
    }
    private void Start()
    {
        backgroundManager = FindAnyObjectByType<ParallexBackgroundManager>();

        cinemachinFollow = cinemachineCamera.GetComponent<CinemachineFollow>();
    }
    
    public void OnZoom(float adjustValue)
    {
        float newZoom = cinemachineCamera.Lens.OrthographicSize + adjustValue;

        if (newZoom > minZoom && newZoom < maxZoom) // Damping 변경때문에 Clamp 사용 안함
        {
            if (zoomCoroutine != null) StopCoroutine(zoomCoroutine);
            zoomCoroutine = StartCoroutine(ZoomCamera(newZoom));
            //cinemachinFollow.TrackerSettings.PositionDamping += damping * adjustValue;
        }
        
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
