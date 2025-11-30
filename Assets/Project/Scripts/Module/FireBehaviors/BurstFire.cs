using System;
using System.Collections;
using UnityEngine;
// 점사형
[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Burst")]
public class BurstFire : FireBehavior
{
    public int burstCount = 3;          // 점사 수
    public float burstInterval = 0.08f; // 발 사이 시간(초)

    public override IEnumerator Fire(Action<float> spawn)
    {
        for (int i = 0; i < burstCount; i++)
        {
            spawn(0f);
            if (i < burstCount - 1)
                yield return new WaitForSeconds(burstInterval);
        }
    }
}
