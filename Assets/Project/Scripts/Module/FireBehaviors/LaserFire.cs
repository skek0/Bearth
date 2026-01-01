using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// 레이저형
[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Laser")]
public class LaserFire : FireBehavior
{
    public override void BuildShots(List<Shot> outShots)
    {
        outShots.Add(new Shot { angleDeg = 0f, delay = 0f });
    }
}