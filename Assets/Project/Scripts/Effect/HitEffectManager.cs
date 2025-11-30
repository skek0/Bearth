using UnityEngine;
public struct HitEffectInfo
{
    public Vector2 Position;
    public Vector2 Direction;
    public bool isCritical;

}
public class HitEffectManager : S_Singleton<HitEffectManager>
{
    public GameObject hitEffectPrefab;
    
    ObjectPoolManager poolManager;
    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;
    }

    public void SpawnHitEffect(HitEffectInfo hitEffectInfo)
    {
        if (hitEffectInfo.Position == null){ Debug.Log("Assign hitEffect Position"); return; }

        GameObject _object = poolManager.GetObject(hitEffectPrefab, false);
        if (_object != null && _object.TryGetComponent(out HitEffect hitEffect))
        {
            hitEffect.SetInfo(hitEffectInfo);
            hitEffect.gameObject.SetActive(true);
            hitEffect.Begin();
        }

    }
}
