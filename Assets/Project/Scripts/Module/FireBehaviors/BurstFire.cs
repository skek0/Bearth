using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Burst")]
public class BurstFire : FireBehavior
{
    public int burstCount = 3;
    public float burstInterval = 0.08f;

    public override void BuildShots(List<Shot> outShots)
    {
        for (int i = 0; i < burstCount; i++)
            outShots.Add(new Shot { angleDeg = 0f, delay = i == 0 ? 0f : burstInterval });
    }
}
