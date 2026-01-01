using UnityEngine;

public class ShipSaveLoadDriver : MonoBehaviour
{
    [SerializeField] private CoreModule core;              // 현재 함선 코어
    [SerializeField] private Transform loadContainerRoot;  // 로드시 스폰될 부모(예: ModulesContainer)

    private IModulePrefabResolver resolver;

    private void Awake()
    {
        resolver = new ResourcesModulePrefabResolver("Modules/");
    }

    public void Save()
    {
        var data = ShipSaveLoad.SaveFromCore(core);
        var json = ShipSaveLoad.ToJson(data, true);
        PlayerPrefs.SetString("SHIP_SAVE", json);
        PlayerPrefs.Save();
        Debug.Log(json);
    }

    public void Load()
    {
        var json = PlayerPrefs.GetString("SHIP_SAVE", "");
        if (string.IsNullOrWhiteSpace(json)) return;

        var data = ShipSaveLoad.FromJson(json);
        ShipSaveLoad.LoadToContainer(loadContainerRoot, data, resolver);
    }
}
