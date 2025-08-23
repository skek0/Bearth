using UnityEngine;
using System.Collections;
using System;
// 일반형
[CreateAssetMenu(menuName = "Weapon/FireBehaviors/Single")]
public class SingleFire : FireBehavior
{
    public override IEnumerator Fire(Action<float> spawn)
    {
        spawn(0f); // 각도 보정 0, 속도 기본
        yield break;
    }
}