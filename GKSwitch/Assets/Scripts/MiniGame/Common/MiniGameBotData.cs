using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MiniGameBotData
{

    public BattleContext.MiniGameDifficulty botDifficulty;
#if UNITY_EDITOR
    public string m_sComment = "Bla bla pour le designer";
#endif

    [lwMinMaxVector(-10, 100, true)]
    public Vector2 miniGamelevel = new Vector2(0, 10);

    public int m_nGoodActionPointWin;

    public static int GetDataId(MiniGameBotData[] datas, BattleContext.MiniGameDifficulty difficulty )
    {
        int nTestId = BattleContext.instance.m_nBotDataTestId;
        if ( nTestId!=-1 && nTestId<datas.Length )
        {
            return nTestId;
        }

        List<int> availableId = new List<int>();
        for (int nDataId = 0; nDataId < datas.Length; nDataId++)
        {
            if (datas[nDataId].botDifficulty == difficulty)
            {
                availableId.Add(nDataId);
            }
        }

        if (availableId.Count > 0)
        {
            return availableId[Random.Range(0, availableId.Count)];
        }

        while( difficulty!= BattleContext.MiniGameDifficulty.easy )
        {
            difficulty = (BattleContext.MiniGameDifficulty)(((int)difficulty) - 1);
            for (int nDataId = 0; nDataId < datas.Length; nDataId++)
            {
                if (datas[nDataId].botDifficulty == difficulty)
                {
                    availableId.Add(nDataId);
                }
            }
            if (availableId.Count > 0)
            {
                return availableId[Random.Range(0, availableId.Count)];
            }
        }
        return 0;
    }

    public void ComputeGoodActionPointWin(MiniGameManager.MiniGames miniGame )
    {
        /*
        BattleContext battleContext = BattleContext.instance;
        List<MiniGameLevels.MiniGameLevel> levelList = MiniGameLevels.MINIGAME_LEVELS_ARRAY[(int)miniGame];
        int nBotLevel = 0; 
        GKPlayerData botPlayer = battleContext.GetPlayer(1);

        if(battleContext.isMiniGameEqualizer )
        {
            nBotLevel = battleContext.ComputeMiniGameLevelMax(miniGame);
        }
        else if( botPlayer!=null && battleContext.IsBotGame())
        {
            nBotLevel = botPlayer.GetMiniGameLevel( miniGame );
        }
        else
        {
            nBotLevel = Mathf.Min(Random.Range((int)miniGamelevel.x, (int)miniGamelevel.y), levelList.Count - 1);
            int nPlayerLevel = (int)PlayerData.instance.miniGameDatas[(int)miniGame].nLevel;
            nBotLevel = (int)Mathf.Clamp(nPlayerLevel + nBotLevel, 0, levelList.Count - 1);
        }
        */
        m_nGoodActionPointWin = 100; // levelList[nBotLevel].nPoints * battleContext.battleMultiplier;

    }
}