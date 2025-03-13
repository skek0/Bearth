using UnityEngine;

public class ParallexBackground : MonoBehaviour
{
    public Transform target;
    public float parallaxSpeed;

    private Vector3 lastTargetPosition;

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