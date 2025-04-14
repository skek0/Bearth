using TMPro;
using UnityEngine;

public class Rotation : MonoBehaviour
{
    public float rotationSpeed = 10f;
    Rigidbody2D rigid;

    private void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }


    public void Turn(Vector2 direction)
    {
        float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
        Quaternion targetRotation = Quaternion.Euler(0, 0, targetAngle);

        Quaternion currentRotation = Quaternion.Euler(0, 0, rigid.rotation);

        Quaternion newRotation = Quaternion.RotateTowards(currentRotation, targetRotation, rotationSpeed * Time.fixedDeltaTime);

        rigid.MoveRotation(newRotation.eulerAngles.z);
    }
}
