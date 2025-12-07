using UnityEngine;
public struct FCTInfo
{
    public Vector2 Position;
    public int Amount;
    public DamageType Type;
}

/// <summary> Floating Combat Text Manager </summary>
public class FCTManager : S_Singleton<FCTManager>
{
    [SerializeField] GameObject damageSkinPrefab;
    
    ObjectPoolManager poolManager;

    private void Start()
    {
        poolManager = ObjectPoolManager.Instance;
    }

    public void SpawnFCT(FCTInfo damageTaken)
    {
        //if(데미지 표시 옵션)
        if (damageTaken.Position == null) return;
        GameObject _object = poolManager.GetObject(damageSkinPrefab, false);
        if(_object != null && _object.TryGetComponent(out DamageSkin popup))
        {
            popup.SetInfo(damageTaken);
            popup.gameObject.SetActive(true);
        }
    }
}
