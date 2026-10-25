using System;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : SceneSingleton<EnemySpawner>
{
    [ReadOnly][SerializeField] private float spawnInterval = 2f;
    [ReadOnly][SerializeField] private int maxEnemyCount = 20;
    [ReadOnly][SerializeField] private int currentEnemyCount = 0;
    [ReadOnly][SerializeField] private List<string> shipIdPool = new List<string>();

    private float timer;

    private void Start()
    {

    }
    private void Update()
    {
        timer += Time.deltaTime;

        if (timer < spawnInterval)
            return;

        timer = 0f;

        TrySpawn();
    }

    private void TrySpawn()
    {
        ShipMaker.CreateShip("tank", Vector2.zero);
    }
}