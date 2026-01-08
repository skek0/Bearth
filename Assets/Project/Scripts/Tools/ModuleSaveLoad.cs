using System.Collections.Generic;
using System.Linq;
using System.Reflection;
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

        // 코어 포함 전체 모듈 수집
        var allModules = core.GetComponentsInChildren<Module>(true).ToList();

        // 코어가 리스트에 없을 수 없지만, 안전하게 보정
        if (!allModules.Contains(core))
            allModules.Insert(0, core);

        // ✅ 코어 먼저 저장(정렬)
        allModules = allModules
            .OrderByDescending(m => m is CoreModule) // CoreModule 먼저
            .ThenBy(m => m.name)
            .ToList();

        // guid/typeId 보장 + coreGuid 기록
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

        // modules 저장
        foreach (var m in allModules)
        {
            var guid = guidByModule[m];
            var typeId = m.GetComponent<ModuleTypeId>()?.TypeId ?? "";

            data.modules.Add(new ModuleSaveData
            {
                guid = guid,
                typeId = typeId,
                localPos = m.transform.localPosition,
                localRotZ = m.transform.localEulerAngles.z,
                hp = GetHp(m),
                faction = GetFaction(m),
            });
        }

        // links 저장: BaseModule 기준, AttachedTo 기반
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

        // 1) 전부 스폰(코어 포함) + physics off
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

        // 2) 포즈 + 상태 적용
        foreach (var m in data.modules)
        {
            if (!map.TryGetValue(m.guid, out var module)) continue;

            module.transform.localPosition = m.localPos;
            module.transform.localRotation = Quaternion.Euler(0, 0, m.localRotZ);

            SetHp(module, m.hp);
            SetFaction(module, m.faction);
        }

        // 3) 링크 복원: child.LoadAttach(parentConnector)
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

    // ---- Module protected 필드 접근(리플렉션) ----
    // 네 코드 변경 최소화를 위해 이렇게 했고,
    // 원하면 Module에 public getter/setter 추가로 리플렉션 제거 가능.
    private static readonly FieldInfo HpField =
        typeof(Module).GetField("hp", BindingFlags.Instance | BindingFlags.NonPublic);

    private static readonly FieldInfo FactionField =
        typeof(Module).GetField("faction", BindingFlags.Instance | BindingFlags.NonPublic);

    private static int GetHp(Module m) => HpField != null ? (int)HpField.GetValue(m) : 0;
    private static void SetHp(Module m, int hp) { if (HpField != null) HpField.SetValue(m, hp); }

    private static FactionType GetFaction(Module m) =>
        FactionField != null ? (FactionType)FactionField.GetValue(m) : FactionType.Neutral;

    private static void SetFaction(Module m, FactionType f)
    {
        if (FactionField != null) FactionField.SetValue(m, f);
    }
}