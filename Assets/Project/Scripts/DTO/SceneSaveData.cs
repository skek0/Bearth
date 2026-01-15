using System;
using System.Collections.Generic;
using UnityEngine;


#region Scene Save (whole scene)

[Serializable]
public class SceneSaveData
{
    public int version = 1;
    public List<ShipInstanceSaveData> ships = new();
    public List<WorldModuleSaveData> looseModules = new();
}

[Serializable]
public class ShipInstanceSaveData
{
    public string shipId;          // 코어 guid 재사용 권장
    public Vector2 worldPos;       // 우주선 루트(코어) 월드 위치
    public float worldRotZ;        // 우주선 루트(코어) 월드 회전
    public ShipSaveData ship;      // 기존 단일 우주선 저장 데이터
}

[Serializable]
public class WorldModuleSaveData
{
    public string guid;
    public string typeId;

    public Vector2 worldPos;
    public float worldRotZ;

    public int hp;
    public FactionType faction;
}

#endregion
