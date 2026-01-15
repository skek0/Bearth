using UnityEngine;

public class SaveController : MonoBehaviour
{
    [SerializeField] Transform shipsRoot;
    [SerializeField] Transform looseRoot;

    IModulePrefabResolver resolver;

    private void Awake()
    {
        resolver = new ResourcesModulePrefabResolver("Modules/");
    }

    public void Save()
    {
        SceneSaveLoad.SaveToFile(); // 기본 경로
    }

    public void Load()
    {
        SceneSaveLoad.LoadFromFile(shipsRoot, looseRoot, resolver);
    }
}
