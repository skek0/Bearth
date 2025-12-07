using System.Collections.Generic;
using UnityEngine;

public enum FactionType
{
    Neutral,
    Enemy,
    Mine
}
public abstract class Module : MonoBehaviour, IDamageable
{
    protected bool connectable = false;
    [SerializeField] protected List<BlankModule> connectedModules = new List<BlankModule>();
    [SerializeField] protected FactionType faction = FactionType.Neutral;
    [SerializeField] protected BasicInfo baseStat;  // 캐싱용 serializefield
    [SerializeField] protected int hp = 5;
    public bool Connectable {  get { return connectable; } }

    protected virtual void Awake()
    {
        //gameObject.layer = LayerMask.NameToLayer("Module");
        hp = baseStat.MaxHp;
    }
    public virtual void TakeDamage(DamageData damage)
    {
        hp -= damage.Amount;
        
        if(hp <= 0)
        {
            hp = 0;
            OnDeath();

            for(int i = connectedModules.Count - 1; i >= 0; i--)
            {
                connectedModules[i].Detach(transform.position);
            }
            Destroy(gameObject);
        }
    }
    protected virtual void OnDeath() {}
    public void AddConnectedModule(BlankModule module)
    {
        connectedModules.Add(module);
    }
    public void RemoveConnectedModule(BlankModule module)
    {
        connectedModules.Remove(module);
    }
}
