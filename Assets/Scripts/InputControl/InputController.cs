using UnityEngine;

public class InputController : MonoBehaviour
{
    public static InputController Instance { get; private set; }
    public PlayerInputActions Actions { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject); // 중복 방지
            return;
        }

        Instance = this;
        Actions = new PlayerInputActions();
        DontDestroyOnLoad(gameObject); // 씬 전환에도 유지
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
