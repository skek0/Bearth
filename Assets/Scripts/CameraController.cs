using UnityEngine;

public class CameraController : MonoBehaviour
{
    public float followingSpeed;
    [SerializeField] Transform player;
    Camera thisCamera;

    private void Awake()
    {
        thisCamera = GetComponent<Camera>();
    }
    private void Start()
    {
        player = GameObject.FindWithTag("Player").transform;    
    }

    private void Update()
    {
        transform.position = Vector3.Lerp(transform.position, player.position, followingSpeed) + Vector3.back*10;
    }

    public void SetZoom(int size)
    {
        thisCamera.orthographicSize = size;
    }

}
