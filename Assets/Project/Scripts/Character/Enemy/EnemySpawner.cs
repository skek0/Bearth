using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class EnemySpawner : SceneSingleton<EnemySpawner>
{
    [ReadOnly][SerializeField] private float spawnInterval = 2f;
    [ReadOnly][SerializeField] private int currentEnemyCount = 0;

    [Header("Spawn Ring (화면 바깥)")]
    [SerializeField] private float spawnMargin = 3f;        // 화면 모서리 바깥 최소 여유
    [SerializeField] private float spawnRingWidth = 5f;     // 링 두께

    [Header("Despawn")]
    [SerializeField] private float despawnDistance = 60f;
    [SerializeField] private float despawnBuffer = 5f;      // 스폰 반경 바깥 최소 여유
    [SerializeField] private float despawnCheckInterval = 1f;

    private readonly List<GameObject> enemies = new();
    private RegionStat currentRegion;
    private int totalWeight;
    private bool subscribed;
    private Camera cam;
    private float spawnTimer;
    private float despawnTimer;

    protected override void Awake()
    {
        base.Awake();
        cam = Camera.main;
    }

    private void OnEnable() => TrySubscribe();

    private void OnDisable()
    {
        if (subscribed && RegionManager.Instance != null)
            RegionManager.Instance.CurrentRegionChanged -= OnRegionChanged;
        subscribed = false;
    }

    private bool TrySubscribe()
    {
        var rm = RegionManager.Instance;
        if (rm == null) return false;

        rm.CurrentRegionChanged += OnRegionChanged;
        subscribed = true;
        OnRegionChanged(rm.CurrentRegionId); // 구독 전에 정해진 초기 지역 반영
        return true;
    }

    private void OnRegionChanged(string regionId)
    {
        totalWeight = 0;

        if (regionId == null || !RegionSpecDB.TryGetRegion(regionId, out var region))
        {
            currentRegion = null;
            return;
        }

        currentRegion = region;
        if (region.Enemies == null) return;

        foreach (var e in region.Enemies)
            totalWeight += Mathf.Max(0, e.Weight);
    }

    private void Update()
    {
        if (!subscribed && !TrySubscribe()) return;

        var player = RegionManager.Instance.Player;
        if (player == null) return;

        float dt = Time.deltaTime;

        despawnTimer += dt;
        if (despawnTimer >= despawnCheckInterval)
        {
            despawnTimer = 0f;
            DespawnFarEnemies(player);
        }

        spawnTimer += dt;
        if (spawnTimer >= spawnInterval)
        {
            spawnTimer = 0f;
            TrySpawn(player);
        }

        currentEnemyCount = enemies.Count;
    }

    private void TrySpawn(Transform player)
    {
        if (currentRegion == null || totalWeight <= 0) return;

        enemies.RemoveAll(e => e == null);
        if (enemies.Count >= currentRegion.MaxEnemyCount) return;

        string shipId = PickShipId();
        if (shipId == null) return;

        var ship = ShipMaker.CreateShip(shipId, GetSpawnPosition(player));
        if (ship != null) enemies.Add(ship);
    }

    private string PickShipId()
    {
        int roll = Random.Range(0, totalWeight);
        foreach (var e in currentRegion.Enemies)
        {
            int w = Mathf.Max(0, e.Weight);
            if (roll < w) return e.ShipId;
            roll -= w;
        }
        return null;
    }

    private float GetScreenRadius()
    {
        float halfH = cam.orthographicSize;
        float halfW = halfH * cam.aspect;
        return Mathf.Sqrt(halfW * halfW + halfH * halfH);
    }

    private Vector2 GetSpawnPosition(Transform player)
    {
        float r = GetScreenRadius() + spawnMargin + Random.Range(0f, spawnRingWidth);
        float angle = Random.Range(0f, Mathf.PI * 2f);
        Vector2 offset = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * r;
        return (Vector2)player.position + offset;
    }

    private void DespawnFarEnemies(Transform player)
    {
        // 스폰 반경보다 안쪽이면 스폰하자마자 디스폰되므로 하한을 둠
        float minLimit = GetScreenRadius() + spawnMargin + spawnRingWidth + despawnBuffer;
        float limit = Mathf.Max(despawnDistance, minLimit);
        float limitSqr = limit * limit;
        Vector2 p = player.position;

        for (int i = enemies.Count - 1; i >= 0; i--)
        {
            var e = enemies[i];
            if (e == null) { enemies.RemoveAt(i); continue; }

            if (((Vector2)e.transform.position - p).sqrMagnitude < limitSqr) continue;

            DestroyShip(e);
            enemies.RemoveAt(i);
        }
    }

    private static void DestroyShip(GameObject core)
    {
        // CancelSpawn이 인스턴스 머티리얼까지 정리해 줌
        foreach (var m in core.GetComponentsInChildren<Module>(true))
            m.CancelSpawn();
    }
}