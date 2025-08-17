using System.Collections;
using UnityEngine;

public class GunModule : WeaponModule
{
    [SerializeField] protected int damage;
    [SerializeField] protected float speed;
    [SerializeField] protected float interval;
    [SerializeField] protected float preDelay;
    [SerializeField] protected float accuracy;
    [SerializeField] Transform firePoint;

    bool isReady = false;
    Coroutine readyCoroutine;
    
    // (선택) 시작 시 장착 상태면 preDelay부터 걸고 싶다면 활성화
    private void OnEnable()
    {
        if (attackable)
            StartPreDelay();
        else
            isReady = false;
    }
    public override void Attack()
    {
        if(isReady)
        {
            GameObject bullet = ObjectPoolManager.Instance.GetObject("Bullet");
            bullet.transform.SetPositionAndRotation(firePoint.position, transform.rotation);

            bullet.GetComponent<Bullet>().SetBulletInfo(damage, speed);
            bullet.SetActive(true);
            isReady = false;
            readyCoroutine = StartCoroutine(WaitforInterval(interval));
        }
    }
    private IEnumerator WaitforInterval(float time)
    {
        yield return CoroutineCache.WaitforSeconds(time);
        isReady = true;
        readyCoroutine = null;
    }
    private void RestartTimer(float t)
    {
        if (readyCoroutine != null)
        {
            StopCoroutine(readyCoroutine);
            readyCoroutine = null;
        }
        readyCoroutine = StartCoroutine(WaitforInterval(t));
    }
    public override void OnSelected()
    {
        base.OnSelected();
        if(readyCoroutine != null)
        {
            StopCoroutine(readyCoroutine);
            readyCoroutine = null;
        }
        isReady = false;
    }
    public override void OnDeselected()
    {
        // 1) 먼저 게이트를 내리고 타이머 정지 (한 프레임 발사 창 방지)
        if (readyCoroutine != null)
        {
            StopCoroutine(readyCoroutine);
            readyCoroutine = null;
        }
        isReady = false;

        base.OnDeselected();

        // 3) 부착 성공(= attackable) 시 preDelay 시작
        if (attackable)
            StartPreDelay();
    }
    private void StartPreDelay()
    {
        isReady = false;
        RestartTimer(preDelay);
    }
    private void OnDisable()
    {
        if (readyCoroutine != null)
        {
            StopCoroutine(readyCoroutine);
            readyCoroutine = null;
        }
        isReady = false;
    }
}
