using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IModulePrefabResolver
{
    GameObject Resolve(string typeId);
}

public sealed class ResourcesModulePrefabResolver : IModulePrefabResolver
{
    private readonly string root;
    public ResourcesModulePrefabResolver(string root = "Modules/") => this.root = root;
    public GameObject Resolve(string typeId) => Resources.Load<GameObject>(root + typeId);
}

public static class ShipSaveLoad
{
    // =========================
    // SAVE (core first)
    // =========================
    public static ShipSaveData SaveFromCore(CoreModule core)
    {
        var data = new ShipSaveData();

        var allModules = core.GetComponentsInChildren<Module>(true).ToList();
        if (!allModules.Contains(core))
            allModules.Insert(0, core);

        // 코어 먼저
        allModules = allModules
            .OrderByDescending(m => m is CoreModule)
            .ThenBy(m => m.name)
            .ToList();

        // guid/typeId 보장
        var guidByModule = new Dictionary<Module, string>(allModules.Count);
        foreach (var m in allModules)
        {
            var g = m.GetComponent<ModuleGuid>();
            if (g == null) g = m.gameObject.AddComponent<ModuleGuid>();

            var t = m.GetComponent<ModuleTypeId>();
            if (t == null || string.IsNullOrWhiteSpace(t.TypeId))
                Debug.LogError($"[Save] ModuleTypeId missing/empty on {m.name}", m);

            guidByModule[m] = g.Guid;
        }

        data.coreGuid = guidByModule[core];
        var coreT = core.transform;

        // modules
        foreach (var m in allModules)
        {
            var guid = guidByModule[m];
            var typeId = m.TryGetComponent<ModuleTypeId>(out var typeid) ? typeid.TypeId : "";

            data.modules.Add(new ModuleSaveData
            {
                guid = guid,
                typeId = typeId,

                localPos = coreT.InverseTransformPoint(m.transform.position),
                localRotZ = (Quaternion.Inverse(coreT.rotation) * m.transform.rotation).eulerAngles.z,

                hp = m.Hp,
                faction = m.Faction,
            });
        }

        // links: BaseModule 기준
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

    public static string ToJson(ShipSaveData data, bool pretty = true)
        => JsonUtility.ToJson(data, pretty);

    public static ShipSaveData FromJson(string json)
        => JsonUtility.FromJson<ShipSaveData>(json);

    // =========================
    // LOAD
    // =========================
    public static Dictionary<string, Module> LoadToContainer(
        Transform containerRoot,
        ShipSaveData data,
        IModulePrefabResolver resolver)
    {
        // 0) 컨테이너 비우기
        for (int i = containerRoot.childCount - 1; i >= 0; i--)
            Object.Destroy(containerRoot.GetChild(i).gameObject);

        // 1) 전부 스폰 + physics off
        var map = new Dictionary<string, Module>(data.modules.Count);

        foreach (var m in data.modules)
        {
            var prefab = resolver.Resolve(m.typeId);
            if (prefab == null)
            {
                Debug.LogError($"[Load] Prefab not found typeId='{m.typeId}'");
                continue;
            }

            var go = Object.Instantiate(prefab, containerRoot);
            var module = go.GetComponent<Module>();
            if (module == null)
            {
                Debug.LogError($"[Load] Module missing on prefab typeId='{m.typeId}'");
                Object.Destroy(go);
                continue;
            }

            // guid 주입
            if (!module.TryGetComponent<ModuleGuid>(out var guid)) guid = module.gameObject.AddComponent<ModuleGuid>();
            guid.SetGuid(m.guid);

            SetPhysicsEnabled(go, false);

            map[m.guid] = module;
        }

        // 2) 포즈 + 상태
        foreach (var m in data.modules)
        {
            if (!map.TryGetValue(m.guid, out var module)) continue;

            module.transform.localPosition = m.localPos;
            module.transform.localRotation = Quaternion.Euler(0, 0, m.localRotZ);

            module.Hp = m.hp;             // ✅ 리플렉션 제거
            module.Faction = m.faction;   // ✅ 리플렉션 제거
        }

        // 3) 링크 복원
        foreach (var link in data.links)
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
        }

        // 4) physics on
        foreach (var kv in map)
            SetPhysicsEnabled(kv.Value.gameObject, true);

        return map;
    }

    // =========================
    // Helpers
    // =========================
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
