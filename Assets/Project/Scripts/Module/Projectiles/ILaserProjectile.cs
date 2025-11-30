using UnityEngine;
public interface ILaserProjectile
{
    // dps: 초당 데미지, duration: 켜져 있을 시간(초)
    void SetInfo(float dps, float duration, Transform firePoint);
}