using UnityEngine;
using System.Collections;

public class GameStateManager {

    public enum GameState
    {
        PlayersJoining,
        LoadingCubes,
        StartCountDown,
        GamePlay,
        EndGame
    }

    public GameState state; 

    public GameStateManager()
    {
        state = GameState.PlayersJoining; 
    }
}
