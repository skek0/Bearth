using UnityEngine;
using System.Collections;
using System;
// 산탄형
[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Shotgun")]
public class ShotgunFire : FireBehavior
{
    public int pelletCount = 6;
    public float spreadAngle = 12f; // 전체 부채꼴 각도

    public override IEnumerator Fire(Action<float> spawn)
    {
        if (pelletCount <= 1) { spawn(0f); yield break; }

        float step = spreadAngle / (pelletCount - 1);
        float start = -spreadAngle * 0.5f;

        for (int i = 0; i < pelletCount; i++)   
            spawn(start + step * i);
    }
}