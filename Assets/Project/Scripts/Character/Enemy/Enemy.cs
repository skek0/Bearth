using UnityEngine;

public class Enemy : MonoBehaviour
{
    [SerializeField] EnemyAI ai;

    [Header("Temp")]
    [SerializeField] GameObject player;

    private void Awake()
    {
        ai = GetComponent<EnemyAI>();
    }

    public void SetTarget(GameObject target) => player = target;

    private void Update()
    {
        if (player == null || ai == null) return;
        ai.UpdateAI(new EnemyContext(player));
    }
}
