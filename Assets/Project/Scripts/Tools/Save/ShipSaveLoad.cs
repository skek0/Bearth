using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public static class ShipSaveLoad
{
    // =========================
    // SAVE (core 기준 로컬)
    // =========================
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

        var guidByModule = new Dictionary<Module, string>(allModules.Count);
        foreach (var m in allModules)
            guidByModule[m] = m.GetComponent<ModuleGuid>().Guid;

        data.coreGuid = guidByModule[core];

        var coreT = core.transform;

        foreach (var m in allModules)
        {
            var guid = guidByModule[m];

            Vector2 localPos = coreT.InverseTransformPoint(m.transform.position);
            float localRotZ = (Quaternion.Inverse(coreT.rotation) * m.transform.rotation).eulerAngles.z;

            data.modules.Add(new ModuleSaveData
            {
                guid = guid,
                moduleId = m.ModuleId,
                localPos = localPos,
                localRotZ = localRotZ,
                hp = m.Hp,
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
                childGuid = child.GetComponent<ModuleGuid>().Guid,
                parentGuid = guidByModule[parent],
                parentPortId = portId
            });
        }

        return data;
    }

    // =========================
    // LOAD (리스폰 지점 기준, ModuleMaker 경유)
    // =========================
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

        var coreEntry = data.modules.FirstOrDefault(m => m.guid == data.coreGuid);
        if (coreEntry == null)
        {
            Debug.LogError("[Load] core entry missing");
            return null;
        }

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

        core.ModuleGuid.SetGuid(coreEntry.guid);
        coreGo.transform.SetPositionAndRotation(spawnPos, Quaternion.identity);
        core.Hp = coreEntry.hp;
        core.Faction = coreEntry.faction;

        CameraRebinder.BindTo(core.transform);

        var map = new Dictionary<string, Module>(data.modules.Count)
        {
            [coreEntry.guid] = core
        };

        SetPhysicsEnabled(core.gameObject, false);

        foreach (var m in data.modules)
        {
            if (m.guid == data.coreGuid) continue;

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
            module.ModuleGuid.SetGuid(m.guid);

            Vector3 worldPos = core.transform.TransformPoint(m.localPos);
            Quaternion worldRot = core.transform.rotation * Quaternion.Euler(0, 0, m.localRotZ);
            module.transform.SetPositionAndRotation(worldPos, worldRot);

            module.Hp = m.hp;
            module.Faction = m.faction;

            SetPhysicsEnabled(go, false);

            map[m.guid] = module;
        }

        RestoreLinksBfs(data, map);

        foreach (var kv in map)
            SetPhysicsEnabled(kv.Value.gameObject, true);

        return core;
    }

    private static void RestoreLinksBfs(ShipSaveData data, Dictionary<string, Module> map)
    {
        var childrenByParent = new Dictionary<string, List<LinkSaveData>>();
        foreach (var link in data.links)
        {
            if (!childrenByParent.TryGetValue(link.parentGuid, out var list))
                childrenByParent[link.parentGuid] = list = new List<LinkSaveData>();
            list.Add(link);
        }

        var q = new Queue<string>();
        q.Enqueue(data.coreGuid);

        while (q.Count > 0)
        {
            var parentGuid = q.Dequeue();
            if (!childrenByParent.TryGetValue(parentGuid, out var children)) continue;

            foreach (var link in children)
            {
                if (!map.TryGetValue(link.childGuid, out var childModule)) continue;
                if (!map.TryGetValue(link.parentGuid, out var parentModule)) continue;
                if (childModule is not BaseModule child) continue;

                var parentConnector = FindConnectorByPortId(parentModule.transform, link.parentPortId);
                if (parentConnector == null)
                {
                    Debug.LogError($"[Load] Missing connector '{link.parentPortId}' on parent '{parentModule.name}'", parentModule);
                    continue;
                }

                child.LoadAttach(parentConnector);
                q.Enqueue(link.childGuid);
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