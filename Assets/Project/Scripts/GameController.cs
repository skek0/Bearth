using System;
using UnityEngine;

public class GameController : MonoBehaviour
{
    private void Start()
    {
        SaveController.Instance.Load();

        EnableEnemySpawner();
    }

    private void EnableEnemySpawner()
    {
            
    }
}
