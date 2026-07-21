using UnityEngine;

public class SaveController : SceneSingleton<SaveController>
{
    [SerializeField] Transform playerRoot;
    [SerializeField] Transform respawnPoint;

    public void Save() => SceneSaveLoad.SaveToFile();
    public void Load() => SceneSaveLoad.LoadFromFile(playerRoot, respawnPoint);
}