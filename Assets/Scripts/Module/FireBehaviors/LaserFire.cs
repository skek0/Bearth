using System;
using System.Collections;
using UnityEngine;
// 레이저형
[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Laser")]
public class LaserFire : FireBehavior
{
    public override IEnumerator Fire(Action<float> spawn)
    {
        throw new NotImplementedException();
    }
}