using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Shotgun")]
public class ShotgunFire : FireBehavior
{
    public int pelletCount = 6;
    public float spreadAngle = 12f;

    public override void BuildShots(List<Shot> outShots)
    {
        if (pelletCount <= 1) { outShots.Add(new Shot { angleDeg = 0f, delay = 0f }); return; }

        float step = spreadAngle / (pelletCount - 1);
        float start = -spreadAngle * 0.5f;

        for (int i = 0; i < pelletCount; i++)
            outShots.Add(new Shot { angleDeg = start + step * i, delay = 0f });
    }
}
