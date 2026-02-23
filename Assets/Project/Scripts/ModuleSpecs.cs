using UnityEngine;

public class ModuleSpecs : MonoBehaviour
{
    [Header("Resources paths (no extension)")]
    [SerializeField] private string baseStatPath = "ModuleSpecs/BaseStats";
    [SerializeField] private string rangedWeaponPath = "ModuleSpecs/WeaponRangedStats";

    private void Awake()
    {
        Load(baseStatPath, ModuleSpecDB.LoadBaseStats, "BaseStats");
        Load(rangedWeaponPath, ModuleSpecDB.LoadWeaponRangedStats, "WeaponRangedStats");
    }

    private static void Load(string path, System.Action<string> load, string tableName)
    {
        var ta = Resources.Load<TextAsset>(path);
        if (ta == null)
        {
            Debug.LogError($"[ModuleSpecs] {tableName}: TextAsset not found: Resources/{path}.json");
            return;
        }

        load(ta.text);
    }
}
