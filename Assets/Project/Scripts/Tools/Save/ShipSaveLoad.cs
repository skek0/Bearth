using System.Collections;
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

        // 코어 포함 전체 모듈 수집
        var allModules = core.GetComponentsInChildren<Module>(true).ToList();
        if (!allModules.Contains(core))
            allModules.Insert(0, core);

        // 코어 먼저
        allModules = allModules
            .OrderByDescending(m => m is CoreModule)
            .ThenBy(m => m.name)
            .ToList();

        // guid 보장
        var guidByModule = new Dictionary<Module, string>(allModules.Count);
        foreach (var m in allModules)
        {
            var g = m.GetComponent<ModuleGuid>();


            guidByModule[m] = g.Guid;
        }

        data.coreGuid = guidByModule[core];

        var coreT = core.transform;

        // modules 저장
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

    // =========================
    // LOAD
    // =========================
    public static CoreModule LoadShipToFleetRoot(
        Transform fleetRoot,
        ShipInstanceSaveData shipInst,
        IModulePrefabResolver resolver)
    {
        var data = shipInst.ship;
        if (data == null || data.modules == null || data.modules.Count == 0)
        {
            Debug.LogError("[Load] ShipSaveData empty");
            return null;
        }

        // 1) 코어 엔트리 찾기
        var coreEntry = data.modules.FirstOrDefault(m => m.guid == data.coreGuid);
        if (coreEntry == null)
        {
            Debug.LogError("[Load] core entry missing");
            return null;
        }

        // 2) 코어 스폰
        var corePrefab = resolver.Resolve(coreEntry.typeId);
        if (corePrefab == null)
        {
            Debug.LogError($"[Load] Core prefab not found moduleId='{coreEntry.typeId}'");
            return null;
        }

        var coreGo = Object.Instantiate(corePrefab, fleetRoot);
        var core = coreGo.GetComponent<CoreModule>();
        if (core == null)
        {
            Debug.LogError($"[Load] CoreModule missing on prefab moduleId='{coreEntry.typeId}'");
            Object.Destroy(coreGo);
            return null;
        }

        if (!core.TryGetComponent<ModuleGuid>(out var coreGuid)) coreGuid = core.gameObject.AddComponent<ModuleGuid>();
        coreGuid.SetGuid(coreEntry.guid);

        core.transform.SetPositionAndRotation(new Vector3(shipInst.worldPos.x, shipInst.worldPos.y, 0f), Quaternion.Euler(0, 0, shipInst.worldRotZ));

        // 상태
        core.Hp = coreEntry.hp;
        core.Faction = coreEntry.faction;
        core.SetModuleId(coreEntry.moduleId);
        core.ApplyBaseStat(ModuleSpecDB.BaseStats[coreEntry.moduleId]);


        if (core.CompareTag("Player")) CameraRebinder.BindTo(core.transform);

        // 3) 나머지 모듈 스폰
        var map = new Dictionary<string, Module>(data.modules.Count)
        {
            [coreEntry.guid] = core
        };

        // physics off (로드 중 충돌/튜닝 방지)
        SetPhysicsEnabled(core.gameObject, false);

        foreach (var m in data.modules)
        {
            if (m.guid == data.coreGuid) continue;

            var prefab = resolver.Resolve(m.typeId);
            if (prefab == null)
            {
                Debug.LogError($"[Load] Prefab not found moduleId='{m.typeId}'");
                continue;
            }

            var go = Object.Instantiate(prefab, fleetRoot);
            var module = go.GetComponent<Module>();
            if (module == null)
            {
                Debug.LogError($"[Load] Module missing on prefab moduleId='{m.typeId}'");
                Object.Destroy(go);
                continue;
            }

            // guid 주입
            if (!module.TryGetComponent<ModuleGuid>(out var guid)) guid = module.gameObject.AddComponent<ModuleGuid>();
            guid.SetGuid(m.guid);
            Debug.Log(m.guid);

            // 코어 기준 로컬 -> 월드로 환산해서 임시 배치
            Vector3 worldPos = core.transform.TransformPoint(m.localPos);
            Quaternion worldRot = core.transform.rotation * Quaternion.Euler(0, 0, m.localRotZ);

            module.transform.SetPositionAndRotation(worldPos, worldRot);

            module.Hp = m.hp;
            module.Faction = m.faction;
            module.SetModuleId(m.moduleId);
            module.ApplyBaseStat(ModuleSpecDB.BaseStats[m.moduleId]);

            SetPhysicsEnabled(go, false);

            map[m.guid] = module;
        }

        // 4) 링크 복원: 코어 -> 바깥 BFS (부모 먼저)
        RestoreLinksBfs(data, map);

        // 5) physics on
        foreach (var kv in map)
            SetPhysicsEnabled(kv.Value.gameObject, true);
        
        core.StartCoroutine(Co());

        IEnumerator Co()
        {
            yield return new WaitForFixedUpdate();
            if (core == null) yield break;
            if (core.Rigid == null) yield break;

            core.Rigid.linearVelocity = shipInst.vel;
            core.Rigid.angularVelocity = shipInst.angVel;
        }

        return core;
    }

    private static void RestoreLinksBfs(ShipSaveData data, Dictionary<string, Module> map)
    {
        // parentGuid -> children links
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
