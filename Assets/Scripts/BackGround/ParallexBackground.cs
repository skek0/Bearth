using UnityEngine;

public class ParallexBackground : MonoBehaviour
{
    public Transform target;
    public float parallaxSpeed;
    [SerializeField] float lengthX;
    [SerializeField] float lengthY;

    private Vector3 lastTargetPosition;

    private void Awake()
    {
        lengthX = GetComponent<SpriteRenderer>().bounds.size.x;
        lengthY = GetComponent<SpriteRenderer>().bounds.size.y;
    }
    void Start()
    {
        lastTargetPosition = target.position;
    }
    void LateUpdate()
    {
        Vector3 deltaMovement = target.position - lastTargetPosition;
        transform.position += new Vector3(deltaMovement.x * parallaxSpeed, deltaMovement.y * parallaxSpeed, 0);
        lastTargetPosition = target.position;
    }
}