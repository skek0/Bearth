using System;

[Serializable]
public struct Shot
{
    public float angleDeg;   // 필수
    public float delay;      // 이 샷을 쏘기 전 대기(초). 단발이면 0
}