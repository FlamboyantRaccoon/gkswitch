using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BattleContext : lwSingleton<BattleContext>
{
    public enum MiniGameDifficulty { easy, medium, hard, champion, fiesta };
    public enum BattleType { normal, friendly, champion, tutorial };
    public enum ResultType { defeat = 0, victory = 1, championDefeat = 2, championVictory = 3 };
    public enum LeaderBoardStatus { none = 0, waiting = 1, valid = 2 };

    public int playerCount { get { return m_playerList.Count; } }
    public int currentRound { get; private set; }
    public int totalRound { get { return m_nRoundCount; } }
    public bool isBattleEnded { get { return currentRound >= m_nRoundCount; } }
    public bool isPracticeGame { get; set; }
    public bool isSpecialBot { get; set; }
    public bool isInstructor { get; set; }


    private lwRndArray m_selector;
    public MiniGameManager.MiniGames[] m_battleAvailableGames;
    public MiniGameManager.MiniGames selectedMiniGame { get { return m_selectedMiniGame; } set { m_selectedMiniGame = value; } }
    private MiniGameManager.MiniGames m_selectedMiniGame;
    private List<int> m_roundMiniGameSelection;


    public MiniGameDifficulty botDifficulty
    {
        get
        {
            if (m_botDifficulty == null || m_botDifficulty.Length == 0) return MiniGameDifficulty.easy;
            return currentRound < m_botDifficulty.Length ? (MiniGameDifficulty)m_botDifficulty[currentRound] : (MiniGameDifficulty)m_botDifficulty[m_botDifficulty.Length - 1];
        }
    }

    private List<GKPlayerData> m_playerList = new List<GKPlayerData>();

    private int m_nRoundCount = 3;

    public int m_nGameDataTestId = -1;
    public int m_nBotDataTestId = -1;

    private int[] m_botDifficulty = null;

    public BattleContext()
    {
        m_battleAvailableGames = new MiniGameManager.MiniGames[] { MiniGameManager.MiniGames.BalloonDrill, MiniGameManager.MiniGames.DrawShape,
                                                                    MiniGameManager.MiniGames.GreatClimbing, MiniGameManager.MiniGames.Banquet,
                                                                    MiniGameManager.MiniGames.Expressions, MiniGameManager.MiniGames.Picking,
                                                                    MiniGameManager.MiniGames.SnowArena, MiniGameManager.MiniGames.TrickOrTreat,
                                                                    MiniGameManager.MiniGames.IceHockey
        };
    }

    public void CreateGKPlayers()
    {
        Reset();
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            GKPlayerData playerData = null;
            if(players[i].gameObject.TryGetComponent<GKPlayerData>( out playerData ))
            {
                playerData.Clean();
            }
            else
            {
                playerData = players[i].gameObject.AddComponent<GKPlayerData>();
            }
            RegisterPlayer(playerData);
        }
        isPracticeGame = true;
    }

    public void RegisterPlayer(GKPlayerData player)
    {
        m_playerList.Add(player);
        player.Id = m_playerList.Count - 1;
    }

    public void UnRegisterPlayer(GKPlayerData player)
    {
        bool bFound = false;
        int nPlayerId = 0;
        while (!bFound && nPlayerId < m_playerList.Count)
        {
            if (m_playerList[nPlayerId] == player)
            {
                bFound = true;
            }
            else
            {
                nPlayerId++;
            }
        }
        if (bFound)
        {
            m_playerList.RemoveAt(nPlayerId);
        }
    }

    public GKPlayerData GetPlayer(int nPlayerId)
    {
        if (nPlayerId < m_playerList.Count)
        {
            return m_playerList[nPlayerId];
        }
        return null;
    }

    public MiniGameManager.MiniGames GetMiniGame()
    {

        if(m_roundMiniGameSelection!=null && m_roundMiniGameSelection.Count > currentRound && m_roundMiniGameSelection[currentRound]>=0 )
        {
            return (MiniGameManager.MiniGames)m_roundMiniGameSelection[currentRound];
        }
        int rnd = Random.Range(0, m_battleAvailableGames.Length);
        return m_battleAvailableGames[rnd];
    }

    public void Reset()
    {
        m_playerList.Clear();
        currentRound = 0;
    }

    public void SetBattleInfo( int nRoundCount, List<int> roundMiniGameSelection)
    {
        m_nRoundCount = nRoundCount;
        m_roundMiniGameSelection = roundMiniGameSelection;
    }

/*    public void InitBotDifficulty(int[] nDifficulty = null)
    {
        if (nDifficulty != null)
        {
            m_botDifficulty = nDifficulty;
        }
        else
        {
            m_botDifficulty = LevelBotDifficultyManager.GetLevelBotDifficulty(PlayerData.instance.levelInfos.nLevel).ComputeBotDifficultyOrder();
        }
    }*/

