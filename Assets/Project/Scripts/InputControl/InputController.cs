using UnityEngine;

public class InputController : GlobalSingleton<InputController>
{
    //public static InputController Instance { get; private set; }
    public PlayerInputActions Actions { get; private set; }

    protected override void Awake()
    {
        base.Awake();

        //Instance = this;
        Actions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        Actions.Enable();
    }

    private void OnDisable()
    {
        Actions.Disable();
    }
}
