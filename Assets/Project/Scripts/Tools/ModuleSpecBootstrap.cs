using UnityEngine;

public static class ModuleSpecBootstrap
{
    // ModuleSpecs에서 쓰던 경로를 그대로 사용
    const string BaseStatPath = "ModuleSpecs/BaseStats";
    const string RangedWeaponPath = "ModuleSpecs/WeaponRangedStats";

    static bool initialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    static void InitBeforeSceneLoad()
    {
        if (initialized) return;
        initialized = true;

        TempLoad_BeforeScene(BaseStatPath, ModuleSpecDB.LoadBaseStats);
        TempLoad_BeforeScene(RangedWeaponPath, ModuleSpecDB.LoadWeaponRangedStats);
        ModuleSpecDB.LoadSchematics();

        Debug.Log("ModuleSpecDB initialized before scene");
    }

    static void TempLoad_BeforeScene(string path, System.Action<string> load)
    {
        var ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogError($"Asset not found: Resources/{path}.json");
            return;
        }

        load(ta.text);
    }
}
