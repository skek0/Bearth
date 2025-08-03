using System.Collections.Generic;
using UnityEngine;

public abstract class Module : MonoBehaviour, IDamageable
{
    [SerializeField]protected bool connectable = false;
    [SerializeField]protected List<BlankModule> connectedModules = new List<BlankModule>();
    public bool Connectable {  get { return connectable; } }
    public void GetDamage(float damage)
    {
        throw new System.NotImplementedException();
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
