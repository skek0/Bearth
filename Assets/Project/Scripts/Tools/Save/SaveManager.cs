using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public static class SceneSaveLoad
{
    public static string DefaultPath =>
        Path.Combine(Application.persistentDataPath, "scene_save.json");

    public static void SaveToFile(string path = null)
    {
        path ??= DefaultPath;

        var data = SaveSceneData();
        var json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path, json);
        Debug.Log($"[SceneSave] Saved: {path}");
    }

    public static void LoadFromFile(
        Transform shipsRoot,
        Transform looseRoot,
        IModulePrefabResolver resolver,
        string path = null)
    {
        path ??= DefaultPath;

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[SceneLoad] Save file not found: {path}");
            return;
        }

        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<SceneSaveData>(json);

        LoadSceneData(shipsRoot, looseRoot, data, resolver);
        Debug.Log($"[SceneLoad] Loaded: {path}");
    }

    // =========================
    // SAVE: 씬 전체
    // =========================
    public static SceneSaveData SaveSceneData()
    {
        var scene = new SceneSaveData();

        var cores = Object.FindObjectsByType<CoreModule>(FindObjectsSortMode.None);

        // ship 소속 판정용
        var inAnyShip = new HashSet<Module>();

        foreach (var core in cores)
        {
            if (core == null) continue;

            var ship = ShipSaveLoad.SaveFromCore(core);

            foreach (var m in core.GetComponentsInChildren<Module>(true))
                inAnyShip.Add(m);

            scene.ships.Add(new ShipInstanceSaveData
            {
                shipId = ship.coreGuid,
                worldPos = core.transform.position,
                worldRotZ = core.transform.eulerAngles.z,
                ship = ship,
                vel = core.Rigid.linearVelocity,
                angVel = core.Rigid.angularVelocity
            });
        }

        // 루즈 모듈
        var allModules = Object.FindObjectsByType<Module>(FindObjectsSortMode.None);

        foreach (var m in allModules)
        {
            if (m == null) continue;

            if (inAnyShip.Contains(m)) continue;
            if (m is CoreModule) continue;

            if (m == null || string.IsNullOrWhiteSpace(m.TypeId))
                Debug.LogError($"[SceneSave] TypeId missing/empty on {m.name}", m);

            scene.looseModules.Add(new WorldModuleSaveData
            {
                guid = m.ModuleGuid.Guid,
                typeId = m.TypeId ?? "",
                moduleId = m.ModuleId,
                worldPos = m.transform.position,
                worldRotZ = m.transform.eulerAngles.z,
                hp = m.Hp,
                faction = m.Faction,
                vel = m.Rigid ? m.Rigid.linearVelocity : Vector2.zero,
                angVel = m ? m.Rigid.angularVelocity : 0f,
            });
            Debug.Log(m.TypeId);
        }

        return scene;
    }

    // =========================
    // LOAD: 씬 전체
    // =========================
    public static void LoadSceneData(
        Transform shipsRoot,
        Transform looseRoot,
        SceneSaveData data,
        IModulePrefabResolver resolver)
    {
        if (shipsRoot == null) { Debug.LogError("[SceneLoad] shipsRoot is null"); return; }
        if (looseRoot == null) { Debug.LogError("[SceneLoad] looseRoot is null"); return; }
        if (resolver == null) { Debug.LogError("[SceneLoad] resolver is null"); return; }
        if (data == null) { Debug.LogError("[SceneLoad] data is null"); return; }

        ClearChildren(shipsRoot);
        ClearChildren(looseRoot);

        // 1) ships
        foreach (var shipInst in data.ships)
        {
            ShipSaveLoad.LoadShipToFleetRoot(shipsRoot, shipInst, resolver);
        }

        // 2) loose
        foreach (var m in data.looseModules)
        {
            var prefab = resolver.Resolve(m.typeId);
            if (prefab == null)
            {
                Debug.LogError($"[SceneLoad] Prefab not found typeId='{m.typeId}'");
                continue;
            }

            var go = Object.Instantiate(prefab, looseRoot);
            var module = go.GetComponent<Module>();
            if (module == null)
            {
                Debug.LogError($"[SceneLoad] Module missing on prefab typeId='{m.typeId}'");
                Object.Destroy(go);
                continue;
            }

            if (!module.TryGetComponent<ModuleGuid>(out var guid)) guid = module.gameObject.AddComponent<ModuleGuid>();
            guid.SetGuid(m.guid);

            module.transform.SetPositionAndRotation(
                new Vector3(m.worldPos.x, m.worldPos.y, 0f), 
                Quaternion.Euler(0, 0, m.worldRotZ));
            module.Faction = m.faction; 
            module.SetModuleId(m.moduleId);
            module.ApplyBaseStat(ModuleSpecDB.BaseStats[m.moduleId]);

            if (module.TryGetComponent<Rigidbody2D>(out var rb))
            {
                rb.linearVelocity = m.vel;
                rb.angularVelocity = m.angVel;
            }
        }
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.Destroy(root.GetChild(i).gameObject);
    }
}
