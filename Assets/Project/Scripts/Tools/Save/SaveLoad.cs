using System.IO;
using UnityEngine;

public static class SceneSaveLoad
{
    public static string DefaultPath =>
        Path.Combine(Application.persistentDataPath, "player_save.json");
    public static void SaveToFile(string path = null)
    {
        var playerGo = GameObject.FindWithTag("Player");
        if (playerGo == null || !playerGo.TryGetComponent<CoreModule>(out var playerCore))
        {
            Debug.LogError("[PlayerSave] Player core not found.");
            return;
        }

        var data = new PlayerSaveData { ship = ShipSaveLoad.SaveFromCore(playerCore) };
        File.WriteAllText(path ?? DefaultPath, JsonUtility.ToJson(data, true));
        Debug.Log($"[PlayerSave] Saved: {path ?? DefaultPath}");
    }

    public static void LoadFromFile(Transform playerRoot, Transform respawnPoint, string path = null)
    {
        path ??= DefaultPath;

        if (!File.Exists(path))
        {
            Debug.LogWarning($"[PlayerLoad] Save file not found: {path}");
            return;
        }

        var data = JsonUtility.FromJson<PlayerSaveData>(File.ReadAllText(path));

        ClearChildren(playerRoot);
        ShipSaveLoad.LoadShipToRoot(playerRoot, data.ship, respawnPoint.position);

        Debug.Log($"[PlayerLoad] Loaded: {path}");
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.Destroy(root.GetChild(i).gameObject);
    }
    public static void Save1ShipToFile(string shipId)
    {
        var cores = GameObject.FindObjectsByType<CoreModule>(FindObjectsSortMode.None);
        if (cores.Length == 0)
        {
            Debug.LogError("[PlayerSave] CoreModule not found.");
            return;
        }

        if (cores.Length > 1)
        {
            Debug.LogError($"[PlayerSave] Multiple CoreModules found. Count: {cores.Length}");
            return;
        }

        var data = new PlayerSaveData { ship = ShipSaveLoad.SaveFromCore(cores[0]) };
        string savePath = Path.Combine(Application.persistentDataPath, $"{shipId}.json");
        File.WriteAllText(savePath, JsonUtility.ToJson(data, true));

        Debug.Log($"[PlayerSave] Saved: {savePath}");
    }
}