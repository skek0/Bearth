using UnityEngine;

public class MoveHorizontal : MonoBehaviour
{
    public float speed = 10f;
    private Rigidbody2D rigid;

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
        rigid.linearDamping = 2f;
    }

    public void Move(float moveX)
    {
        rigid.AddRelativeForceX(speed * moveX, ForceMode2D.Force);
    }
}
