using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaveController : SceneSingleton<SaveController>
{
    [SerializeField] Transform playerRoot;
    [SerializeField] Transform respawnPoint;

    public void Save() => SceneSaveLoad.SavePlayerShip();
    public void Load() => SceneSaveLoad.LoadPlayerShip(playerRoot, respawnPoint);
    public void SaveShip() => SceneSaveLoad.Save1EnemyShip();
}