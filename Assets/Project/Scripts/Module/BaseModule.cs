using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor.Experimental.GraphView;
using UnityEditor.MemoryProfiler;
using UnityEngine;

public class BaseModule : Module, ISelectable
{
    [SerializeField] Transform senderTransform;

    public CoreModule BelongedCore { get; private set; }
    public int AttachDepth { get; private set; }   // 코어 0, 코어에 직접 부착 1
    public static int GetDepth(Module m) => m is BaseModule b ? b.AttachDepth : 0;


    public event Action<BaseModule, Module> Attached;
    public event Action<BaseModule, Module> Detached;
    public event Action<BaseModule> Selected;
    public event Action<BaseModule> Deselected;
    public event Action<BaseModule> Died;

    protected float dragSpeed;
    protected Module attachedTo;
    protected float torqueOnExplosion = 0f;

    Camera cam;
    private Vector2 targetWorldPos = Vector2.zero;
    private bool hasDragTarget;

    private string attachedParentPortId;

    public Module AttachedTo => attachedTo;
    public string AttachedParentPortId => attachedParentPortId;
    public Transform SenderTransform => senderTransform;
    public IReadOnlyCollection<Collider2D> SelfColliders => selfColliders;
    public bool CanSelect => BelongedCore == null || BelongedCore.Faction != FactionType.Enemy;
    private HashSet<Collider2D> selfColliders;
    Rigidbody2D sourceRb;
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

        dragSpeed = GameSettings.Instance.moduleDragSpeed;
        torqueOnExplosion = GameSettings.Instance.moduleTorqueOnExplosion;
    }

    private void FixedUpdate()
    {
        if (!hasDragTarget || rigid == null) return;
        if (targetWorldPos == Vector2.zero) return;

        Vector2 newPos = Vector2.Lerp(rigid.position, targetWorldPos, dragSpeed * Time.fixedDeltaTime);
        rigid.MovePosition(newPos);
    }

    public virtual void OnSelected()
    {
        Detach(transform.position);

        rigid.mass = 0f;
        rigid.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        connectable = false;

        Selected?.Invoke(this);
    }
    
    public void OnDrag(Vector2 pos)
    {
        hasDragTarget = true;
        targetWorldPos = cam.ScreenToWorldPoint(pos);

        var cand = ConnectionManager.Instance.QueryCandidate(this);
        if (cand != null)
        {
            AimToTransform(cand);
        }
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
        if (targetConnector == null)
        {
            rigid.mass = mass; // 원래 질량으로 복구
            return;
        }
        Attach(targetConnector);
    }

    public virtual void Detach(Vector3 detachedFromPos, bool byDemolition = false)
    {
        attachedParentPortId = null;

        sourceRb = BelongedCore != null ? BelongedCore.Rigid : null;

        var oldParent = attachedTo;
        attachedParentPortId = null;
        sourceRb = BelongedCore != null ? BelongedCore.Rigid : null;

        SetCore(null);
        AttachDepth = 0;

        if (attachedTo != null)
        {
            attachedTo.RemoveConnectedModule(this);
            attachedTo = null;
        }
        if (oldParent != null) Detached?.Invoke(this, oldParent);

        if (GetComponent<Rigidbody2D>() == null) // 독립 모듈이 아니었을경우
        {
            rigid = gameObject.AddComponent<Rigidbody2D>();
            GameSettings.Instance.Rigidbody2DSettings.ApplyTo(rigid);

            // 분리시 튕김 방지용으로 코어의 속도를 그대로 가져오기 - 효용성이 있는지 확인 필요
            if (sourceRb != null)
            {
                rigid.linearVelocity = sourceRb.linearVelocity;
                rigid.angularVelocity = sourceRb.angularVelocity;
            }

            rigid.mass = mass;
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
            rigid.mass = mass;
            return;
        }
        if (!TryResolveParentAndCore(closestConnector, out var parent, out var core))
        {
            Debug.LogError($"[BaseModule] Trying to attach to an unconnected module: {name}");
            rigid.mass = mass;
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

        parent.AddConnectedModule(this);
        attachedTo = parent;
        AttachDepth = GetDepth(parent) + 1;
        SetCore(core);
        Attached?.Invoke(this, parent);
    }
    private void SetCore(CoreModule core)
    {
        if (BelongedCore == core) return;
        BelongedCore?.AddMass(-mass);
        BelongedCore = core;
        BelongedCore?.AddMass(mass);
    }
   
    private static CoreModule ResolveCore(Module m) => m switch
    {
        CoreModule c => c,
        BaseModule b => b.BelongedCore,
        _ => null
    };
   
    private bool TryResolveParentAndCore(Transform connector, out Module parent, out CoreModule core)
    {
        parent = GetModuleByConnector(connector);
        core = parent != null ? ResolveCore(parent) : null;
        return core != null;
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
    { 
        // connector기준으로 위치 맞추기
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
        if (!TryResolveParentAndCore(parentConnector, out var parent, out var core)) return false;

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

        // 부모 모듈 연결 관계 반영 + 코어 확정
        parent.AddConnectedModule(this);
        attachedTo = parent;
        AttachDepth = GetDepth(parent) + 1;
        SetCore(core);
        Attached?.Invoke(this, parent);

        return true;
    }

    public void EnsureLoadAttachInitialized()
    {
        if (senderTransform == null)
            senderTransform = transform.Find("Sender");

        if (cam == null)
            cam = Camera.main;
    }
}
