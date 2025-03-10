using System;
using UnityEngine;
using UnityEngine.InputSystem;

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
    CoreModule playerPart;

    Camera mainCamera;
    Rigidbody2D rigid;
    float moveX, moveY, isRotating, isAttacking;

    private void Awake()
    {
        moveAction = InputSystem.actions.FindAction("Move");
        rotateAction = InputSystem.actions.FindAction("Aim");
        attackAction = InputSystem.actions.FindAction("Attack");

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
        rigid.AddRelativeForceY(moveY * 10f * Time.fixedDeltaTime, ForceMode2D.Force);
        rigid.AddRelativeForceX(moveX * 10f * Time.fixedDeltaTime, ForceMode2D.Force);
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
        // 마우스 위치를 월드 좌표로 변환
        Vector3 mousePosition = mainCamera.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        mousePosition.z = 0f; // 2D 환경이므로 Z는 0으로 설정

        Vector3 direction = (mousePosition - transform.position).normalized;
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;

        // 현재 회전을 Quaternion으로 변환
        Quaternion currentRotation = Quaternion.Euler(0f, 0f, rigid.rotation);
        Quaternion targetRotation = Quaternion.Euler(0f, 0f, targetAngle);

        // Slerp을 사용하여 부드러운 회전 적용
        Quaternion smoothRotation = Quaternion.Slerp(currentRotation, targetRotation, rotateSpeed * Time.fixedDeltaTime);

        // Rigidbody2D를 이용해 회전 적용
        rigid.MoveRotation(smoothRotation.eulerAngles.z);
    }
    private void TryAttack()
    {
        playerPart.CommandAttack();
    }

}