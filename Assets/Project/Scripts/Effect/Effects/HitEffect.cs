using UnityEngine;

public class HitEffect : MonoBehaviour
{
    [SerializeField] private ParticleSystem ps;

    private void Awake()
    {
        if(ps == null) ps = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        if (!ps.IsAlive())
        {
            ObjectPoolManager.Instance.ReturnObject(gameObject);
        }
    }
    public void SetInfo(HitEffectInfo hitEffectInfo)
    {
        transform.SetPositionAndRotation(hitEffectInfo.Position, Quaternion.FromToRotation(Vector3.up, hitEffectInfo.Direction));
    }

    public void Begin()
    {
        ps.Clear();
        ps.Play();
    }
}
