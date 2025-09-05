using UnityEngine;
public struct HitEffectInfo
{
    public Vector2 Position;
    public Vector2 Direction;
    public bool isCritical;

    //public HitEffectInfo(Vector2 position, Vector2 direction)
    //{
    //    Position = position;
    //    Direction = direction;
    //}
}
public class HitEffectSpawner : MonoBehaviour
{
    public GameObject hitEffectPrefab;
    
    ObjectPoolManager poolManager;
    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;
    }
    private void OnEnable() => EventBus.Subscribe<HitEffectInfo>(OnHitOccured);
    private void OnDisable() => EventBus.Unsubscribe<HitEffectInfo>(OnHitOccured);

    void OnHitOccured(HitEffectInfo hitEffectInfo)
    {
        if (hitEffectInfo.Position == null) Debug.LogError("Assign hitEffect Position");

        GameObject _object = poolManager.GetObject(hitEffectPrefab, false);
        if (_object != null && _object.TryGetComponent(out HitEffect hitEffect))
        {
            hitEffect.SetInfo(hitEffectInfo);
            hitEffect.gameObject.SetActive(true);
            hitEffect.Begin();
        }

    }
}
