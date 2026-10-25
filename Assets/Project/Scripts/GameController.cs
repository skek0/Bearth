using System;
using UnityEngine;

public class GameController : SceneSingleton<GameController>
{
    public enum GameState
    {
        Playing,
        Paused,
        GameOver
    }

    public event Action<GameState> GameStateChanged;

    public GameState Gamestate
    {
        get => gameState;
        set
        {
            if (gameState == value) return; // 같은 상태로 재진입 시 타임스케일 재설정 방지

            switch (value)
            {
                case GameState.Playing:
                    Time.timeScale = 1f;
                    break;
                case GameState.Paused:
                    Time.timeScale = 0f;
                    break;
                case GameState.GameOver:
                    Time.timeScale = 0f; // 필요시 조정
                    break;
            }

            gameState = value;
            GameStateChanged?.Invoke(gameState);
        }
    }

    private GameState gameState;

    protected override void Awake()
    {
        base.Awake();
        GameData.Initialize();
    }
    private void Start()
    {
        SaveController.Instance.Load();
        CameraRebinder.BindTo(FindAnyObjectByType<PlayerModule>().transform);

        EnemySpawner.Instance.enabled = true;
        //Gamestate = GameState.Playing;
    }

    //public void TogglePause()
    //{
    //    if (gameState == GameState.GameOver) return;

    //    Gamestate = gameState == GameState.Playing
    //        ? GameState.Paused
    //        : GameState.Playing;
    //}

    //public void Pause()
    //{
    //    if (gameState == GameState.Playing)
    //        Gamestate = GameState.Paused;
    //}

    //public void Resume()
    //{
    //    if (gameState == GameState.Paused)
    //        Gamestate = GameState.Playing;
    //}

}
