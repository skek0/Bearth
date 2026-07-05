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
        var json = JsonUtility.ToJson(data, true);

        File.WriteAllText(path ?? DefaultPath, json);
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

        var json = File.ReadAllText(path);
        var data = JsonUtility.FromJson<PlayerSaveData>(json);

        ClearChildren(playerRoot);
        ShipSaveLoad.LoadShipToRoot(playerRoot, data.ship, respawnPoint.position, respawnPoint.eulerAngles.z);

        Debug.Log($"[PlayerLoad] Loaded: {path}");
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            Object.Destroy(root.GetChild(i).gameObject);
    }
}