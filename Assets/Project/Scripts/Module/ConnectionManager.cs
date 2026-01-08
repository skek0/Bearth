using System.Linq;
using UnityEngine;

public class ConnectionManager : MonoBehaviour
{
    public static ConnectionManager Instance { get; private set; }

    [Header("Scan (Connector)")]
    [SerializeField] private float scanRadius = 2.0f;
    [SerializeField] private int connectorBufferSize = 64;

    [Header("Place Check (Module Overlap)")]
    [SerializeField] private float sizeOffset = 0.97f;
    [SerializeField] private int overlapBufferSize = 64;

    private Collider2D[] connectorBuffer;
    private Collider2D[] overlapBuffer;

    private ContactFilter2D connectorFilter;
    private ContactFilter2D moduleFilter;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        connectorBuffer = new Collider2D[Mathf.Max(32, connectorBufferSize)];
        overlapBuffer = new Collider2D[Mathf.Max(32, overlapBufferSize)];

        connectorFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Connector"),
            useTriggers = true
        };

        moduleFilter = new ContactFilter2D
        {
            useLayerMask = true,
            layerMask = LayerMask.GetMask("Module"),
            useTriggers = false
        };
    }

    public Transform QueryCandidate(BaseModule module)
    {
        if (module == null) return null;

        Vector2 scanCenter = module.SenderTransform != null
            ? (Vector2)module.SenderTransform.position
            : (Vector2)module.transform.position;

        // ✅ NonAlloc 대신: OverlapCircle + results 배열 오버로드
        int hitCount = Physics2D.OverlapCircle(scanCenter, scanRadius, connectorFilter, connectorBuffer);
        if (hitCount <= 0) return null;

        Transform best = null;
        float bestSqr = float.MaxValue;
        Vector3 myPos = module.transform.position;

        for (int i = 0; i < hitCount; i++)
        {
            var col = connectorBuffer[i];
            if (col == null) continue;

            Transform connector = col.transform;

            if (!IsConnectableConnector(connector)) continue;

            float sqr = (connector.position - myPos).sqrMagnitude;
            if (sqr >= bestSqr) continue;

            if (HasEnoughPlace(module, connector))
            {
                bestSqr = sqr;
                best = connector;
            }
        }

        return best;
    }

    public void RequestAttach(BaseModule module)
    {
        if (module == null) return;

        var cand = QueryCandidate(module);
        if (cand != null && !HasEnoughPlace(module, cand))
            cand = null;

        if (cand != null) module.CommitAttach(cand);
        else module.FallbackDetachState();
    }

    private bool IsConnectableConnector(Transform connector)
    {
        if (connector != null && connector.parent != null && connector.parent.TryGetComponent(out Module m))
            return m.Connectable;
        return false;
    }

    private bool HasEnoughPlace(BaseModule module, Transform closestConnector)
    {
        var selfCols = module.SelfColliders;
        if (selfCols == null || selfCols.Count == 0) return false;

        Transform anchor = module.SenderTransform != null ? module.SenderTransform : module.transform;

        foreach (var c in selfCols)
        {
            if (c == null) continue;

            if (c is BoxCollider2D box)
            {
                Vector2 boxCenter = (Vector2)closestConnector.position
                    - (Vector2)closestConnector.up * (anchor.localPosition.y - box.offset.y);

                Vector2 boxSize = sizeOffset * box.size;
                float boxAngle = closestConnector.eulerAngles.z;

                int count = Physics2D.OverlapBox(boxCenter, boxSize, boxAngle, moduleFilter, overlapBuffer);
                if (HasBlockingCollider(module, count)) return false;
            }
            else if (c is CircleCollider2D circle)
            {
                Vector2 cCenter = (Vector2)closestConnector.position
                    - (Vector2)closestConnector.up * (anchor.localPosition.y - circle.offset.y);

                float radius = sizeOffset * circle.radius;

                int count = Physics2D.OverlapCircle(cCenter, radius, moduleFilter, overlapBuffer);
                if (HasBlockingCollider(module, count)) return false;
            }
            else
            {
                return false;
            }
        }

        return true;
    }

    private bool HasBlockingCollider(BaseModule module, int count)
    {
        // 버퍼가 꽉 찼으면 보수적으로 실패 처리
        if (count >= overlapBuffer.Length) return true;

        var self = module.SelfColliders;

        for (int i = 0; i < count; i++)
        {
            var c = overlapBuffer[i];
            if (c == null) continue;
            if (self.Contains(c)) continue;
            return true;
        }
        return false;
    }
}
