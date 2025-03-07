using UnityEngine;
using UnityEngine.InputSystem;

// Ŭ�� ��ȣ�ۿ밡��
public class ControllableModule : ModuleInfo, IClickable
{
    [SerializeField] protected float connectionDistance;

    protected override void Awake()
    {
        base.Awake();
    }
    private void FixedUpdate()
    {
        HeadingToReceiver();
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        rigid.MovePosition(mousePosition);
    }

    public void OnSelected()
    {
        enabled = true;
        TryDetach();
        SetUnableCollidePlayer(true);
    }
    public void OnDeselected()
    {
        enabled = false;
        SetUnableCollidePlayer(false);
        TryAttach();
    }

    private void SetUnableCollidePlayer(bool ignoreCollide)
    {
        if (ignoreCollide)
        {
            gameObject.layer = LayerMask.NameToLayer("ClickedModule");
            SetChildPartsUnableCollide(true);
        }
        else //ignoreCollider == false
        {
            gameObject.layer = LayerMask.NameToLayer("Module");
            SetChildPartsUnableCollide(false);
        }
        //�浹 ���� ������ Project Setting���� �����
    }

    private void SetChildPartsUnableCollide(bool ignoreCollide)
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            if (transform.GetChild(i).TryGetComponent(out ControllableModule child))
            {
                child.SetUnableCollidePlayer(ignoreCollide);
            }
        }
    }

    private void HeadingToReceiver()
    {
        Transform receiver = GetClosestConnector();
        if (receiver != null)
        {
            HeadingToTarget(receiver.transform);
        }
    }
    void HeadingToTarget(Transform targetReceiver)
    {
        if (targetReceiver != null)
        {
            Vector3 direction = targetReceiver.position - transform.position;
            float targetAngle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg - 90f;
            float currentAngle = transform.eulerAngles.z;

            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 20f); // �ε巯�� ȸ��
            transform.rotation = Quaternion.Euler(0, 0, newAngle);
        }
    }

    protected virtual void TryDetach()
    {
        if (sender.isOccupied)
        {
            transform.SetParent(null);
            sender.isOccupied = false;
            connectedTo.isOccupied = false;
            connectedTo = null;

            rigid = gameObject.AddComponent<Rigidbody2D>();
            rigid.gravityScale = 0f;
        }
    }
    protected virtual void TryAttach()
    {
        Transform closestConnector = GetClosestConnector();
        if (closestConnector != null)
        {
            AttachToReceiver(closestConnector);
            Destroy(rigid);
        }
    }

    Transform GetClosestConnector()
    {
        Collider2D[] nearbyModules =
            Physics2D.OverlapCircleAll(transform.position, connectionDistance, LayerMask.GetMask("Connector"));

        Transform closestConnector = null;
        float closestDistance = float.MaxValue;
        foreach (var collider in nearbyModules)
        {
            if (collider.TryGetComponent(out Connector receiver) && // Ŀ�����̰�
                receiver.IsAttachable(transform)
                )
            {
                float distance = Vector2.Distance(receiver.transform.position, sender.transform.position);
                if (distance < closestDistance)
                {
                    closestDistance = distance;
                    closestConnector = receiver.transform;
                }
            }
        }
        return closestConnector;
    }

    void AttachToReceiver(Transform receiverTransform)
    {
        TurnForReceiver(receiverTransform);

        Vector3 positionOffset = receiverTransform.transform.position - sender.transform.position;
        transform.position += positionOffset;

        transform.SetParent(receiverTransform.transform.parent);

        sender.isOccupied = true;

        Connector receiver = receiverTransform.GetComponent<Connector>();
        receiver.isOccupied = true;
        connectedTo = receiver;
    }

    private void TurnForReceiver(Transform targetTransform)
    {
        transform.up = targetTransform.up;
    }

    //private void OnDrawGizmos()
    //{
    //    Gizmos.color = Color.green;
    //    Gizmos.DrawWireSphere(transform.position, connectionDistance);
    //}
}
