using UnityEngine;
public interface IPlayerControl
{
    public void SetMoveInput(Vector2 moveinput);
    public void SetRotationInput(Vector2 rotationinput);
    public void AttackCommand();
}