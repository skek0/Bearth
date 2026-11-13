using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using static UnityEngine.Audio.IAudioGenerator;

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
public class CoreStat
{
    public string ModuleID;
    public int MaxAttachDepth;
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
    public const string baseStatPath = "ModuleSpecs/BaseStats";
    public const string coreStatPath = "ModuleSpecs/CoreStats";
    public const string rangedWeaponPath = "ModuleSpecs/WeaponRangedStats";
    private static Dictionary<string, BaseStat> _baseStats = new();
    private static Dictionary<string, CoreStat> _coreStats = new();
    private static Dictionary<string, WeaponRangedStat> _weaponRangedStats = new();


    public static IReadOnlyDictionary<string, BaseStat> BaseStats => _baseStats;
    public static IReadOnlyDictionary<string, CoreStat> CoreStats => _coreStats;
    public static IReadOnlyDictionary<string, WeaponRangedStat> WeaponRangedStats => _weaponRangedStats;

    public static void Load()
    {
        LoadBaseStats();
        LoadCoreStats();
        LoadWeaponRangedStats();
    }
    public static void LoadBaseStats()
    {
        _baseStats.Clear();

        foreach( var stat in JsonLoader.LoadFromResources<BaseStat>(baseStatPath) )
        {
            _baseStats.Add(stat.ModuleID, stat);
        }
    }

    public static void LoadCoreStats()
    {
        _coreStats.Clear();
        foreach (var stat in JsonLoader.LoadFromResources<CoreStat>(coreStatPath))
            _coreStats.Add(stat.ModuleID, stat);
    }
    
    public static void LoadWeaponRangedStats()
    {
        _weaponRangedStats.Clear();

        foreach (var stat in JsonLoader.LoadFromResources<WeaponRangedStat>(rangedWeaponPath))
        {
            _weaponRangedStats.Add(stat.ModuleID, stat);
        }
    }
}
