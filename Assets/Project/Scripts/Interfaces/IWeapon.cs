using UnityEngine;

public interface IWeapon
{
    Transform FirePoint { get; }
    void Attack();
}