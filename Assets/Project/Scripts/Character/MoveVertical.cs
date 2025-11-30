using UnityEngine;

public class MoveVertical : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.linearDamping = 2f;
    }

    public void Move(float moveY)
    {
        rigid.AddRelativeForceY(speed * moveY, ForceMode2D.Force);
    }
}