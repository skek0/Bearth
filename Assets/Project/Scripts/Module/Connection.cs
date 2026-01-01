using UnityEngine;
using System.Collections.Generic;
using UnityEditor.Experimental.GraphView;
using System;
using Unity.VisualScripting;
using System.Linq;

public class Connection : MonoBehaviour
{
    public Transform ClosestConnector { get; private set; }
    [SerializeField] float sizeOffset = 0.97f; // 부착될 크기 조절 계수, 0.9

    HashSet<Collider2D> moduleColliders;
    Transform anchor;
    List<GameObject> nearConnectors = new();
    LayerMask layermask;

    Vector2 circleCenter;
    float circleRadius;

    [SerializeField] int bufferSize = 64;
    private static Collider2D[] overlapBuffer;
    private static int overlapBuffersize;
    private ContactFilter2D filter;

    private void Awake()
    {
        layermask = LayerMask.GetMask("Module"); 
        
        filter = new ContactFilter2D();
        filter.useLayerMask = true;
        filter.SetLayerMask(layermask);
        filter.useTriggers = false; // Module 레이어에 Trigger가 있으면 제외(원하면 true로)

        EnsureSharedBuffer(bufferSize);
    }
    private static void EnsureSharedBuffer(int requestedSize)
    {
        if (requestedSize <= 0) requestedSize = 32;
        if (overlapBuffer != null && overlapBuffersize >= requestedSize) return;

        overlapBuffersize = requestedSize;
        overlapBuffer = new Collider2D[overlapBuffersize];
    }
    public void SetColliderAndAnchor(Collider2D[] col, Transform anchorTransform)
    {
        moduleColliders = new HashSet<Collider2D>(col);
        anchor = anchorTransform;
    }

    void Update()
    {
        Transform closestConnector = null;
        float minSqrDistance = float.MaxValue;
        Vector3 myPos = transform.position;

        foreach (var connector in nearConnectors)
        {
            Vector3 targetPos = connector.transform.position;
            float sqrDist = (targetPos - myPos).sqrMagnitude;

            if (sqrDist < minSqrDistance)
            {
                minSqrDistance = sqrDist;
                closestConnector = connector.transform;
            }
        }
        if(closestConnector != null && HasEnoughPlace(closestConnector))
        {
            ClosestConnector = closestConnector;
        }
        else ClosestConnector = null;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        GameObject collidedObject = collision.gameObject;
        if(collidedObject.layer == LayerMask.NameToLayer("Connector") && IsConnectableConnector(collidedObject))
        {
            if (!nearConnectors.Contains(collidedObject))
                nearConnectors.Add(collidedObject);
        }
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        GameObject exitingObject = collision.gameObject;
        if (exitingObject.layer == LayerMask.NameToLayer("Connector"))
        {
            if (nearConnectors.Contains(exitingObject))
                nearConnectors.Remove(exitingObject);
        }
    }

    bool IsConnectableConnector(GameObject connector)
    {
        connector.transform.parent.TryGetComponent(out Module module);
        if(module != null)
        {
            return module.Connectable;
        }
        return false;
    }

    Vector2 center;
    Vector2 size;
    float angle;
    private bool HasEnoughPlace(Transform closestConnector)
    {
        EnsureSharedBuffer(bufferSize);

        foreach (var collider in moduleColliders)
        {
            if (collider is BoxCollider2D boxCollider)
            {
                Vector2 boxCenter = closestConnector.position
                    - closestConnector.up * (anchor.localPosition.y - boxCollider.offset.y);

                Vector2 boxSize = sizeOffset * new Vector2(boxCollider.size.x, boxCollider.size.y);
                float boxAngle = closestConnector.eulerAngles.z;

                // 디버그 표시용
                center = boxCenter;
                size = boxSize;
                angle = boxAngle;

                int count = Physics2D.OverlapBox(boxCenter, boxSize, boxAngle, filter, overlapBuffer);
                if (HasBlockingCollider(count)) return false;
            }
            else if (collider is CircleCollider2D circleCollider)
            {
                Vector2 cCenter = closestConnector.position
                    - closestConnector.up * (anchor.localPosition.y - circleCollider.offset.y);

                float radius = sizeOffset * circleCollider.radius;

                // 디버그용: 원 그리기 원하면 필드에 저장해 두고 OnDrawGizmos에서 그려
                circleCenter = cCenter;
                circleRadius = radius;

                int count = Physics2D.OverlapCircle(cCenter, radius, filter, overlapBuffer); // ✅ results 오버로드
                if (HasBlockingCollider(count)) return false;
            }
        }

        return true;
    }
    private bool HasBlockingCollider(int count)
    {
        // 배열이 꽉 찼으면 더 있었을 수도 있음 -> 보수적으로 막기
        if (count >= overlapBuffersize) return true;

        for (int i = 0; i < count; i++)
        {
            Collider2D c = overlapBuffer[i];
            if (c == null) continue;

            if (!IsSelfCollider(c))
                return true;
        }
        return false;
    }
    private bool IsSelfCollider(Collider2D other)
    {
        if (other == null) return false;

        // 내가 가진 콜라이더 중 하나인가?
        if (moduleColliders != null && moduleColliders.Contains(other))
            return true;

        return false;
    }
    private void OnDrawGizmos()
    {
        if (moduleColliders == null) return;

        Gizmos.color = Color.green;

        foreach (var collider in moduleColliders)
        {
            if (collider is BoxCollider2D)
            {
                DrawWireBox2D(center, size, angle);
            }
            else if (collider is CircleCollider2D)
            {
                DrawWireCircle2D(circleCenter, circleRadius, 32); // 분할 수는 취향껏                
            }
        }
    }

    // 2D 회전 박스 그리기
    private void DrawWireBox2D(Vector2 center, Vector2 size, float angle)
    {
        Vector2 half = size * 0.5f;

        // 로컬 좌표의 꼭짓점
        Vector2[] corners = new Vector2[4];
        corners[0] = new Vector2(-half.x, -half.y);
        corners[1] = new Vector2(-half.x, half.y);
        corners[2] = new Vector2(half.x, half.y);
        corners[3] = new Vector2(half.x, -half.y);

        // 회전 변환
        float rad = angle * Mathf.Deg2Rad;
        float cos = Mathf.Cos(rad);
        float sin = Mathf.Sin(rad);

        for (int i = 0; i < corners.Length; i++)
        {
            float x = corners[i].x;
            float y = corners[i].y;
            corners[i] = new Vector2(
                cos * x - sin * y,
                sin * x + cos * y
            ) + center;
        }

        // 네 변 그리기
        for (int i = 0; i < 4; i++)
        {
            Gizmos.DrawLine(corners[i], corners[(i + 1) % 4]);
        }
    }
    private void DrawWireCircle2D(Vector2 center, float radius, int segments = 32)
    {
        if (segments < 3) segments = 3;

        float step = Mathf.PI * 2f / segments;
        Vector2 prev = center + new Vector2(radius, 0f);

        for (int i = 1; i <= segments; i++)
        {
            float a = step * i;
            Vector2 curr = center + new Vector2(Mathf.Cos(a) * radius, Mathf.Sin(a) * radius);
            Gizmos.DrawLine(prev, curr);
            prev = curr;
        }
    }
}
