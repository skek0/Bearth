using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputHandler : MonoBehaviour
{
    IPlayerControl player;
    private PlayerInputActions.PlayerActions playeraction;

    private bool looking;
    private bool attacking;

    private void Start()
    {
        SetPlayer(GetComponent<IPlayerControl>());
    }
    public void SetPlayer(IPlayerControl _player)
    {
        player = _player;
    }
    private void OnEnable()
    {
        playeraction = InputController.Instance.Actions.Player;

        playeraction.Move.performed += OnMove;
        playeraction.Move.canceled += OnMoveCanceled;

        playeraction.LookHold.performed += OnLookStarted;
        playeraction.LookHold.canceled += OnLookCanceled;

        playeraction.Attack.performed += OnAttackStarted;
        playeraction.Attack.canceled += OnAttackCanceled;
    }

    private void OnDisable()
    {
        playeraction.Move.performed -= OnMove;
        playeraction.Move.canceled -= OnMoveCanceled;

        playeraction.LookHold.performed -= OnLookStarted;
        playeraction.LookHold.canceled -= OnLookCanceled;

        playeraction.Attack.performed -= OnAttackStarted;
        playeraction.Attack.canceled -= OnAttackCanceled;
    }

    private void Update()
    {
        if (looking)
        {
            player?.SetRotationInput(playeraction.Look.ReadValue<Vector2>());
        }
        if (attacking)
        {
            player?.AttackCommand();
        }
    }

    private void OnMove(InputAction.CallbackContext ctx)
    {
        player?.SetMoveInput(ctx.ReadValue<Vector2>());
    }

    private void OnMoveCanceled(InputAction.CallbackContext ctx)
    {
        player?.SetMoveInput(ctx.ReadValue<Vector2>());
    }

    private void OnLookStarted(InputAction.CallbackContext ctx)
    {
        looking = true;
    }

    private void OnLookCanceled(InputAction.CallbackContext ctx)
    {
        looking = false;
    }
    private void OnAttackStarted(InputAction.CallbackContext ctx)
    {
        attacking = true;
    }
    private void OnAttackCanceled(InputAction.CallbackContext ctx)
    {
        attacking= false;
    }
}
