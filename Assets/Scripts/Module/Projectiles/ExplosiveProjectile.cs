using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Projectile/Explosive")]
public class ExplosiveProjectile : Projectile
{
    [SerializeField] float radius;
    [SerializeField] float reducePerDist;
}