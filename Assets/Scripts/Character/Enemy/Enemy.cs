using UnityEngine;

[RequireComponent(typeof(MoveHorizontal))]
[RequireComponent(typeof(MoveVertical))]
[RequireComponent(typeof(Rotation))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(DetectionRange))]
public class Enemy : MonoBehaviour
{
    MoveHorizontal m_MoveHorizontal;
    MoveVertical m_MoveVertical;
    Rotation m_Rotation;
    DetectionRange m_DetectionRange;

    enum State
    {
        IDLE,
        ALERT,
        ATTACK,
    }

    private void Awake()
    {
        m_MoveHorizontal = GetComponent<MoveHorizontal>();
        m_MoveVertical = GetComponent<MoveVertical>();
        m_Rotation = GetComponent<Rotation>();
        m_DetectionRange = GetComponent<DetectionRange>();
    }

}
