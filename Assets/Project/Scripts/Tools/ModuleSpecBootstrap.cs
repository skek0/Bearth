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

        LoadAndInject(BaseStatPath, ModuleSpecDB.LoadBaseStats, "BaseStats");
        LoadAndInject(RangedWeaponPath, ModuleSpecDB.LoadWeaponRangedStats, "WeaponRangedStats");

        Debug.Log("ModuleSpecDB initialized before scene");
    }

    static void LoadAndInject(string path, System.Action<string> load, string tableName)
    {
        var ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogError($"[ModuleSpecBootstrap] {tableName}: TextAsset not found: Resources/{path}.json");
            return;
        }

        load(ta.text);
    }
}
