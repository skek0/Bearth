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

    Coroutine returnRoutine;
    TrailEmitter trailEmitter;  // 충돌 후 이펙트 삭제되는 문제로 궤적을 따로 둠
    bool returned;
    Vector2 prevPos;

    string ownerGuid = "";

    public void SetBulletInfo(int damage, float speed, string guid)
    {
        this.damage = damage;
        this.speed = speed;
        this.ownerGuid = guid;
    }

    private void Update()
    {
        ProceedBullet();
    }

    private void ProceedBullet()
    {
        prevPos = transform.position;

        Vector2 step = (Vector2)transform.up * (speed * Time.deltaTime);
        float dist = step.magnitude;
        if (dist <= 0f)
            return;

        RaycastHit2D hit = Physics2D.CircleCast(prevPos, radius, step.normalized, dist, hitMask);
        if (hit.collider == null)
        {
            transform.position = (Vector2)transform.position + step;
            return;
        }

        if (ShouldIgnoreHit(hit.collider))
        {
            transform.position = (Vector2)transform.position + step;
            return;
        }

        if (!hit.collider.TryGetComponent(out IDamageable damageable))
        {
            transform.position = (Vector2)transform.position + step;
            return;
        }

        transform.position = hit.point;
        HandleDamageHit(damageable, hit.point, prevPos);
    }

    private bool ShouldIgnoreHit(Collider2D col)
    {
        if (string.IsNullOrEmpty(ownerGuid))
            return false;

        // ModuleGuid를 직접 찍는게 가장 안전/가벼움 (Module GetComponent NRE 방지)
        if (col.TryGetComponent(out ModuleGuid mg) && mg.Guid == ownerGuid)
            return true;


        return false;
    }

    private void HandleDamageHit(IDamageable damageable, Vector2 hitPoint, Vector2 fromPos)
    {
        // 데미지
        damageable.ApplyDamage(new DamageData { Amount = damage });

        // 궤적 마감
        if (trailEmitter != null)
            trailEmitter.AssignLastPos(hitPoint);

        // 이펙트/FCT
        SpawnHitFx(hitPoint, fromPos);

        // 반환
        ReturnBullet();
    }

    private void SpawnHitFx(Vector2 hitPoint, Vector2 fromPos)
    {
        HitEffectManager.Instance.SpawnHitEffect(
            new HitEffectInfo
            {
                Direction = (fromPos - hitPoint).normalized,
                Position = hitPoint
            });

        FCTManager.Instance.SpawnFCT(
            new FCTInfo
            {
                Position = hitPoint,
                Amount = damage
            });
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
