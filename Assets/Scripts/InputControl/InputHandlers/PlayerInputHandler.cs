using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    IPlayerControl mover;
    private PlayerInputActions.PlayerActions player;

    private bool looking;

    private void OnEnable()
    {
        mover = GetComponent<IPlayerControl>();
        player = InputController.Instance.Actions.Player;

        player.Move.performed += OnMove;
        player.Move.canceled += OnMoveCanceled;

        player.LookHold.performed += OnLookStarted;
        player.LookHold.canceled += OnLookCanceled;
    }

    private void OnDisable()
    {
        player.Move.performed -= OnMove;
        player.Move.canceled -= OnMoveCanceled;

        player.LookHold.performed -= OnLookStarted;
        player.LookHold.canceled -= OnLookCanceled;
    }

    private void Update()
    {
        if (looking)
        {
            mover?.SetRotationInput(player.Look.ReadValue<Vector2>());
        }
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        mover?.SetMoveInput(ctx.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        mover?.SetMoveInput(ctx.ReadValue<Vector2>());
    }

    private void OnLookStarted(InputAction.CallbackContext ctx)
    {
        looking = true;
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        looking = false;
    }
}
