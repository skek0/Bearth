using System.Collections;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Laser : MonoBehaviour
{
    float damagePerSec;
    LineRenderer lineRenderer;
    public float sight;

    void FireLaser()
    {
        RaycastHit hit;
        Vector3 origin = transform.position;
        Vector3 direction = transform.forward;

        if (Physics.Raycast(origin, direction, out hit, sight))
        {
            if (hit.transform.TryGetComponent(out IDamageable damagable))
            {
                damagable.GetDamage(damagePerSec * Time.deltaTime);
            }
            lineRenderer.SetPosition(1, hit.point); // 맞은 위치까지 라인 설정
        }
        else
        {
            lineRenderer.SetPosition(1, origin + direction * sight); // 최대 사거리까지 레이저 그리기
        }

        // 시각적 효과
        lineRenderer.SetPosition(0, origin);
        lineRenderer.enabled = true;
    }

}