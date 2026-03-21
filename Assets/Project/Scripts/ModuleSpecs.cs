using UnityEngine;

public class ModuleSpecs : MonoBehaviour
{
    [Header("Resources paths (no extension)")]
    [SerializeField] private string baseStatPath = "ModuleSpecs/BaseStats";
    [SerializeField] private string rangedWeaponPath = "ModuleSpecs/WeaponRangedStats";
    [SerializeField] private string schematicPath = "Schematics";

    private void Awake()
    {
        Load(baseStatPath, ModuleSpecDB.LoadBaseStats);
        Load(rangedWeaponPath, ModuleSpecDB.LoadWeaponRangedStats);
        ModuleSpecDB.LoadSchematics(schematicPath);

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
