using UnityEngine;

public abstract class RangedBehavior : ScriptableObject
{
    public abstract void Fire(FireContext ctx);
}
