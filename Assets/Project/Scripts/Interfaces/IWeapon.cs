using UnityEngine;

public interface IWeapon
{
    Transform AttackPoint { get; }
    void Attack();
}