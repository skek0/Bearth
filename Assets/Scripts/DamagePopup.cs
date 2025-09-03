using UnityEngine;
public struct DamageInfo
{
    public Vector3 Position;
    public int Amount;
    public DamageType Type;
}
public class DamagePopup : MonoBehaviour
{
    [SerializeField] GameObject damageSkinPrefab;

    [SerializeField]ObjectPoolManager poolManager;

    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;
    }
    void OnEnable() => EventBus.Subscribe<DamageInfo>(OnDamageTaken);
    void OnDisable() => EventBus.Unsubscribe<DamageInfo>(OnDamageTaken);

    void OnDamageTaken(DamageInfo damageTaken)
    {
        // 타겟 기준으로 떠있는 데미지 숫자 출력, 히트 사운드 등
        if (damageTaken.Position == null) return;
        GameObject _object = poolManager.GetObject(damageSkinPrefab, false);
        if(_object != null && _object.TryGetComponent(out DamageSkin popup))
        {
            popup.SetInfo(damageTaken);
            popup.gameObject.SetActive(true);
        }
        // 예: FloatingText.Spawn(damageTaken.Target.transform.position, damageTaken.Amount);
    }
}
