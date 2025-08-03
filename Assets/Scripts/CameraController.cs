using Unity.Cinemachine;
using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour, ICamera
{
    [SerializeField] float zoomSpeed;
    [SerializeField] float maxZoom;
    [SerializeField] float minZoom;

    Camera m_camera;

    private void Awake()
    {
        m_camera = GetComponent<Camera>();
    }
    private void Start()
    {
        CameraInputHandler.Instance.SetCamera(this);
    }
    public void Zoom(float adjustValue)
    {
        float newZoom = m_camera.orthographicSize - adjustValue * zoomSpeed;

        if (newZoom >= minZoom && newZoom <= maxZoom) // Damping 변경때문에 Clamp 사용 안함
        {
            m_camera.orthographicSize = newZoom;
        }        
    }

}
