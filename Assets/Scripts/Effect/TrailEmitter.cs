using UnityEngine;

[DisallowMultipleComponent]
public class TrailEmitter : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    private Transform followTarget;
    private bool following;

    void Awake()
    {
        if (ps == null) ps = GetComponent<ParticleSystem>();
        Debug.Assert(ps != null, "[TrailEmitter] ParticleSystem missing"); 
        
        var main = ps.main;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startSpeed = 0f;
        main.playOnAwake = false;            

        var em = ps.emission;
        em.rateOverTime = 0f;                // 시간기준 방출 끔 (기본)
    }

    /// 총알과 바인딩하여 따라다니며 궤적 생성 시작
    public void Begin(Transform target)
    {
        followTarget = target;
        following = true;

        ps.Clear();
        ps.Play();
    }
    public void AssignLastPos(Vector3 pos)
    {
        transform.position = pos;
        ps.Play(true);
        var lastParticle = new ParticleSystem.EmitParams
        {
            position = pos,
            velocity = Vector2.zero,
            startLifetime = ps.main.startLifetime.constant,
            startSize = ps.main.startSize.constant,
        };
        ps.Emit(lastParticle, 1);
        ps.Stop(true, ParticleSystemStopBehavior.StopEmitting);
    }

    public void Fade()
    {
        following = false;
        if (ps.isPlaying)
            ps.Stop();
    }

    void Update()
    {
        if (following && followTarget)
            transform.SetPositionAndRotation(followTarget.position, followTarget.rotation);

        // 분리되고 모든 입자가 사라지면 풀로 복귀
        if (!following && !ps.IsAlive())
            ObjectPoolManager.Instance.ReturnObject(gameObject);
    }

    void OnDisable()
    {
        // 다음 재사용을 위한 초기화
        followTarget = null;
        following = false;
        if (ps) ps.Clear();
    }

}
