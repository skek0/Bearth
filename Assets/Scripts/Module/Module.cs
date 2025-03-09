using System;
using UnityEngine;
using UnityEngine.InputSystem;

// 모듈
public class Module : ModuleInfo, IDamageable, IClickable
{
    [Header("General")]
    [SerializeField] int maxHP;
    [SerializeField] float mass = 1f;
    [Tooltip("Distance between Parts")]
    [SerializeField] float connectionDistance;

    [Header("On Death")]
    [Tooltip("Explosion : on destroyed")]
    public float explosionForce;
    [Tooltip("Explosion : on destroyed")]
    public float explosionRadius;

    private int hp;

    #region Base Module Function----------------
    protected override void Awake()
    {
        base.Awake();
    }
    private void Start()
    {
        hp = maxHP;
    }

    public void GetDamage(int damage)
    {
        hp -= damage;

        if(hp <= 0)
        {
            Die();
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Bullet") && TryGetComponent(out Bullet bullet))
        {
            GetDamage(bullet.damage);
        }
    }

    protected virtual void Die()
    {
        //DetachChildren();
        //VacateConnector(ConnectorType.Receiver);
        //Destroy(gameObject); // OnDestroy()
        Detach();
        Destroy(gameObject);
    }

    //public void Detached()
    //{
    //    DetachChildren();
    //    transform.SetParent(null);
    //    AddRigidBody();
    //    VacateConnector(ConnectorType.Sender);
    //}

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
            }
        }
    }
    private void VacateConnectors()
    {
        sender.isOccupied = false;
        connectedTo.isOccupied = false;
        connectedTo = null;
        foreach(var receiver in receivers)
        {
            receiver.isOccupied = false;
        }
    }
    private void AddRigidBody()
    {
        rigid = gameObject.AddComponent<Rigidbody2D>();
        rigid.mass = mass;
        rigid.gravityScale = 0f;
    }
    
    //private void VacateConnector(ConnectorType type)
    //{
    //    if (type == ConnectorType.Sender) 
    //    {
    //        transform.SetParent(null);
    //        sender.isOccupied = false;
    //        connectedTo.isOccupied = false;
    //        connectedTo = null;
    //    }
    //    else if(type == ConnectorType.Receiver)
    //    {
    //        foreach(var receiver in receivers)
    //        {
    //            receiver.isOccupied = false;
    //        }
    //    }
    //}

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
                // 방향 계산 (오브젝트에서 폭발 중심 방향)
                Vector2 direction = col.transform.position - transform.position;
                direction.Normalize();

                // 거리 비례하여 힘 적용 (멀어질수록 약해짐)
                float distance = Vector2.Distance(transform.position, col.transform.position);
                float force = explosionForce * (1 - (distance / explosionRadius));

                // 힘 가하기
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
            //DetachChildren();
            gameObject.layer = LayerMask.NameToLayer("ClickedModule");
        }
        else //ignoreCollider == false
        {
            gameObject.layer = LayerMask.NameToLayer("Module");
        }
        //충돌 판정 유무는 Project Setting에서 변경됨
    }

    //private void DetachChildren()
    //{
    //    for (int i = transform.childCount - 1; i >= 0; i--)
    //    {
    //        Transform child = transform.GetChild(i);
    //        if (child.TryGetComponent(out Module part))
    //        {
    //            part.Detached();    // 자식의 자식도 독립되게
    //        }
    //    }
    //    VacateConnector(ConnectorType.Receiver);
    //}

    protected virtual void TryDetach()
    {
        if (sender.isOccupied)
        {
            //VacateConnector(ConnectorType.Sender);
            //AddRigidBody();
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

            float newAngle = Mathf.LerpAngle(currentAngle, targetAngle, Time.deltaTime * 20f); // 부드러운 회전
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
            if (collider.TryGetComponent(out Connector receiver) && // 커넥터이고
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