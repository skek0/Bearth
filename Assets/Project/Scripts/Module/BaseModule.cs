using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class BaseModule : Module, IControllable
{
    [SerializeField] Transform senderTransform;

    public CoreModule BelongedCore { get; private set; }

    public event Action<BaseModule, CoreModule> AttachedToCore;      // (self, core)
    public event Action<BaseModule, CoreModule> DetachedFromCore;     // (self, oldCore)
    public event Action<BaseModule> Selected;
    public event Action<BaseModule> Deselected;
    public event Action<BaseModule> Died;

    protected float dragSpeed;
    protected Module attachedTo;
    protected float torqueOnExplosion = 0f;

    public event Action<IControllable> OnDestroyed; // 드래그 입력 중단을 위한 이벤트

    Camera cam;
    private Vector2 targetWorldPos;
    private bool hasDragTarget;

    private string attachedParentPortId;

    public Module AttachedTo => attachedTo;
    public string AttachedParentPortId => attachedParentPortId;
    public Transform SenderTransform => senderTransform;
    public IReadOnlyCollection<Collider2D> SelfColliders => selfColliders;
    private HashSet<Collider2D> selfColliders;

    protected override void Awake()
    {
        base.Awake();
        rigid = GetComponent<Rigidbody2D>();
        cam = Camera.main;

        var cols = GetComponents<Collider2D>();
        selfColliders = new HashSet<Collider2D>(cols);
    }

    protected override void Start()
    {
        base.Start();
        if (senderTransform == null)
        {
            senderTransform = transform.Find("Sender");
        }

        dragSpeed = GameManager.Instance.moduleDragSpeed;
        torqueOnExplosion = GameManager.Instance.moduleTorqueOnExplosion;
    }
    
    public void OnDrag(Vector2 pos)
    {
        var sp = new Vector2(pos.x, pos.y);
        targetWorldPos = cam.ScreenToWorldPoint(sp);

        var cand = ConnectionManager.Instance.QueryCandidate(this);
        if (cand != null)
        {
            AimToTransform(cand);

        }
    }

    private void FixedUpdate()
    {
        if (!hasDragTarget || rigid == null) return;

        Vector2 newPos = Vector2.Lerp(rigid.position, targetWorldPos, dragSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(newPos);
    }

    public virtual void OnSelected()
    {
        hasDragTarget = true;
        Detach(transform.position);

        rigid.mass = 0f;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        connectable = false;

        Selected?.Invoke(this);
    }

    public void OnDeselected()
    {
        hasDragTarget = false;

        TryAttach();

        rigid.collisionDetectionMode = CollisionDetectionMode2D.Discrete;

        Deselected?.Invoke(this);
    }

    private void TryAttach()
    {
        var targetConnector = ConnectionManager.Instance.RequestAttach(this);
        if (targetConnector == null) return;
        Attach(targetConnector);
    }

    public virtual void Detach(Vector3 detachedFromPos, bool byDemolition = false)
    {
        attachedParentPortId = null;

        if (BelongedCore != null)
            NotifyDetached();

        if (GetComponent<Rigidbody2D>() == null) // 독립 모듈이 아니었을경우
        {
            rigid = gameObject.AddComponent<Rigidbody2D>();
            GameManager.Instance.Rigidbody2DSettings.ApplyTo(rigid);
        }

        connectable = false;

        transform.parent = ModulesContainer.Instance.transform;
        faction = FactionType.Neutral;

        for (int i = ConnectedModules.Count - 1; i >= 0; i--)
        {
            ConnectedModules[i].Detach(detachedFromPos, byDemolition);
        }

        Vector2 direction = transform.position - detachedFromPos;
        rigid.AddForce(direction, ForceMode2D.Impulse);

        if (byDemolition)
        {
            float torque = UnityEngine.Random.Range(-torqueOnExplosion, torqueOnExplosion);
            rigid.AddTorque(torque, ForceMode2D.Impulse);
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

        if (BelongedCore != null)
            NotifyDetached();

        Detach(transform.position);
        Died?.Invoke(this);
    }
    private void AimToTransform(Transform target)
    {
        Vector2 dir = target.position - transform.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }

    public void Attach(Transform closestConnector)
    {
        if (closestConnector == null)
        {
            FallbackDetachState();
            return;
        }

        // 세이브용
        var port = closestConnector.GetComponent<ConnectorPort>();
        attachedParentPortId = port != null ? port.PortId : null;

        SetPositionAndRotationByConnector(closestConnector);
        SetParent(closestConnector);

        if (TryGetComponent<Rigidbody2D>(out var rb))
            Destroy(rb);

        connectable = true;
        faction = FactionType.Mine;

        var newCore = AddThisToAttachedModuleAndGetCore(closestConnector);
        NotifyAttached(newCore);
    }
    public void FallbackDetachState()
    {
        transform.SetParent(ModulesContainer.Instance.transform);

        if (rigid != null) rigid.mass = 1f;

        attachedParentPortId = null;

        // connectable/faction 정책도 여기서 통일
        connectable = false;
        faction = FactionType.Neutral;
    }
    private void NotifyDetached()
    {
        var old = BelongedCore;
        BelongedCore = null;
        if (old != null)
            DetachedFromCore?.Invoke(this, old);
    }

    private void NotifyAttached(CoreModule newCore)
    {
        if (newCore == null) return;
        if (BelongedCore == newCore) return;

        // 다른 코어였다면 먼저 detach 통보
        if (BelongedCore != null)
            NotifyDetached();

        BelongedCore = newCore;
        AttachedToCore?.Invoke(this, newCore);
    }


    private CoreModule AddThisToAttachedModuleAndGetCore(Transform closestConnector)
    {
        Module _attachedTo = GetModuleByConnector(closestConnector);
        _attachedTo.AddConnectedModule(this);
        attachedTo = _attachedTo;

        if (_attachedTo.TryGetComponent<BaseModule>(out var bModule))
            return bModule.BelongedCore;
        if (_attachedTo.TryGetComponent<CoreModule>(out var cModule))
            return cModule;

        return null;
    }

    private Module GetModuleByConnector(Transform closestConnector)
    {
        Transform current = closestConnector;

        while (current != null)
        {
            if (current.TryGetComponent(out Module module))
                return module;

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
    { // 상위 코어모듈 찾기
        Transform current = connector;

        while (current != null)
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

    public bool LoadAttach(Transform parentConnector)
    {
        if (parentConnector == null) return false;

        EnsureLoadAttachInitialized();

        // 어떤 포트에 붙었는지 저장용으로 기록
        var port = parentConnector.GetComponent<ConnectorPort>();
        attachedParentPortId = port != null ? port.PortId : null;

        // 기존 부착 로직 재사용
        SetPositionAndRotationByConnector(parentConnector);
        SetParent(parentConnector);

        // 부착 상태 전환
        if (TryGetComponent<Rigidbody2D>(out var rb))
            Destroy(rb);

        connectable = true;
        faction = FactionType.Mine;

        // 부모 모듈 연결 관계 반영 + 코어 확정
        var newCore = AddThisToAttachedModuleAndGetCore(parentConnector);
        NotifyAttached(newCore);


        return true;
    }

    private void EnsureLoadAttachInitialized()
    {
        if (senderTransform == null)
            senderTransform = transform.Find("Sender");

        if (cam == null)
            cam = Camera.main;
    }
}
