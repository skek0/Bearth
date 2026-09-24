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
        var ship = ShipSaveLoad.SaveFromCore(playerCore);
        File.WriteAllText(path ?? DefaultPath, JsonUtility.ToJson(ship, true));
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

        var ship = JsonUtility.FromJson<ShipSaveData>(File.ReadAllText(path));
        ClearChildren(playerRoot); 
        ShipSaveLoad.LoadShipToRoot(playerRoot, ship, respawnPoint.position);

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
        if (cores.Length != 1)
        {
            Debug.LogError("CoreModule not found.");
            return;
        }

        if (cores[0] is PlayerModule)
        {
            Debug.LogError("[ShipEditor] 이 함수는 적 함선 전용입니다. 플레이어 함선은 저장 대상이 아닙니다.");
            return;
        }

        string savePath = Path.Combine(Application.dataPath, "Resources", "Enemies", $"{shipId}.json");

        Directory.CreateDirectory(Path.GetDirectoryName(savePath));

        var ship = ShipSaveLoad.SaveFromCore(cores[0]);
        File.WriteAllText(savePath, JsonUtility.ToJson(ship, true));

        Debug.Log($"[PlayerSave] Saved: {savePath}");
    }
}