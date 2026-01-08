using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMove : MonoBehaviour
{
    [Header("Movement")]
    public float maxForwardSpeed = 14f; // transform.up 방향
    public float maxSideSpeed = 8f;  // transform.right 방향
    [SerializeField] private float accelRate = 2f;

    [Header("Rotation")]
    [SerializeField] private float proportionalGain = 2.0f;     // P 게인
    [SerializeField] private float derivativeGain = 0.7f;       // D 게인
    [SerializeField] private float maxTorque = 15f;             // 최대 토크 제한
    [Tooltip("Rotate stopping difference")]
    [SerializeField] float angleThreshold = 0.2f;               // 목표값에 허용되는 오차 (0.1~0.5 정도 권장)

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
        Vector2 thrust = maxForwardSpeed * accelRate * Time.fixedDeltaTime * moveInput.y * transform.up;
        Vector2 throttle = maxSideSpeed * accelRate * Time.fixedDeltaTime * moveInput.x * transform.right;
        rb.AddForce(thrust + throttle);

        AdjustLocalSpeeds();
        AdjustLocalRotates();
    }

    void AdjustLocalSpeeds()
    {
        Vector2 v = rb.linearVelocity;
        // 로컬 축 분해
        float vForward = Vector2.Dot(v, transform.up);
        float vStrafe = Vector2.Dot(v, transform.right);

        // 축별 클램프
        vForward = Mathf.Clamp(vForward, -maxForwardSpeed/2, maxForwardSpeed);
        vStrafe = Mathf.Clamp(vStrafe, -maxSideSpeed, maxSideSpeed);

        // 월드 벡터로 재합성
        rb.linearVelocity = transform.up * vForward + transform.right * vStrafe;
    }

    private void AdjustLocalRotates()
    {
        if (rotationInput != Vector2.zero)
        {
            Vector2 dir = (Vector2)Camera.main.ScreenToWorldPoint(rotationInput) - rb.position;
            float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;

            float currentAngle = rb.rotation;
            float angleDiff = Mathf.DeltaAngle(currentAngle, targetAngle);

            if (Mathf.Abs(angleDiff) < angleThreshold)
            {
                rb.angularVelocity = 0f;
                return;
            }

            // PD 제어
            float torque = angleDiff * proportionalGain - rb.angularVelocity * derivativeGain;

            // 토크 제한
            torque = Mathf.Clamp(torque, -maxTorque, maxTorque);

            rb.AddTorque(torque);
        }
    }
}
