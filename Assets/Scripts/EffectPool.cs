using UnityEngine;

public class EffectPool : MonoBehaviour
{
    [SerializeField] GameObject lightning;

    private void Start()
    {
        ObjectPoolManager.Instance.CreatePool("Lightning", lightning, 3, transform);
    }
}
