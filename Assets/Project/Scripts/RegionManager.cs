using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RegionManager : SceneSingleton<RegionManager>
{
    [SerializeField] private float checkInterval = 0.25f;

    public event Action<string> CurrentRegionChanged;
    public string CurrentRegionId => current?.RegionId;
    public Transform Player => player;

    private readonly List<RegionStat> sorted = new();
    private RegionStat current;
    private int currentIndex = -1;
    private Transform player;
    private float timer;

    public void SetPlayer(Transform playerTransform)
    {
        player = playerTransform;
        timer = 0f;

        sorted.Clear();
        sorted.AddRange(RegionSpecDB.Regions.Values.OrderBy(r => r.MinDistance));

        ForceEvaluate();
    }

    private void Update()
    {
        if (player == null || sorted.Count == 0) return;

        timer += Time.deltaTime;
        if (timer < checkInterval) return;
        timer = 0f;

        Evaluate();
    }

    // 히스테리시스 없이 현재 거리에 맞는 지역을 바로 정함 (초기화/리스폰용)
    private void ForceEvaluate()
    {
        if (player == null || sorted.Count == 0) return;

        float dist = ((Vector2)player.position).magnitude;

        int idx = 0;
        for (int i = 0; i < sorted.Count; i++)
        {
            if (dist >= sorted[i].MinDistance) idx = i;
        }
        SetRegion(idx);
    }

    private void Evaluate()
    {
        if (currentIndex < 0) { ForceEvaluate(); return; }

        float dist = ((Vector2)player.position).magnitude;
        int idx = currentIndex;

        // 바깥 지역 진입: 다음 지역의 MinDistance를 넘으면 바로 전환
        while (idx + 1 < sorted.Count && dist >= sorted[idx + 1].MinDistance)
            idx++;

        // 안쪽 지역 복귀: 현재 지역의 MinDistance - Margin 아래로 내려가야 전환
        while (idx > 0 && dist < sorted[idx].MinDistance - sorted[idx].Margin)
            idx--;

        if (idx != currentIndex) SetRegion(idx);
    }

    private void SetRegion(int idx)
    {
        currentIndex = idx;

        var next = sorted[idx];
        if (current == next) return;

        current = next;
        Debug.Log($"[RegionManager] Region changed: {current.RegionId}");
        CurrentRegionChanged?.Invoke(current.RegionId);
    }
}