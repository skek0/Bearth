using UnityEngine;

public class PlayerModule : CoreModule, IPlayerControl
{
    PlayerMove moveComponent;
    PlayerAttack attackComponent;

    protected override void Awake()
    {
        base.Awake();

        moveComponent = GetComponent<PlayerMove>();
        attackComponent = GetComponent<PlayerAttack>();
    }
    public void SetMoveInput(Vector2 moveinput)
    {
        moveComponent.SetMoveInput(moveinput);
    }

    public void SetRotationInput(Vector2 rotationinput)
    {
        moveComponent.SetRotationInput(rotationinput);
    }

    public void AttackCommand()
    {
        attackComponent.AttackCommand();
    }
}
