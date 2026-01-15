using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SceneSaveLoad
{
    public static string DefaultPath => Path.Combine(Application.persistentDataPath, "scene_save.json");

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
    // Pure logic
    // =========================
    public static SceneSaveData SaveSceneData()
    {
        var scene = new SceneSaveData();

        // 1) 씬의 모든 코어(= 우주선들)
        var cores = Object.FindObjectsByType<CoreModule>(FindObjectsSortMode.None);

        // ship 소속 모듈 판정용
        var inAnyShip = new HashSet<Module>();

        foreach (var core in cores)
        {
            if (core == null) continue;

            var shipData = ShipSaveLoad.SaveFromCore(core);

            // 소속 마킹
            foreach (var m in core.GetComponentsInChildren<Module>(true))
                inAnyShip.Add(m);

            scene.ships.Add(new ShipInstanceSaveData
            {
                shipId = shipData.coreGuid,
                worldPos = core.transform.position,
                worldRotZ = core.transform.eulerAngles.z,
                ship = shipData
            });
        }

        // 2) 루즈 모듈(어떤 코어 자식도 아닌 Module)
        var allModules = Object.FindObjectsByType<Module>(FindObjectsSortMode.None);

        foreach (var m in allModules)
        {
            if (m == null) continue;

            // ship에 속한 애들은 제외
            if (inAnyShip.Contains(m)) continue;

            // 코어는 제외(일반적으로 위에서 걸러지지만 안전 처리)
            if (m is CoreModule) continue;

            // guid/typeId 보장
            var g = m.GetComponent<ModuleGuid>();
            if (g == null) g = m.gameObject.AddComponent<ModuleGuid>();

            var t = m.GetComponent<ModuleTypeId>();
            if (t == null || string.IsNullOrWhiteSpace(t.TypeId))
                Debug.LogError($"[SceneSave] ModuleTypeId missing/empty on {m.name}", m);

            scene.looseModules.Add(new WorldModuleSaveData
            {
                guid = g.Guid,
                typeId = t?.TypeId ?? "",
                worldPos = m.transform.position,
                worldRotZ = m.transform.eulerAngles.z,
                hp = m.Hp,
                faction = m.Faction
            });
        }

        return scene;
    }

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

        // 0) 기존 삭제
        ClearChildren(shipsRoot);
        ClearChildren(looseRoot);

        // 1) 우주선들 로드
        foreach (var shipInst in data.ships)
        {
            var shipRoot = new GameObject($"Ship_{shipInst.shipId}").transform;
            shipRoot.SetParent(shipsRoot, false);

            // 내부 모듈 로드(로컬 배치)
            ShipSaveLoad.LoadToContainer(shipRoot, shipInst.ship, resolver);

            // 우주선 루트 월드 포즈
            shipRoot.SetPositionAndRotation(
                new Vector3(shipInst.worldPos.x, shipInst.worldPos.y, 0f), 
                Quaternion.Euler(0, 0, shipInst.worldRotZ)
                );
        }

        // 2) 루즈 모듈 로드
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

            // guid 주입
            if (!module.TryGetComponent<ModuleGuid>(out var guid)) guid = module.gameObject.AddComponent<ModuleGuid>();
            guid.SetGuid(m.guid);

            // 포즈/상태
            module.transform.position = new Vector3(m.worldPos.x, m.worldPos.y, 0f);
            module.transform.rotation = Quaternion.Euler(0, 0, m.worldRotZ);

            module.Hp = m.hp;
            module.Faction = m.faction;
        }
    }

    // =========================
    // Helpers
    // =========================
    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.Destroy(root.GetChild(i).gameObject);
    }
}
