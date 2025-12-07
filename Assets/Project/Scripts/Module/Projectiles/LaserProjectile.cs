using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 굵은 레이저: 폭을 따라 N개 샘플 레이를 쏘고,
/// 샘플별 막힌 길이로 "들쑥한 끝선"을 만든 뒤 메쉬 스트립(2N 버텍스)로 그린다.
/// 물리는 RaycastNonAlloc(N회/프레임), 렌더는 메쉬 1드로우.
/// DPS는 "해당 프레임에 맞은 샘플 비율(m/N)"로 분배하여 누적-틱 단위로 반영.
/// </summary>
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class LaserProjectile : MonoBehaviour, ILaserProjectile
{
    [Header("Beam (Visual/PHYSICS)")]
    [Min(0.1f)] public float range = 12f;          // 레이저 사거리
    [Min(0.01f)] public float width = 0.6f;         // 레이저 굵기(월드 단위)
    [Range(1f, 60f)] public float samplesPerUnit = 12f;    // 폭 1유닛 당 샘플 수(정밀도)
    [Range(3, 63)] public int maxSamples = 21;      // 샘플 상한(성능/정밀도 트레이드오프)
    public LayerMask hitMask;

    [Header("Damage")]
    [Tooltip("데미지 적용 주기(초). 0이면 매 프레임 반영")]
    [Range(0f, 0.2f)] public float tick = 0.02f;

    [Header("Rendering")]
    public Material beamMaterial;                   // Additive/Glow 계열 권장
    [Tooltip("UV를 세로로 스크롤(0이면 고정)")]
    public float uvScrollSpeedV = 3f;

    // 런타임 파라미터(ILaserProjectile에서 주입)
    Transform firePoint;
    float dps;            // 초당 데미지
    float duration;       // 유지 시간
    float timer, tickTimer;

    // 샘플링/누적
    int N;                                      // 실제 샘플 수
    Vector3[] sampleEnds;                       // 샘플 끝점(월드)
    readonly Dictionary<Collider2D, float> accum = new(); // 타겟별 누적 데미지(소수 포함)

    // 비주얼: 메쉬 스트립(2N 버텍스: [start(i), end(i)] 쌍)
    MeshFilter mf; MeshRenderer mr; Mesh mesh;
    Vector3[] verts; Vector2[] uvs; int[] indices;

    // 물리 버퍼(할당 줄이기)
    readonly RaycastHit2D[] hitsBuffer = new RaycastHit2D[32];

    Coroutine runCoro;

    // ---- ILaserProjectile ----
    public void SetInfo(float dps, float duration, Transform firePoint)
    {
        this.dps = dps;
        this.duration = Mathf.Max(0f, duration);
        this.firePoint = firePoint;

        if (mf == null) mf = GetComponent<MeshFilter>();
        if (mr == null) mr = GetComponent<MeshRenderer>();
        if (mesh == null) { mesh = new Mesh { name = "LaserStripMesh", indexFormat = UnityEngine.Rendering.IndexFormat.UInt32 }; }
        mf.sharedMesh = mesh;
        if (beamMaterial != null) mr.sharedMaterial = beamMaterial;

        // 샘플 수 결정 및 버퍼 준비
        N = Mathf.Clamp(Mathf.CeilToInt(width * samplesPerUnit), 2, maxSamples);
        sampleEnds = sampleEnds != null && sampleEnds.Length == N ? sampleEnds : new Vector3[N];

        BuildStripBuffersIfNeeded(N);  // 인덱스/UV/버텍스 배열 준비

        // 상태 초기화
        timer = tickTimer = 0f;
        accum.Clear();

        if (runCoro != null) { StopCoroutine(runCoro); }
        runCoro = StartCoroutine(Run());
    }

    void OnDisable()
    {
        if (runCoro != null) { StopCoroutine(runCoro); runCoro = null; }
        accum.Clear();
        if (mesh != null) mesh.Clear();
    }

    IEnumerator Run()
    {
        while (timer < duration)
        {
            if (!firePoint) break;

            // 기본 벡터들
            Vector2 origin = firePoint.position;
            Vector2 dir = firePoint.up.normalized;       // 필요 시 up으로 변경
            Vector2 nrm = new Vector2(-dir.y, dir.x);       // 진행에 수직
            float halfW = width * 0.5f;

            // 샘플링 결과: 콜라이더별 기여도(해당 프레임 m/N 합산)
            var perColliderWeight = DictionaryPool<Collider2D, float>.Get(); // 임시 딕셔너리(가비지 절약용)

            // 1) 멀티 샘플 Raycast
            for (int i = 0; i < N; i++)
            {
                // 폭 방향 오프셋
                float t = (i + 0.5f) / N;
                float off = Mathf.Lerp(-halfW, halfW, t);
                Vector2 o = origin + nrm * off;

                // 가까운 히트 찾기
                int count = Physics2D.RaycastNonAlloc(o, dir, hitsBuffer, range, hitMask);
                float endDist = range; 
                Collider2D hitCollider = null; 
                Vector2 hitPoint = default;

                if (count > 0)
                {
                    float min = float.MaxValue;
                    for (int h = 0; h < count; h++)
                    {
                        var hit = hitsBuffer[h];
                        if (hit.collider == null) continue;
                        if (hit.distance < min)
                        {
                            min = hit.distance;
                            hitCollider = hit.collider;
                            hitPoint = hit.point;
                        }
                    }
                    endDist = (hitCollider != null) ? min : range;
                }

                // 샘플 끝점
                Vector2 end = o + dir * endDist;
                sampleEnds[i] = end;

                // 데미지 기여도(샘플 1개 = 1/N)
                if (hitCollider != null)
                {
                    if (perColliderWeight.TryGetValue(hitCollider, out float w)) perColliderWeight[hitCollider] = w + (1f / N);
                    else perColliderWeight[hitCollider] = (1f / N);
                }

                // 버텍스 두 개: start(i), end(i) 갱신용 월드 좌표 저장은 아래에서 일괄 처리
            }

            // 2) 데미지 누적 (DPS * dt * 비율)
            float dt = Time.deltaTime;
            foreach (var kv in perColliderWeight)
            {
                float add = dps * dt * kv.Value;
                if (accum.TryGetValue(kv.Key, out float cur)) accum[kv.Key] = cur + add;
                else accum[kv.Key] = add;
            }
            DictionaryPool<Collider2D, float>.Release(perColliderWeight);

            // 3) tick에 맞춰 실제 데미지 적용
            bool doTick = tick <= 0f;
            if (!doTick)
            {
                tickTimer += dt;
                if (tickTimer >= tick) { tickTimer = 0f; doTick = true; }
            }
            if (doTick && accum.Count > 0) FlushDamage();

            // 4) 메쉬 버텍스 갱신(2N)
            UpdateStripVertices(origin, nrm, sampleEnds);

            timer += dt;
            yield return null;
        }

        if (accum.Count > 0) FlushDamage();

        // 풀 반납 or 비활성화
        ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    // ---- Damage helpers ----
    void FlushDamage()
    {
        // 정수로 내리고 소수는 보존
        var keys = ListPool<Collider2D>.Get();
        keys.AddRange(accum.Keys);
        foreach (var c in keys)
        {
            float f = accum[c];
            int dmg = Mathf.FloorToInt(f);
            if (dmg > 0)
            {
                accum[c] = f - dmg;
                c.GetComponentInParent<IDamageable>()?.TakeDamage(new DamageData
                {
                    Amount = dmg,
                    Type = DamageType.ENERGY
                });

                // 2) FCT는 "실제 데미지" 기준으로 1번 출력
                var pos = (Vector2)c.bounds.center; // 혹은 콜라이더 월드 중심
                FCTManager.Instance.SpawnFCT(new FCTInfo
                {
                    Position = pos,
                    Amount = dmg
                });
            }
        }
        ListPool<Collider2D>.Release(keys);
    }

    // ---- Mesh strip ----
    void BuildStripBuffersIfNeeded(int N)
    {
        // 버텍스: 2N (i마다 start, end)
        int vCount = 2 * N;
        if (verts == null || verts.Length != vCount) verts = new Vector3[vCount];
        if (uvs == null || uvs.Length != vCount) uvs = new Vector2[vCount];

        // 인덱스: (N-1) 구간 × 2삼각형 × 3 = 6(N-1)
        int iCount = (N - 1) * 6;
        if (indices == null || indices.Length != iCount)
        {
            indices = new int[iCount];
            int k = 0;
            for (int i = 0; i < N - 1; i++)
            {
                // 현재 쌍(2i, 2i+1), 다음 쌍(2i+2, 2i+3)
                int a0 = 2 * i;
                int a1 = 2 * i + 1;
                int b0 = 2 * (i + 1);
                int b1 = 2 * (i + 1) + 1;

                // quad = (a0,a1,b1) + (a0,b1,b0)  (세로 스트립)
                indices[k++] = a0; indices[k++] = a1; indices[k++] = b1;
                indices[k++] = a0; indices[k++] = b1; indices[k++] = b0;
            }
        }

        // 초기 UV: u는 폭 방향(0~1), v는 0(start) / 1(end)
        for (int i = 0; i < N; i++)
        {
            float u = (N == 1) ? 0f : (float)i / (N - 1);
            uvs[2 * i] = new Vector2(u, 0f); // start
            uvs[2 * i + 1] = new Vector2(u, 1f); // end
        }

        mesh.Clear();
        mesh.vertices = verts;      // 좌표는 매 프레임 UpdateStripVertices에서 갱신
        mesh.uv = uvs;
        mesh.triangles = indices;
        mesh.RecalculateBounds();
    }

    void UpdateStripVertices(Vector2 origin, Vector2 nrm, Vector3[] ends)
    {
        // (선택) UV 스크롤
        if (uvScrollSpeedV != 0f && mr != null)
        {
            // 머티리얼 인스턴스에 적용 (공유 머티리얼 오염 방지)
            var mat = mr.material;
            var off = mat.mainTextureOffset;
            off.y += uvScrollSpeedV * Time.deltaTime;
            mat.mainTextureOffset = off;
        }

        float halfW = width * 0.5f;
        var tp = transform; // 로컬 변환용

        for (int i = 0; i < N; i++)
        {
            float t = (i + 0.5f) / N;
            float off = Mathf.Lerp(-halfW, halfW, t);
            Vector3 start = origin + nrm * off;   // 시작선(총구 쪽)
            Vector3 end = ends[i];                // 샘플 끝선

            // ★ 로컬 좌표로 변환해서 메쉬에 넣기
            int a = 2 * i;
            verts[a] = tp.InverseTransformPoint(start);
            verts[a + 1] = tp.InverseTransformPoint(end);
        }

        mesh.vertices = verts;
        mesh.RecalculateBounds(); // 필요 시만 호출(카메라 밖에서도 커지면 생략 금지)
        // 노멀/탱젠트 불필요. 셰이더가 필요하면 적절히 추가 생성
    }

    // ---- Pools (가비지 줄이기용 간단 풀) ----
    static class DictionaryPool<K, V>
    {
        static readonly Stack<Dictionary<K, V>> pool = new();
        public static Dictionary<K, V> Get() => pool.Count > 0 ? pool.Pop() : new Dictionary<K, V>(8);
        public static void Release(Dictionary<K, V> d) { d.Clear(); pool.Push(d); }
    }
    static class ListPool<T>
    {
        static readonly Stack<List<T>> pool = new();
        public static List<T> Get() => pool.Count > 0 ? pool.Pop() : new List<T>(8);
        public static void Release(List<T> l) { l.Clear(); pool.Push(l); }
    }
}