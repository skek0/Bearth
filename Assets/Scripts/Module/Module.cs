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
    public bool Connectable {  get { return connectable; } }
    public void TakeDamage(DamageData damage)
    {
        Debug.Log($"{gameObject.name} took {damage.Amount} damage of type {damage.Type}");

        health -= damage.Amount;

        if (health <= 0)
        {
            //Die();
        }

        //EventBus.RaiseDamageEvent(gameObject, damage);
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
