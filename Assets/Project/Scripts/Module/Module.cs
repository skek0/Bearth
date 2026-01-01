using System.Collections.Generic;
using UnityEngine;

public enum FactionType
{
    Neutral,
    Enemy,
    Mine
}

[RequireComponent(typeof(ModuleGuid))]
[RequireComponent(typeof(ModuleTypeId))]
public abstract class Module : MonoBehaviour, IDamageable
{
    protected bool connectable = false;
    [SerializeField] private List<BaseModule> connectedModules = new();
    [SerializeField] protected FactionType faction = FactionType.Neutral;
    [SerializeField] protected BasicInfo baseStat;  // 캐싱용 serializefield
    [SerializeField] protected int hp = 5;
    public IReadOnlyList<BaseModule> ConnectedModules => connectedModules;
    public bool Connectable => connectable;

    protected virtual void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Module");
        hp = baseStat.MaxHp;
    }
    public void ApplyDamage(DamageData damage)
    {
        hp -= damage.Amount;
        
        if(hp <= 0)
        {
            hp = 0;
            OnDeath();

            for(int i = ConnectedModules.Count - 1; i >= 0; i--)
            {
                ConnectedModules[i].Detach(transform.position, true);
            }
            Destroy(gameObject);
        }
    }
    protected virtual void OnDeath() {}
    public void AddConnectedModule(BaseModule module)
    {
        connectedModules.Add(module);
    }
    public void RemoveConnectedModule(BaseModule module)
    {
        connectedModules.Remove(module);
    }
}
