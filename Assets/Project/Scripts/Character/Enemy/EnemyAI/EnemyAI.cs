using UnityEngine;

[RequireComponent(typeof(CoreMove))]
public abstract class EnemyAI : MonoBehaviour
{
    CoreMove mover;
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

        mover = GetComponent<CoreMove>();
    }
    /// <summary>매 프레임 이동 입력을 초기화한 뒤 UpdateAI 호출 (범위 안일 때 계속 추진하는 것 방지)</summary>
    public void Tick(EnemyContext ctx)
    {
        mover.SetMoveInput(Vector2.zero);
        mover.ClearLookTarget();
        UpdateAI(ctx);
    }
    public abstract void UpdateAI(EnemyContext ctx);
    protected void Move(Vector2 worldDirection) => mover.SetMoveInput(transform.InverseTransformDirection(worldDirection.normalized));

    protected void MoveTo(Vector2 position)
    {
        Vector2 direction = position - (Vector2)transform.position;
        Move(direction);
    }
    protected void Turn(Vector2 direction) => mover.SetLookTarget((Vector2) transform.position + direction);
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
