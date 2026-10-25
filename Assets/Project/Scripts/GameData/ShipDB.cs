using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[Serializable]
public class ShipEntry
{
    public string ShipId;
    public ShipSaveData Data;
}

public static class ShipSpecDB
{
    public const string ResourcePath = "Enemies/EnemyShips";

    /// <summary>에디터에서 저장할 때 쓰는 실제 파일 경로</summary>
    public static string FilePath =>
        Path.Combine(Application.dataPath, "Resources", ResourcePath + ".json");

    private static readonly Dictionary<string, ShipSaveData> _ships = new();

    public static IReadOnlyDictionary<string, ShipSaveData> Ships => _ships;
    public static IReadOnlyCollection<string> ShipIds => _ships.Keys;

    public static void Load()
    {
        _ships.Clear();

        var json = JsonLoader.LoadText(ResourcePath);
        if (json == null) return;

        foreach (var pair in JsonLoader.LoadDictionary<ShipEntry>(
                     json,
                     row => row.ShipId,
                     "Ships"))
        {
            if (pair.Value.Data == null)
            {
                Debug.LogWarning($"[ShipSpecDB] '{pair.Key}': Data is null, skipped");
                continue;
            }
            _ships.Add(pair.Key, pair.Value.Data);
        }
    }

    public static bool TryGetShip(string shipId, out ShipSaveData data)
        => _ships.TryGetValue(shipId, out data);

    public static bool Contains(string shipId) => _ships.ContainsKey(shipId);
}