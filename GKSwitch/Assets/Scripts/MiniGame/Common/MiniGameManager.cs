using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameManager : lwSingletonMonoBehaviour<MiniGameManager>
{
    public enum MiniGames { BalloonDrill = 0, ConveyorBelt = 1, GreatClimbing = 2, DrawShape = 3, Picking = 4, Expressions = 5, TrickOrTreat = 6, SnowArena = 7  }

    public const int xDEFAULT_AVAILABLE_MG = (1 << (int)MiniGames.BalloonDrill) | (1 << (int)MiniGames.ConveyorBelt) | (1 << (int)MiniGames.GreatClimbing) | (1 << (int)MiniGames.DrawShape) |
                                                (1 << (int)MiniGames.Picking) | (1 << (int)MiniGames.Expressions);
    public const int xTUTO_START_MG = 1 << (int)MiniGames.BalloonDrill;

    // TODO Add forbidden mini game list
    public const int xNOT_AVAILABLE_MG = 0; // (1 << (int)MiniGames.SnowArena);// (1 << (int)MiniGames.TrickOrTreat) | (1 << (int)MiniGames.ConveyorBelt);

    [System.Serializable] private class MiniGamePrefab : lwEnumArray<MiniGames, MiniGame> { }; // dummy definition to use Unity serialization

    [SerializeField]
    MiniGamePrefab m_miniGamePrefabs;

    static public int miniGameCount { get { return System.Enum.GetValues(typeof(MiniGames)).Length; } }

    static public MiniGameManager.MiniGames[] ComputeAvailableMiniGame( int xMask )
    {
        List<MiniGameManager.MiniGames> miniGames = new List<MiniGameManager.MiniGames>();
        for (int i = 0; i < MiniGameManager.miniGameCount; i++)
        {
            if ((xMask & (1 << i)) != 0)
            {
                miniGames.Add((MiniGameManager.MiniGames)i);
            }
        }

        return miniGames.ToArray();
    }

    static public MiniGameManager.MiniGames[] ComputeAvailableMiniGame()
    {
        List<MiniGameManager.MiniGames> miniGames = new List<MiniGameManager.MiniGames>();
        miniGames.Add(MiniGames.BalloonDrill);

        return miniGames.ToArray();
    }


    public MiniGame InstantiateMiniGame( MiniGames game ) 
    {
        return GameObject.Instantiate(m_miniGamePrefabs[game]) as MiniGame;
    }

}
