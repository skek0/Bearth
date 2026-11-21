using UnityEngine;


[RequireComponent(typeof(PlayerInputHandler))]
public class PlayerModule : CoreModule, IPlayerControl
{
    CoreMove moveComponent;

    protected override void Awake()
    {
        base.Awake();
        moveComponent = GetComponent<CoreMove>();
    }
    public void SetMoveInput(Vector2 moveinput)
    {
        moveComponent.SetMoveInput(moveinput);
    }

    public void SetRotationInput(Vector2 rotationinput)
    {
        moveComponent.SetLookTarget(Camera.main.ScreenToWorldPoint(rotationinput));
    }

    public void ClearRotationInput()
    {
        moveComponent.ClearLookTarget();
    }

public void AttackCommand()
    {
        base.Attack();
    }
}
