using System;
using UnityEngine;

public interface IControllable
{
    event Action<IControllable> OnDestroyed;
    public void OnSelected();
    public void OnDrag(Vector2 pos);
    public void OnDeselected();
}