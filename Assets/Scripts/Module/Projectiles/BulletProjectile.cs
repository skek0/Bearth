using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/Projectile/Bullet")]
public class BulletProjectile : Projectile
{
    [SerializeField] int penetration;
}