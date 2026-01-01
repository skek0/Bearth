using System.Collections.Generic;
using UnityEngine;

public abstract class FireBehavior : ScriptableObject
{
    /// <summary>
    /// "이번 1회 공격"에서 발사될 샷들을 outShots에 채운다.
    /// outShots는 호출 측에서 재사용한다(할당/GC 방지).
    /// </summary>
    public abstract void BuildShots(List<Shot> outShots);
}
