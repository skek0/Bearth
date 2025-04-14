using System.Collections;
using UnityEngine;

public class Bullet : MonoBehaviour
{
    public int damage;
    public float speed;
    public float sight;

    TrailRenderer trailRenderer;

    private void Awake()
    {
        trailRenderer = GetComponent<TrailRenderer>();
    }

    private void FixedUpdate()
    {
        transform.Translate(Vector2.right * speed);
    }

    private void OnEnable()
    {
        TrailEffect(true);
    }
    private void OnDisable()
    {
        TrailEffect(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.TryGetComponent(out IDamageable damageable))
        {
            Debug.Log($"Crushed to {collision.name}");
            damageable.GetDamage(damage);
            ObjectPoolManager.Instance.ReturnObject("Bullet", gameObject);
        }
    }


    void TrailEffect(bool enabled)
    {
        if(enabled)
        {
            StartCoroutine(IsMaybeinSight());
        }
        else
        {
            trailRenderer.Clear();
        }
    }

    public void SetSight(float _sight)
    {
        sight = _sight;
    }
    IEnumerator IsMaybeinSight()
    {
        yield return CoroutineCache.WaitforSeconds(sight / (speed*2));
        if(gameObject.activeInHierarchy)
        {
            ObjectPoolManager.Instance.ReturnObject("Bullet", gameObject);
        }
    }
}
