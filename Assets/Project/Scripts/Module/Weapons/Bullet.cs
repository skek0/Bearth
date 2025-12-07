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


    private void Update()
    {
        prevPos = transform.position;
        transform.Translate(speed * Time.deltaTime * Vector2.up);
    }

    private void OnEnable()
    {
        returned = false;

        if (trailEmitterPrefab)
        {
            var emitter = ObjectPoolManager.Instance.GetObject(trailEmitterPrefab,false);
            emitter.transform.position = transform.position;
            emitter.SetActive(true);
            trailEmitter = emitter.GetComponent<TrailEmitter>();
            trailEmitter.Begin(transform);
        }

        // 활성화되면 자동으로 반환 타이머 시작
        returnRoutine = StartCoroutine(AutoReturn());
    }
    private void OnDisable()
    {
        // 반환되었을 때 코루틴 정리
        if (returnRoutine != null)
        {
            StopCoroutine(returnRoutine);
            returnRoutine = null;
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            damageable.TakeDamage(new DamageData { Amount = damage });

            if (trailEmitter != null)
            {
                trailEmitter.AssignLastPos(transform.position);
            }

            Vector2 closestPoint = collision.ClosestPoint(prevPos);
            Vector2 dir = (prevPos - closestPoint).normalized;

            HitEffectManager.Instance.SpawnHitEffect(new HitEffectInfo { Direction = dir, Position = closestPoint });
            FCTManager.Instance.SpawnFCT(new FCTInfo { Position = closestPoint, Amount = damage });

            ReturnBullet();
        }
    }
    private IEnumerator AutoReturn()
    {
        yield return CoroutineCache.WaitforSeconds(lifetime);
        ReturnBullet();
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
