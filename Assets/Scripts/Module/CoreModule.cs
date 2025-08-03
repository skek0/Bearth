using UnityEngine;

public class CoreModule : Module
{
    protected virtual void Awake()
    {
        connectable = true;
    }
}
