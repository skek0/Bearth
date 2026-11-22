using System;
using System.Collections.Generic;

/// <summary>
/// 플레이어 성장 상태: scrap 보유량과 업그레이드 레벨.
/// 획득 방식은 아직 미정이므로 AddScrap만 열어 둔다.
/// </summary>
public static class PlayerProgress
{
    private static int scrap;
    private static readonly Dictionary<string, int> levels = new();

    public static int Scrap => scrap;

    /// <summary>scrap 값이 바뀔 때 (현재 값)</summary>
    public static event Action<int> ScrapChanged;
    /// <summary>업그레이드 레벨이 바뀔 때 (업그레이드 ID, 새 레벨). 전체 리셋 시 ID는 null.</summary>
    public static event Action<string, int> UpgradeChanged;

    #region Scrap
    public static void AddScrap(int amount)
    {
        if (amount <= 0) return;
        scrap += amount;
        ScrapChanged?.Invoke(scrap);
    }

    public static bool TrySpendScrap(int amount)
    {
        if (amount < 0 || scrap < amount) return false;
        scrap -= amount;
        ScrapChanged?.Invoke(scrap);
        return true;
    }
    #endregion

    #region Upgrade
    public static int GetLevel(string upgradeId) => levels.TryGetValue(upgradeId, out var lv) ? lv : 0;

    public static bool IsMaxLevel(string upgradeId)
    {
        return !UpgradeSpecDB.TryGet(upgradeId, out var spec) || GetLevel(upgradeId) >= spec.MaxLevel;
    }

    /// <summary>다음 레벨 비용. 최대 레벨이면 false.</summary>
    public static bool TryGetNextCost(string upgradeId, out int cost)
    {
        cost = 0;
        if (!UpgradeSpecDB.TryGet(upgradeId, out var spec)) return false;

        int lv = GetLevel(upgradeId);
        if (lv >= spec.MaxLevel) return false;

        cost = spec.Levels[lv].Cost;
        return true;
    }

    public static bool CanUpgrade(string upgradeId)
    {
        return TryGetNextCost(upgradeId, out int cost) && scrap >= cost;
    }

    public static bool TryUpgrade(string upgradeId)
    {
        if (!TryGetNextCost(upgradeId, out int cost)) return false;
        if (!TrySpendScrap(cost)) return false;

        int newLevel = GetLevel(upgradeId) + 1;
        levels[upgradeId] = newLevel;
        UpgradeChanged?.Invoke(upgradeId, newLevel);
        return true;
    }

    /// <summary>
    /// 원본 CoreStat을 복사한 뒤 현재 레벨까지의 Add 값을 모두 더해 반환. 원본은 건드리지 않는다.
    /// </summary>
    public static CoreStat BuildCoreStat(CoreStat source)
    {
        if (source == null) return null;

        var stat = source.Clone();
        foreach (var spec in UpgradeSpecDB.Ordered)
        {
            int lv = Math.Min(GetLevel(spec.UpgradeID), spec.MaxLevel);
            for (int i = 0; i < lv; i++)
            {
                var add = spec.Levels[i].Add;
                if (add == null) continue;
                foreach (var kv in add)
                    CoreStatFields.AddTo(stat, kv.Key, kv.Value);
            }
        }
        return stat;
    }

    /// <summary>해당 업그레이드의 현재 레벨까지 누적 증가량 (UI 표시용)</summary>
    public static Dictionary<string, float> GetTotalAdd(string upgradeId, int level)
    {
        var total = new Dictionary<string, float>();
        if (!UpgradeSpecDB.TryGet(upgradeId, out var spec)) return total;

        level = Math.Min(level, spec.MaxLevel);
        for (int i = 0; i < level; i++)
        {
            var add = spec.Levels[i].Add;
            if (add == null) continue;
            foreach (var kv in add)
                total[kv.Key] = (total.TryGetValue(kv.Key, out var v) ? v : 0f) + kv.Value;
        }
        return total;
    }
    #endregion

    #region Save
    public static void WriteTo(PlayerSaveData data)
    {
        data.scrap = scrap;
        data.upgrades = new List<UpgradeSaveData>();
        foreach (var kv in levels)
            data.upgrades.Add(new UpgradeSaveData { upgradeId = kv.Key, level = kv.Value });
    }

    public static void ReadFrom(PlayerSaveData data)
    {
        scrap = Math.Max(0, data?.scrap ?? 0);
        levels.Clear();

        if (data?.upgrades != null)
        {
            foreach (var u in data.upgrades)
            {
                if (string.IsNullOrWhiteSpace(u.upgradeId)) continue;
                if (!UpgradeSpecDB.TryGet(u.upgradeId, out var spec)) continue; // 삭제된 업그레이드는 무시
                levels[u.upgradeId] = Math.Clamp(u.level, 0, spec.MaxLevel);
            }
        }

        ScrapChanged?.Invoke(scrap);
        UpgradeChanged?.Invoke(null, 0);
    }
    #endregion
}
