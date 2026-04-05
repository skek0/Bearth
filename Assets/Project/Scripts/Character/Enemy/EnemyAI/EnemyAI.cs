using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public abstract class EnemyAI : MonoBehaviour
{
    public float speedV = 10f;
    public float speedH = 10f;
    public float speedR = 10f;

    [SerializeField] float accelRate = 10f;
    Rigidbody2D rigid;

    enum State
    {
        IDLE,
        ALERT,
        ATTACK,
    }

    protected virtual void Reset()
    {
        RemoveOtherEnemyAI();
    }
    protected virtual void Awake()
    {
        RemoveOtherEnemyAI();

        rigid = GetComponent<Rigidbody2D>();
        rigid.linearDamping = 2f;
    }
    public abstract void UpdateAI(EnemyContext ctx);

    protected void Move(Vector2 direction)
    {
        Vector2 dir = direction.normalized;
        dir.x *= speedV*accelRate; dir.y *= speedH*accelRate;
        rigid.AddRelativeForce(dir, ForceMode2D.Force);
    }
    protected void MoveTo(Vector2 position)
    {
        Vector2 direction = position - (Vector2)transform.position;
        Move(direction);
    }
    protected void Turn(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);
        Quaternion currentRotation = Quaternion.Euler(0, 0, rigid.rotation);

        Quaternion newRotation = Quaternion.RotateTowards(
            currentRotation, 
            targetRotation, 
            speedR * accelRate * Time.fixedDeltaTime
            );

        rigid.MoveRotation(newRotation.eulerAngles.z);
    }
    protected void TurnTo(Vector2 position)
    {
        Vector2 direction = position - (Vector2)transform.position;
        Turn(direction);
    }
    private void RemoveOtherEnemyAI()
    {
        var ais = GetComponents<EnemyAI>();

        foreach (var ai in ais)
        {
            if (ai == this)
                continue;

#if UNITY_EDITOR
            if (!Application.isPlaying)
            {
                DestroyImmediate(ai);
                continue;
            }
#endif
            Destroy(ai);
        }
    }
}

public struct EnemyContext
{
    public GameObject target;

    public EnemyContext(GameObject player) : this()
    {
        target = player;
    }
}
