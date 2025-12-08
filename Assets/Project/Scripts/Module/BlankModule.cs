using System;
using UnityEngine;

public class BlankModule : Module, IControllable
{
    [SerializeField] Transform senderTransform;

    protected Connection connection;
    protected Rigidbody2D rigid;
    protected float dragSpeed;
    protected Module attachedTo;
    protected float torqueOnExplosion = 0f;

    public event Action<IControllable> OnDestroyed;

    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody2D>();
    }
    protected virtual void Start()
    {
        if (senderTransform == null) 
        { 
            senderTransform = transform.Find("Sender");
            Debug.Log("Sender assigned automatically, recommend cache");
        }
        connection = GetComponentInChildren<Connection>(true);
        connection.SetColliderAndAnchor(GetComponents<Collider2D>(), senderTransform);
        dragSpeed = GameManager.Instance.moduleDragSpeed;
        torqueOnExplosion = GameManager.Instance.moduleTorqueOnExplosion;
    }

    public virtual void OnDrag(Vector2 pos)
    {
        DragToMove(pos);

        Transform closestConnector = connection.ClosestConnector;
        if (closestConnector != null)
        {
            Vector2 direction = closestConnector.position - transform.position;
            float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

            transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
        }
    }

    private void DragToMove(Vector2 pos)
    {
        Vector3 screenPos = new Vector3(pos.x, pos.y, 0f);
        Vector2 worldPos = Camera.main.ScreenToWorldPoint(screenPos);

        Vector2 newPosition = Vector2.Lerp(rigid.position, worldPos, dragSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(newPosition);
    }

    public virtual void OnSelected()
    {
        Detach(transform.position);
        rigid.mass = 0f;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        connectable = false;
        connection.gameObject.SetActive(true);
    }
    public virtual void OnDeselected()
    {
        TryAttach();
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Discrete;
        connection.gameObject.SetActive(false);
    }

    public virtual void Detach(Vector3 detachedFromPos, bool byDemolition = false)
    {
        if (GetComponent<Rigidbody2D>() == null) // 독립 모듈이 아니었을경우
        {
            rigid = gameObject.AddComponent<Rigidbody2D>();
            GameManager.Instance.Rigidbody2DSettings.ApplyTo(rigid);
        }

        connectable = false;

        transform.parent = ModulesContainer.Instance.transform;
        faction = FactionType.Neutral;

        for (int i = connectedModules.Count - 1; i >= 0; i--)
        {
            connectedModules[i].Detach(detachedFromPos, byDemolition);
        }

        Vector2 direction = transform.position - detachedFromPos;
        rigid.AddForce(direction, ForceMode2D.Impulse);
        if(byDemolition)
        {
            float torque = UnityEngine.Random.Range(-torqueOnExplosion, torqueOnExplosion);
            rigid.AddTorque(torque, ForceMode2D.Impulse);
            Debug.Log(gameObject.name + " " + torque);
        }

        if (attachedTo != null)
        {
            attachedTo.RemoveConnectedModule(this);
            attachedTo = null;
        }

    }
    protected override void OnDeath()
    {
        base.OnDeath();
        OnDestroyed?.Invoke(this);
    }

    private void TryAttach()
    {
        Transform closestConnector = connection.ClosestConnector;
        if (closestConnector != null)
        {
            SetPositionAndRotationByConnector(closestConnector);
            SetParent(closestConnector);
            Destroy(GetComponent<Rigidbody2D>());
            connectable = true;
            AddThisToAttachedModule(closestConnector);
            faction = FactionType.Mine;
            return;
        }
        transform.SetParent(ModulesContainer.Instance.transform);
        rigid.mass = 1f;
    }

    private void AddThisToAttachedModule(Transform closestConnector)
    {
        Module _attachedTo = GetModuleByConnector(closestConnector);
        _attachedTo.AddConnectedModule(this);
        attachedTo = _attachedTo;
    }

    private Module GetModuleByConnector(Transform closestConnector)
    {
        Transform current = closestConnector;

        while (current != null)
        {
            if (current.TryGetComponent(out Module module))
            {
                return module;
            }
            current = current.parent;
        }
        return null;
    }

    void SetPositionAndRotationByConnector(Transform closestConnector)
    { // connector기준으로 위치 맞추기
        senderTransform.GetLocalPositionAndRotation(out Vector3 localPos, out Quaternion localRot);

        Quaternion targetRot = closestConnector.rotation * Quaternion.Inverse(localRot);
        Vector3 targetPos = closestConnector.position - (targetRot * localPos);

        transform.SetPositionAndRotation(targetPos, targetRot);
    }

    private void SetParent(Transform connector)
    { //상위 코어모듈 찾기
        Transform current = connector;

        while(current != null)
        {
            if (current.GetComponent<CoreModule>() != null)
            {
                transform.SetParent(current);
                return;
            }
            current = current.parent;
        }
        transform.SetParent(ModulesContainer.Instance.transform);
    }
}
