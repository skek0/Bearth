using UnityEngine;

public static class ModuleSpecs
{
    private const string baseStatPath = "ModuleSpecs/BaseStats";
    private const string rangedWeaponPath = "ModuleSpecs/WeaponRangedStats";

    public static void LoadSpecs()
    {
        Load(baseStatPath, ModuleSpecDB.LoadBaseStats);
        Load(rangedWeaponPath, ModuleSpecDB.LoadWeaponRangedStats);
    }

    private static void Load(string path, System.Action<string> load)
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
