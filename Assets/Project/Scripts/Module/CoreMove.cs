using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class CoreMove : MonoBehaviour
{
    private Rigidbody2D rb;
    private CoreModule core;

    private Vector2 moveInput;      // x: 측면, y: 전방 (로컬 기준)
    private Vector2 lookTarget;     // 월드 좌표
    private bool hasLookTarget;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        // RequireComponent로 먼저 추가되므로 Awake 시점엔 CoreModule이 없을 수 있음
        core = GetComponent<CoreModule>();
    }

    public void SetMoveInput(Vector2 input) => moveInput = input;

    public void SetLookTarget(Vector2 worldPos)
    {
        lookTarget = worldPos;
        hasLookTarget = true;
    }

    public void ClearLookTarget() => hasLookTarget = false;

    private void FixedUpdate()
    {
        var s = core != null ? core.Stat : null;
        if (s == null) return;

        ApplyThrust(s);
        ClampLocalSpeeds(s);
        ApplyRotation(s);
    }

    private void ApplyThrust(CoreStat s)
    {
        Vector2 thrust = s.MaxForwardSpeed * s.AccelRate * Time.fixedDeltaTime * moveInput.y * (Vector2)transform.up;
        Vector2 strafe = s.MaxSideSpeed * s.AccelRate * Time.fixedDeltaTime * moveInput.x * (Vector2)transform.right;
        rb.AddForce(thrust + strafe);
    }

    private void ClampLocalSpeeds(CoreStat s)
    {
        Vector2 v = rb.linearVelocity;
        float vForward = Vector2.Dot(v, transform.up);
        float vStrafe = Vector2.Dot(v, transform.right);

        vForward = Mathf.Clamp(vForward, -s.MaxForwardSpeed * s.BackwardSpeedRatio, s.MaxForwardSpeed);
        vStrafe = Mathf.Clamp(vStrafe, -s.MaxSideSpeed, s.MaxSideSpeed);

        rb.linearVelocity = (Vector2)transform.up * vForward + (Vector2)transform.right * vStrafe;
    }

    private void ApplyRotation(CoreStat s)
    {
        if (!hasLookTarget) return;

        Vector2 dir = lookTarget - rb.position;
        float targetAngle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg - 90f;
        float angleDiff = Mathf.DeltaAngle(rb.rotation, targetAngle);

        if (Mathf.Abs(angleDiff) < s.AngleThreshold)
        {
            rb.angularVelocity = 0f;
            return;
        }

        float torque = angleDiff * s.ProportionalGain - rb.angularVelocity * s.DerivativeGain;
        torque = Mathf.Clamp(torque, -s.MaxTorque, s.MaxTorque);
        rb.AddTorque(torque);
    }
}