using UnityEngine;

[RequireComponent(typeof(MoveHorizontal))]
[RequireComponent(typeof(MoveVertical))]
[RequireComponent(typeof(Rotation))]
[RequireComponent(typeof(Rigidbody2D))]
public class Enemy : MonoBehaviour
{
    MoveHorizontal m_MoveHorizontal;
    MoveVertical m_MoveVertical;
    Rotation m_Rotation;

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
    }

    private void Update()
    {

    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        //if(collision.)
    }
}
