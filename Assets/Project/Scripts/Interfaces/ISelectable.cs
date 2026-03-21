using System;
using UnityEngine;

public interface ISelectable
{
    public void OnSelected();
    public void OnDrag(Vector2 pos);
    public void OnDeselected();
}