using UnityEngine;

public class SaveController : MonoBehaviour
{
    [SerializeField] Transform playerRoot;
    [SerializeField] Transform respawnPoint;

    public void Save() => SceneSaveLoad.SaveToFile();
    public void Load() => SceneSaveLoad.LoadFromFile(playerRoot, respawnPoint);
}