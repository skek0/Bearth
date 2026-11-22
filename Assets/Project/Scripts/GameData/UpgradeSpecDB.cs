using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

/// <summary>
/// 업그레이드 한 단계. Add의 키는 CoreStat 필드 이름, 값은 그 단계에서 더해지는 양.
/// </summary>
[Serializable]
public class UpgradeLevel
{
    public int Cost;
    public Dictionary<string, float> Add = new();
}

/// <summary>
/// 업그레이드 항목. Levels 배열의 길이가 최대 레벨(한계치).
/// </summary>
[Serializable]
public class UpgradeSpec
{
    public string UpgradeID;
    public string DisplayName;
    public List<UpgradeLevel> Levels = new();

    public int MaxLevel => Levels?.Count ?? 0;
}

public static class UpgradeSpecDB
{
    public const string upgradeSpecPath = "ModuleSpecs/UpgradeSpecs";

    // JSON 파일 순서를 유지 (UI 표시 순서)
    private static readonly List<UpgradeSpec> _ordered = new();
    private static readonly Dictionary<string, UpgradeSpec> _specs = new();

    public static IReadOnlyList<UpgradeSpec> Ordered => _ordered;
    public static IReadOnlyDictionary<string, UpgradeSpec> Specs => _specs;

    public static void Load()
    {
        _ordered.Clear();
        _specs.Clear();

        foreach (var spec in JsonLoader.LoadFromResources<UpgradeSpec>(upgradeSpecPath))
        {
            if (spec == null || string.IsNullOrWhiteSpace(spec.UpgradeID)) continue;
            spec.Levels ??= new List<UpgradeLevel>();
            _ordered.Add(spec);
            _specs.Add(spec.UpgradeID, spec);
        }
    }

    public static bool TryGet(string upgradeId, out UpgradeSpec spec) => _specs.TryGetValue(upgradeId, out spec);

    /// <summary>
    /// Add 키가 CoreStat의 int/float 필드인지 검사.
    /// </summary>
    public static void Validate()
    {
        foreach (var spec in _ordered)
        {
            if (spec.MaxLevel == 0)
                Debug.LogWarning($"[GameDB] Upgrade '{spec.UpgradeID}': Levels empty");

            for (int i = 0; i < spec.Levels.Count; i++)
            {
                var lv = spec.Levels[i];
                if (lv.Cost < 0)
                    Debug.LogError($"[GameDB] Upgrade '{spec.UpgradeID}' Lv{i + 1}: negative cost");
                if (lv.Add == null) continue;

                foreach (var key in lv.Add.Keys)
                {
                    if (CoreStatFields.Get(key) == null)
                        Debug.LogError($"[GameDB] Upgrade '{spec.UpgradeID}' Lv{i + 1}: unknown CoreStat field '{key}'");
                }
            }
        }
    }
}

/// <summary>
/// CoreStat 필드를 이름으로 접근 (업그레이드 가산용)
/// </summary>
public static class CoreStatFields
{
    private static Dictionary<string, FieldInfo> _fields;

    public static FieldInfo Get(string name)
    {
        if (_fields == null)
        {
            _fields = new Dictionary<string, FieldInfo>();
            foreach (var f in typeof(CoreStat).GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (f.FieldType == typeof(int) || f.FieldType == typeof(float))
                    _fields[f.Name] = f;
            }
        }
        return _fields.TryGetValue(name, out var fi) ? fi : null;
    }

    public static void AddTo(CoreStat stat, string name, float amount)
    {
        var f = Get(name);
        if (f == null) return;

        if (f.FieldType == typeof(int))
            f.SetValue(stat, (int)f.GetValue(stat) + Mathf.RoundToInt(amount));
        else
            f.SetValue(stat, (float)f.GetValue(stat) + amount);
    }
}
