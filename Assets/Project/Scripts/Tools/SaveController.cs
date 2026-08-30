using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveController : SceneSingleton<SaveController>
{
    [SerializeField] Transform playerRoot;
    [SerializeField] Transform respawnPoint;
    [SerializeField] TextMeshProUGUI shipId;

    public void Save() => SceneSaveLoad.SaveToFile();
    public void Load() => SceneSaveLoad.LoadFromFile(playerRoot, respawnPoint);
    public void SaveShip() => SceneSaveLoad.Save1ShipToFile(shipId.text);
}