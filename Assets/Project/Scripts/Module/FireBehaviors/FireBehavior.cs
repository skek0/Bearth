using System;
using System.Collections;
using UnityEngine;


public abstract class FireBehavior : ScriptableObject
{
    public abstract IEnumerator Fire(Action<float> spawn);
}
