using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class EnemyShipEntry
{
    public string ShipId;
    public ShipSaveData Data;
}

public static class EnemyShipDB
{
    public const string ResourcePath = "Enemies/EnemyShips";

    private static readonly Dictionary<string, ShipSaveData> _ships = new();

    public static IReadOnlyDictionary<string, ShipSaveData> Ships => _ships;

    public static void Load()
    {
        _ships.Clear();

        var json = JsonLoader.LoadText(ResourcePath);
        if (json == null) return;

        foreach (var pair in JsonLoader.LoadDictionary<EnemyShipEntry>(
                     json,
                     row => row.ShipId,
                     "Ships"))
        {
            if (pair.Value.Data == null)
            {
                Debug.LogWarning($"[EnemyShipDB] '{pair.Key}': Data is null, skipped");
                continue;
            }
            _ships.Add(pair.Key, pair.Value.Data);
        }
    }

    public static bool TryGetShip(string shipId, out ShipSaveData data) => _ships.TryGetValue(shipId, out data);

    public static bool Contains(string shipId) => _ships.ContainsKey(shipId);
}