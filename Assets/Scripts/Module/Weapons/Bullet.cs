using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2f; // 몇 초 후 반환할지

    int damage;
    float speed;

    TrailRenderer trailRenderer; 
    private Coroutine returnRoutine;

    public void SetBulletInfo(int damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
    }

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.down * speed);
    }

    private void OnEnable()
    {
        trailRenderer.emitting = true;
        // 활성화되면 자동으로 반환 타이머 시작
        StartCoroutine(AutoReturn());
    }
    private void OnDisable()
    {
        // 반환되었을 때 코루틴 정리
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
        trailRenderer.emitting = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            Debug.Log($"Crushed to {collision.name}");
            DamageData damage = new DamageData
            {
                Amount = this.damage
            };

            damageable.TakeDamage(damage);
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
    }
    

    private System.Collections.IEnumerator AutoReturn()
    {
        yield return new WaitForSeconds(lifetime);
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
