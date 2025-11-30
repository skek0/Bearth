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
    protected List<BlankModule> connectedModules = new List<BlankModule>();
    [SerializeField]protected FactionType faction = FactionType.Neutral;
    [SerializeField]protected float health = 10;
    [SerializeField]public BasicInfo baseStat;
    public bool Connectable {  get { return connectable; } }

    protected virtual void Awake()
    {
        gameObject.layer = LayerMask.NameToLayer("Module");
    }
    public virtual void TakeDamage(DamageData damage)
    {
        Debug.Log($"{gameObject.name} needs TakeDamage");
    }
    public void AddConnectedModule(BlankModule module)
    {
        connectedModules.Add(module);
    }
    public void RemoveConnectedModule(BlankModule module)
    {
        connectedModules.Remove(module);
    }
}
