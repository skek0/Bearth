using UnityEngine;
using System.Collections;
using TMPro;
using static Unity.Burst.Intrinsics.X86.Avx;
using Unity.VisualScripting;

/*
public struct DamageInfo
{
    public Vector3 Position;
    public int Amount;
    public DamageType Type;
}
*/
public class DamageSkin : MonoBehaviour
{
    [Header("Motion")]
    [SerializeField] float lifetime = 0.7f;
    [SerializeField] float riseSpeed = 2.2f;     // 위로 떠오르는 속도
    [SerializeField] float horizontalJitter = 0.5f; // 좌우 살짝 흔들림
    [SerializeField] AnimationCurve scaleCurve = AnimationCurve.EaseInOut(0, 0.6f, 0.15f, 1.2f);

    [Header("Fade")]
    [SerializeField] float fadeStart = 0.45f;    // 이 시점부터 페이드
    [SerializeField] float fadeDuration = 0.25f;

    [Header("Style")]
    [SerializeField] Color normalColor = Color.white;
    [SerializeField] Color criticalColor = new Color(1f, 0.3f, 0.2f);

    TextMeshPro tmp; 
    float timer = 0f;
    Color baseColor;

    private void Awake()    
    {
        if(tmp == null)tmp = GetComponent<TextMeshPro>();
    }
    void Update()
    {
        timer += Time.deltaTime;

        // 이동
        transform.position += Time.deltaTime * riseSpeed * Vector3.up;

        // 스케일 펀치
        float tNorm = Mathf.Clamp01(timer / lifetime);
        float scale = scaleCurve.Evaluate(Mathf.Min(tNorm, 0.15f));
        transform.localScale = Vector3.one * scale;

        // 페이드
        if (timer >= fadeStart)
        {
            float f = Mathf.InverseLerp(fadeStart, fadeStart + fadeDuration, timer);
            var c = baseColor;
            c.a = Mathf.Lerp(1f, 0f, f);
            tmp.color = c;
        }
    }
    public void SetInfo(DamageInfo damageInfo)
    {
        transform.position = damageInfo.Position + Random.Range(-horizontalJitter, horizontalJitter) * Vector3.right;
        tmp.text = damageInfo.Amount.ToString();
    }
    private void OnEnable()
    {
        StartCoroutine(FloatForSeconds());
    }
    private void OnDisable()
    {
        timer = 0f;
    }

    IEnumerator FloatForSeconds()
    {
        yield return CoroutineCache.WaitforSeconds(lifetime);
        gameObject.SetActive(false);
    }
}
