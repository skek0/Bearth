using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class RegionEnemyEntry
{
    public string ShipId;
    public int Weight;
}

[Serializable]
public class RegionStat
{
    public string RegionId;
    public List<RegionEnemyEntry> Enemies;
}

public static class RegionSpecDB
{
    private const string RegionPath = "Enemies/RegionEnemies";

    private static readonly Dictionary<string, RegionStat> _regions = new();
    public static IReadOnlyDictionary<string, RegionStat> Regions => _regions;

    public static void Load()
    {
        _regions.Clear();

        var json = JsonLoader.LoadText(RegionPath);
        if (json == null) return;

        foreach (var pair in JsonLoader.LoadDictionary<RegionStat>(
                     json,
                     row => row.RegionId,
                     "Regions"))
        {
            _regions.Add(pair.Key, pair.Value);
        }
    }

    public static bool TryGetRegion(string regionId, out RegionStat region)
        => _regions.TryGetValue(regionId, out region);
}