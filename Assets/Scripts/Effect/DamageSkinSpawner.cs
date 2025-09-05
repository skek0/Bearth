using UnityEngine;
public struct DamageInfo
{
    public Vector2 Position;
    public int Amount;
    public DamageType Type;
}
public class DamageSkinSpawner : MonoBehaviour
{
    [SerializeField] GameObject damageSkinPrefab;
    
    ObjectPoolManager poolManager;

    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;
    }
    void OnEnable() => EventBus.Subscribe<DamageInfo>(OnDamageTaken);
    void OnDisable() => EventBus.Unsubscribe<DamageInfo>(OnDamageTaken);

    void OnDamageTaken(DamageInfo damageTaken)
    {
        if (damageTaken.Position == null) return;
        GameObject _object = poolManager.GetObject(damageSkinPrefab, false);
        if(_object != null && _object.TryGetComponent(out DamageSkin popup))
        {
            popup.SetInfo(damageTaken);
            popup.gameObject.SetActive(true);
        }
    }
}
