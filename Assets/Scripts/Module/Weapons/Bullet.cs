using System;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2f; // 몇 초 후 반환할지
    public GameObject trailEmitterPrefab;
    public GameObject hitEffectPrefab;

    int damage;
    float speed;

    private Coroutine returnRoutine;
    TrailEmitter trailEmitter;  // 충돌 후 이펙트 삭제되는 문제로 궤적을 따로 둠
    bool returned;
    Vector2 prevPos;

    public void SetBulletInfo(int damage, float speed)
    {
        this.damage = damage;
        this.speed = speed;
    }


    private void FixedUpdate()
    {
        prevPos = transform.position;
        transform.Translate(speed * Time.fixedDeltaTime * Vector2.down);
    }

    private void OnEnable()
    {
        returned = false;

        if (trailEmitterPrefab)
        {
            var emitter = ObjectPoolManager.Instance.GetObject(trailEmitterPrefab);
            trailEmitter = emitter.GetComponent<TrailEmitter>();
            trailEmitter.transform.position = transform.position;
            trailEmitter.Begin(transform);
        }

        // 활성화되면 자동으로 반환 타이머 시작
        returnRoutine = StartCoroutine(AutoReturn());
    }
    private void OnDisable()
    {
        Debug.Log(name);
        // 반환되었을 때 코루틴 정리
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
        // 외부에서 비활성화되더라도 궤적은 자연 소멸로 마무리
        ReturnBullet();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            Vector3 closestPoint = collision.ClosestPoint(prevPos);
            Debug.Log($"Crushed to {collision.name}");
            DamageData damage = new DamageData
            {
                Amount = this.damage
            };
            damageable.TakeDamage(damage);

            if (trailEmitter != null) 
            {
                trailEmitter.AssignLastPos(transform.position);
            }
            CallHitEffect(closestPoint);

            EventBus.Publish(new DamageInfo
            {
                Position = closestPoint,
                Amount = this.damage
            });

            ObjectPoolManager.Instance.ReturnObject(gameObject);

            ReturnBullet();
        }
    }


    private void CallHitEffect(Vector3 hitPos)
    {
        var hitEffect = ObjectPoolManager.Instance.GetObject(hitEffectPrefab,false);

        Vector2 dir = (prevPos - (Vector2)hitPos).normalized;
        hitEffect.transform.SetPositionAndRotation(hitPos, Quaternion.FromToRotation(Vector3.up, dir));
        hitEffect.SetActive(true);
        hitEffect.GetComponent<ParticleSystem>().Play();
    }
    private IEnumerator AutoReturn()
    {
        yield return CoroutineCache.WaitforSeconds(lifetime);
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    void ReturnBullet()
    {
        if (returned) return;
        returned = true;

        // 1) 궤적은 자연 소멸
        if (trailEmitter != null)
        {
            trailEmitter.Fade();
            trailEmitter = null;
        }
        // 2) 총알은 즉시 풀 복귀
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }
}
