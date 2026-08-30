using System;
using System.Collections.Generic;
using UnityEngine;

#region Ship Save (single ship)

[Serializable]
public class ShipSaveData
{
    public int version = 1;
    public List<ModuleSaveData> modules = new();
    public List<LinkSaveData> links = new();
}

[Serializable]
public class ModuleSaveData
{
    public string moduleId;
    public FactionType faction;
}

[Serializable]
public class LinkSaveData
{
    public int childIndex;
    public int parentIndex;
    public string parentPortId;
}

#endregion