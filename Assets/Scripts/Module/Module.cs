using UnityEngine;
using UnityEngine.InputSystem;

// ���
public class Module : ModuleInfo, IDamageable, IClickable
{
    [Header("General")]
    [SerializeField] float maxHP;
    [SerializeField] float mass = 1f;
    [Tooltip("Distance between Parts")]
    [SerializeField] float connectionDistance;

    [Header("On Death")]
    [Tooltip("Explosion : on destroyed")]
    public float explosionForce;
    [Tooltip("Explosion : on destroyed")]
    public float explosionRadius;

    private float hp;

    #region Base Module Function----------------
    protected override void Awake()
    {
        base.Awake();
    }
    private void OnEnable()
    {
        hp = maxHP;
    }
    public void GetDamage(float damage)
    {
        hp -= damage;

        if(hp <= 0)
        {
            Die();
        }
    }
    public void Push(Vector2 force)
    {
        rigid.AddForce(force, ForceMode2D.Impulse);
    }

    protected virtual void Die()
    {
        Detach();
        Destroy(gameObject);
    }

    private void Detach()
    {
        transform.SetParent(null);
        SetChildrenDetach();
        VacateConnectors();
        AddRigidBody();
    }
    private void SetChildrenDetach()
    {
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.TryGetComponent(out Module part))
            {
                part.TryDetach();
                part.Push(part.transform.position- transform.position);
            }
        }
    }
    private void VacateConnectors()
    {
        sender.isOccupied = false;
        if (connectedTo != null)
        {
            connectedTo.isOccupied = false;
            connectedTo = null;
        }
        foreach(var receiver in receivers)
        {
            receiver.isOccupied = false;
        }
    }
    private void AddRigidBody()
    {
        if(rigid == null)
        { 
            rigid = gameObject.AddComponent<Rigidbody2D>();
            rigid.mass = mass;
            rigid.gravityScale = 0f;
        }
    }

    private void OnDestroy()
    {
        Explode();
    }

    private void Explode()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, explosionRadius, LayerMask.GetMask("Module"));

        foreach (Collider2D col in colliders)
        {
            Rigidbody2D rb = col.attachedRigidbody;

            if (rb != null)
            {
                // ���� ��� (������Ʈ���� ���� �߽� ����)
                Vector2 direction = col.transform.position - transform.position;
                direction.Normalize();

                // �Ÿ� ����Ͽ� �� ���� (�־������� ������)
                float distance = Vector2.Distance(transform.position, col.transform.position);
                float force = explosionForce * (1 - (distance / explosionRadius));

                // �� ���ϱ�
                rb.AddForce(direction * force, ForceMode2D.Impulse);
            }
        }
    }
    
    #endregion--------------------------------------

    #region if Controllable ------------------------
    private void FixedUpdate()
    {
        HeadingToReceiver();
        Vector2 mousePosition = Camera.main.ScreenToWorldPoint(Mouse.current.position.ReadValue());
        rigid.MovePosition(mousePosition);
    }

    public void OnSelected()
    {
        if (isControllable)
        {
            enabled = true;
            TryDetach();
            SetUnableCollidePlayer(true);
        }
    }
    public void OnDeselected()
    {
        //���ٰ� �ı��Ǹ� ������

        if (isControllable)
        {
            enabled = false;
            SetUnableCollidePlayer(false);
            TryAttach();
        }
    }

    private void SetUnableCollidePlayer(bool ignoreCollide)
    {
        if (ignoreCollide)
        {
            gameObject.layer = LayerMask.NameToLayer("ClickedModule");
        }
        else //ignoreCollider == false
        {
            gameObject.layer = LayerMask.NameToLayer("Module");
        }
        //�浹 ���� ������ Project Setting���� �����
    }

    protected virtual void TryDetach()
    {
        if (sender.isOccupied)
        {
            Detach();
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

    private void HeadingToTarget(Transform targetReceiver)
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
            if (collider.TryGetComponent(out Connector receiver) &&
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
        transform.rotation = Quaternion.LookRotation(Vector3.forward, targetTransform.up);
    }
    #endregion--------------------------------------
}