using System.Collections.Generic;
using UnityEngine;

public interface IFireBehavior
{
    public void BuildShots(List<Shot> outShots);
}
public class SingleFire : IFireBehavior
{
    public void BuildShots(List<Shot> outShots)
    {
        outShots.Add(new Shot { angleDeg = 0f, delay = 0f });
    }
}
public class ShotgunFire : IFireBehavior
{
    public int pelletCount = 6;
    public float spreadAngle = 12f;
    public ShotgunFire(int pelletCount, float spreadAngle)
    {
        this.pelletCount = pelletCount;
        this.spreadAngle = spreadAngle;
    }
    public void BuildShots(List<Shot> outShots)
    {
        if (pelletCount <= 1) { outShots.Add(new Shot { angleDeg = 0f, delay = 0f }); return; }

        float step = spreadAngle / (pelletCount - 1);
        float start = -spreadAngle * 0.5f;

        for (int i = 0; i < pelletCount; i++)
            outShots.Add(new Shot { angleDeg = start + step * i, delay = 0f });
    }
}
public class BurstFire : IFireBehavior
{
    public int burstCount = 3;
    public float burstInterval = 0.08f;
    public BurstFire(int burstCount, float burstInterval)
    {
        this.burstCount = burstCount;
        this.burstInterval = burstInterval;
    }
    public void BuildShots(List<Shot> outShots)
    {
        for (int i = 0; i < burstCount; i++)
            outShots.Add(new Shot { angleDeg = 0f, delay = i == 0 ? 0f : burstInterval });
    }
}
