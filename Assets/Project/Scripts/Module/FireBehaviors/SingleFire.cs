using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Single")]
public class SingleFire : FireBehavior
{
    public override void BuildShots(List<Shot> outShots)
    {
        outShots.Add(new Shot { angleDeg = 0f, delay = 0f });
    }
}
