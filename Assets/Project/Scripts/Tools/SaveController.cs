using UnityEngine;

public interface IModulePrefabResolver
{
    GameObject Resolve(string typeId);
}

public sealed class ResourcesModulePrefabResolver : IModulePrefabResolver
{
    private readonly string root;
    public ResourcesModulePrefabResolver(string root = "BaseStat/") => this.root = root;
    public GameObject Resolve(string typeId) => Resources.Load<GameObject>(root + typeId);
}

public class SaveController : MonoBehaviour
{
    [SerializeField] string PrefabsPath;
    [SerializeField] Transform shipsRoot;
    [SerializeField] Transform looseRoot;

    IModulePrefabResolver resolver;

    private void Awake()
    {
        resolver = new ResourcesModulePrefabResolver(PrefabsPath);
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
