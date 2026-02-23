using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using UnityEngine;

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
    public string TypeID;
    public string Type;
    public int Tier;
    public string Rarity;
    public float Mass;
    public int MaxHp;
    public int Price;
    public string PrefabPath;
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
    public int Penetration;
    public string ProjectileID;
    public int PelletAmount;
}

public static class ModuleSpecDB
{
    private static Dictionary<string, BaseStat> _baseStats = new();
    private static Dictionary<string, WeaponRangedStat> _weaponRangedStats = new();

    public static IReadOnlyDictionary<string, BaseStat> BaseStats => _baseStats;
    public static IReadOnlyDictionary<string, WeaponRangedStat> WeaponRangedStats => _weaponRangedStats;

    public static void LoadBaseStats(string json)
    {
        _baseStats = JsonLoader.LoadDictionary<BaseStat>(
            json,
            row => row.ModuleID,
            tableName: "BaseStats"
        );
    }

    public static void LoadWeaponRangedStats(string json)
    {
        _weaponRangedStats = JsonLoader.LoadDictionary<WeaponRangedStat>(
            json,
            row => row.ModuleID, 
            tableName: "WeaponRangedStats"
        );
    }
}
