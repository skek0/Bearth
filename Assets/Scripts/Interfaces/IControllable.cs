using UnityEngine;

public interface IControllable
{
    public void OnSelected();
    public void OnDrag(Vector2 pos);
    public void OnDeselected();
}