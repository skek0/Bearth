using UnityEngine;

public static class GameData
{
    private static bool initialized;

    public static void Initialize()
    {
        if (initialized) return;
        initialized = true;

        ModuleSpecDB.Load();
        EnemyShipDB.Load();
        RegionSpecDB.Load();

        Validate();
    }

    private static void Validate()
    {
        // 함선 → 모듈 ID 검증
        foreach (var ship in EnemyShipDB.Ships)
        {
            foreach (var m in ship.Value.modules)
            {
                if (!ModuleSpecDB.BaseStats.ContainsKey(m.moduleId))
                    Debug.LogError($"[GameDB] Ship '{ship.Key}': unknown moduleId '{m.moduleId}'");
            }
        }

        // 지역 → 함선 ID, 가중치 검증
        foreach (var region in RegionSpecDB.Regions)
        {
            if (region.Value.Enemies == null) continue;

            foreach (var e in region.Value.Enemies)
            {
                if (!EnemyShipDB.Contains(e.ShipId))
                    Debug.LogError($"[GameDB] Region '{region.Key}': unknown shipId '{e.ShipId}'");
                if (e.Weight <= 0)
                    Debug.LogError($"[GameDB] Region '{region.Key}': invalid weight for '{e.ShipId}'");
            }
        }
    }
}