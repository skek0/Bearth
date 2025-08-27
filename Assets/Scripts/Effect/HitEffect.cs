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

    public void Begin(Vector2 pos)
    {
        ps.Clear();
        ps.Play();
    }
}
