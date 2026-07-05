using System;
using System.Collections.Generic;
using UnityEngine;

#region Ship Save (single ship)

[Serializable]
public class ShipSaveData
{
    public int version = 1;
    public string coreGuid;
    public List<ModuleSaveData> modules = new();
    public List<LinkSaveData> links = new();
}

[Serializable]
public class ModuleSaveData
{
    public string guid;
    public string moduleId;

    public Vector2 localPos;
    public float localRotZ;

    public int hp;
    public FactionType faction;
}

[Serializable]
public class LinkSaveData
{
    public string childGuid;
    public string parentGuid;
    public string parentPortId;
}

#endregion