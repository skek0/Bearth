using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ShipSaveLoad
{
    public static ShipSaveData SaveFromCore(CoreModule core)
    {
        var data = new ShipSaveData();

        var allModules = core.GetComponentsInChildren<Module>(true).ToList();
        if (!allModules.Contains(core))
            allModules.Insert(0, core);

        allModules = allModules
            .OrderByDescending(m => m is CoreModule)
            .ThenBy(m => m.name)
            .ToList();

        var indexByModule = new Dictionary<Module, int>(allModules.Count);
        for (int i = 0; i < allModules.Count; i++)
            indexByModule[allModules[i]] = i;

        var coreT = core.transform;

        foreach (var m in allModules)
        {
            data.modules.Add(new ModuleSaveData
            {
                moduleId = m.ModuleId,
                faction = m.Faction,
            });
        }

        foreach (var m in allModules)
        {
            if (m is not BaseModule child) continue;

            var parent = child.AttachedTo;
            if (parent == null) continue;

            var portId = child.AttachedParentPortId;
            if (string.IsNullOrWhiteSpace(portId)) continue;

            data.links.Add(new LinkSaveData
            {
                childIndex = indexByModule[child],
                parentIndex = indexByModule[parent],
                parentPortId = portId
            });
        }

        return data;
    }

    public static CoreModule LoadShipToRoot(
        Transform root,
        ShipSaveData data,
        Vector3 spawnPos)
    {
        if (data == null || data.modules == null || data.modules.Count == 0)
        {
            Debug.LogError("[Load] ShipSaveData empty");
            return null;
        }

        var coreEntry = data.modules[0];

        var coreGo = ModuleMaker.CreateModule(coreEntry.moduleId);
        if (coreGo == null)
        {
            Debug.LogError($"[Load] Failed to create core moduleId='{coreEntry.moduleId}'");
            return null;
        }

        var core = coreGo.GetComponent<CoreModule>();
        if (core == null)
        {
            Debug.LogError($"[Load] CoreModule missing on created object moduleId='{coreEntry.moduleId}'");
            Object.Destroy(coreGo);
            return null;
        }

        coreGo.transform.SetParent(root);
        core.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        core.Faction = coreEntry.faction;

        CameraRebinder.BindTo(core.transform);

        var moduleByIndex = new Module[data.modules.Count];
        moduleByIndex[0] = core;

        SetPhysicsEnabled(core.gameObject, false);

        for (int i = 1; i < data.modules.Count; i++)
        {
            var m = data.modules[i];

            var go = ModuleMaker.CreateModule(m.moduleId);
            if (go == null)
            {
                Debug.LogError($"[Load] Failed to create moduleId='{m.moduleId}'");
                continue;
            }

            var module = go.GetComponent<Module>();
            if (module == null)
            {
                Debug.LogError($"[Load] Module missing on created object moduleId='{m.moduleId}'");
                Object.Destroy(go);
                continue;
            }

            go.transform.SetParent(root);

            module.Faction = m.faction;

            SetPhysicsEnabled(go, false);

            moduleByIndex[i] = module;
        }

        RestoreLinksBfs(data, moduleByIndex);

        foreach (var mod in moduleByIndex)
            if (mod != null) SetPhysicsEnabled(mod.gameObject, true);

        return core;
    }

    private static void RestoreLinksBfs(ShipSaveData data, Module[] moduleByIndex)
    {
        var childrenByParent = new Dictionary<int, List<LinkSaveData>>();
        foreach (var link in data.links)
        {
            if (!childrenByParent.TryGetValue(link.parentIndex, out var list))
                childrenByParent[link.parentIndex] = list = new List<LinkSaveData>();
            list.Add(link);
        }

        var q = new Queue<int>();
        q.Enqueue(0); // core는 항상 index 0

        while (q.Count > 0)
        {
            var parentIndex = q.Dequeue();
            if (!childrenByParent.TryGetValue(parentIndex, out var children)) continue;

            foreach (var link in children)
            {
                var childModule = moduleByIndex[link.childIndex];
                var parentModule = moduleByIndex[link.parentIndex];
                if (childModule == null || parentModule == null) continue;
                if (childModule is not BaseModule child) continue;

                var parentConnector = FindConnectorByPortId(parentModule.transform, link.parentPortId);
                if (parentConnector == null)
                {
                    Debug.LogError($"[Load] Missing connector '{link.parentPortId}' on parent '{parentModule.name}'", parentModule);
                    continue;
                }

                child.LoadAttach(parentConnector);
                q.Enqueue(link.childIndex);
            }
        }
    }

    private static Transform FindConnectorByPortId(Transform parentModuleRoot, string portId)
    {
        if (string.IsNullOrWhiteSpace(portId)) return null;

        var ports = parentModuleRoot.GetComponentsInChildren<ConnectorPort>(true);
        foreach (var p in ports)
            if (p.PortId == portId) return p.transform;

        return null;
    }

    private static void SetPhysicsEnabled(GameObject go, bool enabled)
    {
        foreach (var c in go.GetComponentsInChildren<Collider2D>(true))
            c.enabled = enabled;

        foreach (var r in go.GetComponentsInChildren<Rigidbody2D>(true))
            r.simulated = enabled;
    }
}