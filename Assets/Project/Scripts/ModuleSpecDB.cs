using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class BaseStatRoot
{
    public List<BaseStat> BaseStats;    // json RootKey
}
/// <summary>
/// ModuleID, TypeID, Type, Tier, Rarity, Mass, MaxHp, Price, PrefabPath
/// </summary>
[Serializable]
public class BaseStat
{
    public string ModuleID;
    public float Mass;
    public int MaxHp;
    public int Price;
    public string PrefabPath;
    public string ModuleType;
    public string Components;
    public string Tags;
}

[Serializable]
public class RangedWeaponSpecRoot 
{
    public List<WeaponRangedStat> RangedWeaponStats;
}

/// <summary>
/// ModuleID, FireType, FireMode, Damage, Speed, Interval, Predelay, Accuracy, Penetration, ProjectileID
/// </summary>
[Serializable]
public class WeaponRangedStat
{
    public string ModuleID;
    public string FireType;
    public string FireMode;
    public int Damage;
    public float Speed;
    public float Interval;
    public float PreDelay;
    public int Accuracy;
    public string ProjectileID;
    public int PelletAmount;
    public float BurstInterval;
}
[Serializable]
public class ProjectileInfo
{
    public string ProjectileID;
    public string TrailID;
    public string ShotSfxID;
    public string HitEffectID;
    public string HitSfxID;
    public int PrewarmCount;
}

public static class ModuleSpecDB
{
    private static Dictionary<string, BaseStat> _baseStats = new();
    private static Dictionary<string, WeaponRangedStat> _weaponRangedStats = new();


    public static IReadOnlyDictionary<string, BaseStat> BaseStats => _baseStats;
    public static IReadOnlyDictionary<string, WeaponRangedStat> WeaponRangedStats => _weaponRangedStats;

    public static void LoadBaseStats(string json)
    {
        _baseStats.Clear();

        foreach (var pair in JsonLoader.LoadDictionary<BaseStat>(
                     json,
                     row => row.ModuleID,
                     "BaseStats"))
        {
            _baseStats.Add(pair.Key, pair.Value);
        }
    }

    public static void LoadWeaponRangedStats(string json)
    {
        _weaponRangedStats.Clear();

        foreach (var pair in JsonLoader.LoadDictionary<WeaponRangedStat>(
                     json,
                     row => row.ModuleID,
                     "WeaponRangedStats"))
        {
            _weaponRangedStats.Add(pair.Key, pair.Value);
        }
    }
}
