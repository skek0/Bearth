using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [SerializeField] private float maxThrustSpeed;
    [SerializeField] private float maxThrottleSpeed;
    [SerializeField] private float maxRotationSpeed;

    [SerializeField] private float rotationSmoothFactor = 5f;       // P-계수

    private Rigidbody2D rb;

    private Vector2 moveInput;
    private Vector2 rotationInput;
    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    public void SetMoveInput(Vector2 input)
    {
        moveInput = input;
    }

    public void SetRotationInput(Vector2 input)
    {
        rotationInput = input;
    }

    private void FixedUpdate()
    {
        // 이동 처리
        Vector2 thrust = maxThrustSpeed * moveInput.y * transform.up;
        Vector2 throttle = maxThrottleSpeed * moveInput.x * transform.right;
        rb.AddForce(thrust + throttle);

        // 회전 처리 (감속 회전)
        if (rotationInput != Vector2.zero)
        {
            // 목표 각도 계산 (월드 좌표에서)
            Vector2 dir = (Vector2)Camera.main.ScreenToWorldPoint(rotationInput) - rb.position;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            // 현재 각도
            float currentAngle = rb.rotation;

            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            // 비례 제어 + 최대 회전 속도 제한
            float angularVelocity = Mathf.Clamp(angleDiff * rotationSmoothFactor, -maxRotationSpeed, maxRotationSpeed);

            rb.MoveRotation(currentAngle + angularVelocity * Time.fixedDeltaTime);
        }
    }
}
