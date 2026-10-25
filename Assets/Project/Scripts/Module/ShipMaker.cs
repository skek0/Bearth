using System.Collections.Generic;
using UnityEngine;

public class ShipMaker
{
    static readonly Dictionary<string, ShipSaveData> cache = new();

    public static GameObject CreateShip(string shipId, Vector2 spawnPos)
    {
        if (!ShipSpecDB.TryGetShip(shipId, out var data))
        {
            Debug.LogError($"[ShipMaker] Unknown shipId: {shipId}");
            return null;
        }

        var core = ShipSaveLoad.LoadShipToRoot(ShipContainer.Instance.transform, data, spawnPos);
        return core != null ? core.gameObject : null;
    }
    static bool TryGetShipData(string shipId, out ShipSaveData data)
    {
        if (cache.TryGetValue(shipId, out data)) return true;

        var ta = Resources.Load<TextAsset>("Enemies/" + shipId);
        if (ta == null)
        {
            Debug.LogError($"[ShipMaker] Ship resource not found: Enemies/{shipId}");
            return false;
        }

        data = JsonUtility.FromJson<ShipSaveData>(ta.text);
        if (data == null) return false;

        cache[shipId] = data;
        return true;
    }
}
