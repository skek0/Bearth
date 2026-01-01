using System;
using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float lifetime = 2f; // 몇 초 후 반환할지
    public GameObject trailEmitterPrefab;
    public GameObject hitEffectPrefab;
    [SerializeField] LayerMask hitMask;
    float radius; // 총알 반지름(콜라이더 크기랑 맞추기)

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

        Vector2 step = (Vector2)transform.up * (speed * Time.deltaTime);
        Vector2 nextPos = (Vector2)transform.position + step;

        // 스윕(프레임 사이 통과 방지)
        float dist = step.magnitude;
        if (dist > 0f)
        {
            RaycastHit2D hit = Physics2D.CircleCast(prevPos, radius, step.normalized, dist, hitMask);
            if (hit.collider != null)
            {
                // 충돌 지점으로 스냅
                transform.position = hit.point;

                if (hit.collider.TryGetComponent(out IDamageable damageable))
                {
                    damageable.ApplyDamage(new DamageData { Amount = damage });

                    if (trailEmitter != null)
                        trailEmitter.AssignLastPos(transform.position);

                    Vector2 dir = (prevPos - hit.point).normalized;
                    HitEffectManager.Instance.SpawnHitEffect(new HitEffectInfo { Direction = dir, Position = hit.point });
                    FCTManager.Instance.SpawnFCT(new FCTInfo { Position = hit.point, Amount = damage });

                    ReturnBullet();
                    return;
                }
            }
        }

        transform.position = nextPos;
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
    private void Awake()
    {
        radius = GetComponent<CircleCollider2D>().radius;
    }
}
