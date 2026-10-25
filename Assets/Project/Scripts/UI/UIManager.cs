using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 씬 전역에서 팝업 UI를 등록/관리하는 싱글톤 매니저
/// </summary>
public class UIManager : GlobalSingleton<UIManager>
{
    // 관리할 팝업 종류. 새 팝업이 생기면 여기에 enum만 추가하면 됨.
    public enum PopupType
    {
        SaveName,
        // Confirm,
        // Settings,
    }

    [Serializable]
    private struct PopupEntry
    {
        public PopupType type;
        public GameObject prefab;
    }

    [Header("Popup Prefabs")]
    [SerializeField] private List<PopupEntry> popupEntries;

    [Header("팝업이 생성될 부모 (Canvas)")]
    [SerializeField] private Transform popupRoot;

    private readonly Dictionary<PopupType, GameObject> _prefabMap = new();
    private readonly Dictionary<PopupType, GameObject> _activeInstances = new();

    protected override void Awake()
    {
        base.Awake();
        foreach (var entry in popupEntries)
        {
            _prefabMap[entry.type] = entry.prefab;
        }
    }

    /// <summary>
    /// 팝업을 생성(또는 재사용)하여 반환한다. 필요한 만큼 GetComponent로 캐스팅해서 사용.
    /// </summary>
    public GameObject ShowPopup(PopupType type)
    {
        if (!_prefabMap.TryGetValue(type, out var prefab))
        {
            Debug.LogError($"[UIManager] {type}에 등록된 프리팹이 없습니다.");
            return null;
        }

        if (_activeInstances.TryGetValue(type, out var existing) && existing != null)
        {
            existing.SetActive(true);
            return existing;
        }

        var instance = Instantiate(prefab, popupRoot);
        _activeInstances[type] = instance;
        return instance;
    }

    public void HidePopup(PopupType type)
    {
        if (_activeInstances.TryGetValue(type, out var instance) && instance != null)
        {
            instance.SetActive(false);
        }
    }
}
