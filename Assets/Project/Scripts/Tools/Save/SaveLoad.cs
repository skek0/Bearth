using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public static class SceneSaveLoad
{
    public static string DefaultPath =>
        Path.Combine(Application.persistentDataPath, "player_save.json");
    public static void SavePlayerShip(string path = null)
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

    public static void LoadPlayerShip(Transform playerRoot, Transform respawnPoint, string path = null)
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
    public static void Save1EnemyShip()
    {
        var cores = GameObject.FindObjectsByType<CoreModule>(FindObjectsSortMode.None);
        if (cores.Length != 1)
        {
            Debug.LogError("CoreModule not found.");
            return;
        }

        var core = cores[0];
        if (core is PlayerModule)
        {
            Debug.LogError("[ShipEditor] 이 함수는 적 함선 전용입니다. 플레이어 함선은 저장 대상이 아닙니다.");
            return;
        }

        // 검사 통과 후에만 팝업 오픈
        var popupObj = UIManager.Instance.ShowPopup(UIManager.PopupType.SaveName);
        if (popupObj == null || !popupObj.TryGetComponent(out EnterNamePopup popup)) return;

        popup.Confirmed += OnConfirmed;

        void OnConfirmed(string shipId)
        {
            popup.Confirmed -= OnConfirmed;
            if (core == null) return;
            WriteEnemyShip(core, shipId);
        }
    }
    private static void WriteEnemyShip(CoreModule core, string rawShipId)
    {
        // 입력창 값 정리: 앞뒤 공백 제거 + 빈 값 거부
        string shipId = rawShipId?.Trim();
        if (string.IsNullOrEmpty(shipId))
        {
            Debug.LogError("[ShipEditor] 함선 이름이 비어 있어 저장을 취소했습니다.");
            return;
        }

        string savePath = ShipSpecDB.FilePath;
        Directory.CreateDirectory(Path.GetDirectoryName(savePath));

        // 기존 파일 읽기 (파싱 실패 시 덮어쓰지 않고 중단)
        var entries = new List<ShipEntry>();
        if (File.Exists(savePath))
        {
            try
            {
                entries = JsonConvert.DeserializeObject<List<ShipEntry>>(File.ReadAllText(savePath))
                          ?? new List<ShipEntry>();
            }
            catch (Exception e)
            {
                Debug.LogError($"[ShipEditor] 기존 파일 파싱 실패, 저장 중단: {e.Message}");
                return;
            }
        }

        var entry = new ShipEntry
        {
            ShipId = shipId,
            Data = ShipSaveLoad.SaveFromCore(core)
        };

        int idx = entries.FindIndex(e => e.ShipId == shipId);
        bool overwritten = idx >= 0;
        if (overwritten) entries[idx] = entry;
        else entries.Add(entry);

        // 직렬화가 성공한 뒤에만 파일에 쓰기
        string json = JsonConvert.SerializeObject(entries, Formatting.Indented);
        File.WriteAllText(savePath, json);

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.Refresh();
#endif

        Debug.Log($"[ShipEditor] {(overwritten ? "Overwritten" : "Saved")} '{shipId}': {savePath}");
    }

    private static void ClearChildren(Transform root)
    {
        for (int i = root.childCount - 1; i >= 0; i--)
            UnityEngine.Object.Destroy(root.GetChild(i).gameObject);
    }
}