/*    public void SaveMiniGameStats(Dictionary<int, int> dic, ref List<Dictionary<string, object>> quests)
    {
        //GameSingleton.instance.networkManager.SetMiniGameStats(m_nWebBattleId, m_nWebMiniGameId, dic );
        // update record in player data
        bool bIsMiniGameWin = IsMiniGamePlayerWinner();

        // Good Action always be id 0
        int nGoodAction = 0;
        if (dic.TryGetValue(0, out nGoodAction))
        {
            AddQuestsCheck(ref quests, Quests.QuestType.goodActionDone, (int)selectedMiniGame, nGoodAction);
        }

        PlayerData.instance.UpdateStats(selectedMiniGame, m_playerList[0].m_currentScore, bIsMiniGameWin, dic);
    }*/

    public bool IsBotGame()
    {
        for (int i = 1; i < m_playerList.Count; i++)
        {
            if (!m_playerList[i].isBot)
            {
                return false;
            }
        }
        return true;
    }

    public bool IsMiniGameTutorial()
    {
        if (!IsBotGame())
        {
            return false;
        }
        return true;
    }

    public void ManageEndMiniGame(Dictionary<int, int> dic)
    {
        if (!isPracticeGame)
        {
            currentRound = currentRound + 1;
        }


        /*if( !isPracticeGame )
        {
            GoToVsScreen();
        }*/

        
    }


    public void ManageEndMiniGame()
    {
        AddMiniGameScoreToTotalScore();
        currentRound = currentRound + 1;

        if( isBattleEnded )
        {
            GameSingleton.instance.gameStateMachine.ChangeState(new GameStateResult());
        }
        else
        {
            GameSingleton.instance.gameStateMachine.ChangeState(new GameStateInterRound());
        }

        /*
        if (isPracticeGame)
        {
            GameSingleton.instance.gameStateMachine.ChangeState(new GameStateInterRound());
            return;
        }*/
    }

    public bool IsPlayerWinner()
    {
        return m_playerList.Count == 1 || (m_playerList[0].m_totalScore + m_playerList[0].m_currentScore) > (m_playerList[1].m_totalScore + m_playerList[1].m_currentScore);
    }

    public int GetPlayerGap()
    {
        if (m_playerList.Count == 1)
        {
            return 0;
        }
        return (m_playerList[0].m_totalScore + m_playerList[0].m_currentScore) - (m_playerList[1].m_totalScore + m_playerList[1].m_currentScore);
    }

    public bool IsMiniGamePlayerWinner()
    {
        return m_playerList.Count == 1 || m_playerList[0].m_currentScore > m_playerList[1].m_currentScore;
    }

    public void GoToVsScreen()
    {
        
    }


    // TODO Check if usefull
    public void SetNextRound()
    {
        
    }

    public void LaunchMiniGame()
    {
        
    }

    public void AddPoint(int nPoints, int nPlayerId = 0)
    {
        m_playerList[nPlayerId].AddCurrentScore( nPoints );
    }

    public void SetPoint(int nPoints, int nPlayerId = 0)
    {
        m_playerList[nPlayerId].SetLocalCurrentScore( nPoints );
    }

    public void AddMiniGameScoreToTotalScore()
    {
        for (int i = 0; i < m_playerList.Count; i++)
        {
            m_playerList[i].m_totalScore += m_playerList[i].m_currentScore;
            m_playerList[i].m_currentScore = 0;
        }
    }

    public bool IsEveryBodyReady(GKPlayerData.readyState stateWanted)
    {
        bool bBot = false;
        int nPlayerReady = 0;
        for (int nPlayerId = 0; nPlayerId < m_playerList.Count; nPlayerId++)
        {
            if (m_playerList[nPlayerId].m_nReadyState == stateWanted)
            {
                nPlayerReady++;
            }
            if (m_playerList[nPlayerId].isBot)
            {
                bBot = true;
            }
        }

        return nPlayerReady == m_playerList.Count || bBot;
    }

    public int GetLeader(bool bLocal = false)
    {
        int nLeaderId = 0;
        int nScoreLeader = -1;
        for (int i = 0; i < m_playerList.Count; i++)
        {
            if ( m_playerList[i].m_currentScore > nScoreLeader)
            {
                nScoreLeader = m_playerList[i].m_currentScore;
                nLeaderId = i;
            }
            else if ( m_playerList[i].m_currentScore == nScoreLeader)
            {
                nLeaderId = -1;
            }
        }
        return nLeaderId;
    }

    public void SelectMiniGame()
    {
       
    }

    private int ChooseNextMiniGame()
    {
        /*int nId = (int)m_selector.ChooseValue();
        return (int)BattleContext.instance.m_battleAvailableGames[nId];*/
        return 0;
    }

    public void DebugReadyStateInfo()
    {
        string s ="State Infos : ";
        for (int i = 0; i < m_playerList.Count; i++)
        {
            if (i != 0)
            {
                s += " / ";
            }
            s += m_playerList[i].m_nReadyState.ToString();
        }
        //HudManager.instance.SetDebugText(s);
    }

    public int ComputeRankWin()
    {
        return 0;
    }

    public int ComputeAndUpdateCurrencyWin()
    {
        bool bIsWinner = IsPlayerWinner();
        int nCurrencyWin = 0; // bIsWinner ? GameParams.CURRENCYWIN_WIN : GameParams.CURRENCYWIN_LOSE;
        return nCurrencyWin;
    }


}
