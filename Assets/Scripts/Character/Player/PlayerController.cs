using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(MoveHorizontal))]
[RequireComponent(typeof(MoveVertical))]
[RequireComponent(typeof(Rotation))]
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController : MonoBehaviour
{
    [Tooltip("Vertical Speed")]
    public float throttleSpeed;
    [Tooltip("Horizontal Speed")]
    public float thrustSpeed;
    [Tooltip("Rotation Speed")]
    public float rotateSpeed;

    InputAction moveAction;
    InputAction rotateAction;
    InputAction attackAction;

    MoveVertical m_moveVertical;
    MoveHorizontal m_moveHorizontal;
    Rotation m_rotation;

    Camera mainCamera;
    Rigidbody2D rigid;
    float moveX, moveY, isRotating, isAttacking;

    CoreModule playerPart;

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rotateAction = InputSystem.actions.FindAction("Aim");
        attackAction = InputSystem.actions.FindAction("Attack");

        m_moveVertical = GetComponent<MoveVertical>();
        m_moveHorizontal = GetComponent<MoveHorizontal>();
        m_rotation = GetComponent<Rotation>();

        playerPart = GetComponent<CoreModule>();
        rigid = GetComponent<Rigidbody2D>();
    }

    private void Start()
    {
        mainCamera = Camera.main;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        rotateAction.Enable();
        attackAction.Enable();
    }

    void Update()
    {
        moveY = moveAction.ReadValue<Vector2>().y * throttleSpeed;
        moveX = moveAction.ReadValue<Vector2>().x * thrustSpeed;
        isRotating = rotateAction.ReadValue<float>();
        isAttacking = attackAction.ReadValue<float>();
        if(isAttacking > 0)
        {
            TryAttack();
        }
    }
    private void FixedUpdate()
    {
        m_moveVertical.Move(moveY * 10f * Time.fixedDeltaTime);
        m_moveHorizontal.Move(moveX * 10f * Time.fixedDeltaTime);
        if (isRotating > 0.1) TurnShip();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        rotateAction.Disable();
        attackAction.Disable();
    }

    private void TurnShip()
    {
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f;
        m_rotation.Turn((mousePosition - transform.position).normalized);
    }

    private void TryAttack()
    {
        playerPart.CommandAttack();
    }

}