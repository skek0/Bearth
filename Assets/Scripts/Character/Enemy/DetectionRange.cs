using UnityEngine;

public class DetectionRange : MonoBehaviour
{
    [SerializeField] float detectingRange = 10f;
    public float DetectingRange { get { return detectingRange; } set { detectingRange = value; } }
    private CircleCollider2D detectionCollider;

    private void Awake()
    {
        CreateCollider();
    }

    private void CreateCollider()
    {
        // 이미 있을 수도 있으니까 중복 방지
        detectionCollider = gameObject.GetComponent<CircleCollider2D>();
        if (detectionCollider == null)
            detectionCollider = gameObject.AddComponent<CircleCollider2D>();

        detectionCollider.isTrigger = true;
        detectionCollider.radius = detectingRange;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.attachedRigidbody;

        if (rb != null && rb.CompareTag("Player"))
        {
            Debug.Log("Player Detected");
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        Rigidbody2D rb = collision.attachedRigidbody;

        if (rb != null && rb.CompareTag("Player"))
        {
            Debug.Log("Player Missed");
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, detectingRange);
    }
}
