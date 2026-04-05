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

    private void Update()
    {
        ai.UpdateAI(new EnemyContext(player));
    }
}
