using System;
using System.Collections.Generic;
using UnityEngine;

#region Ship Save (single ship)

[Serializable]
public class ShipSaveData
{
    public int version = 1;
    public string coreGuid;               // 코어 guid
    public List<ModuleSaveData> modules = new();
    public List<LinkSaveData> links = new();
}

[Serializable]
public class ModuleSaveData
{
    public string guid;
    public string typeId;

    public Vector2 localPos;
    public float localRotZ;

    public int hp;
    public FactionType faction;

    public Vector2 vel;
    public float angVel;
}

[Serializable]
public class LinkSaveData
{
    public string childGuid;     // BaseModule
    public string parentGuid;    // Module(코어 포함)
    public string parentPortId;  // 부모 커넥터 portId
}

#endregion
