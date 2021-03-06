using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameTemplate<TMgLogic, TMgData, TMgBot, TMgHud, TMgBalancing> : MiniGame where TMgLogic: MiniGameLogic where TMgData : MiniGameBalancingData where TMgBot : MiniGameBotData where TMgHud : MiniGameBasicHud where TMgBalancing : MG_Balancing<TMgBot, TMgData>
{
    [SerializeField]
    private TMgHud m_miniGameHudPrefab;

    [Header("Game Balancing")]
    [SerializeField]
    protected int m_nDataTestId = -1;
    [SerializeField]
    protected int m_nBotTestId = -1;
    [SerializeField]
    protected TMgBalancing m_gameBalancing;

    protected int m_nGoodPointsWin = 100;
    protected int[] m_gameStats;

    protected TMgHud m_hud;
    protected TMgData m_gameData;
    protected TMgBot m_currentBot;
    protected TMgLogic m_gameLogic;

    public TMgData gameData { get { return m_gameData; } }

    public override Dictionary<int, int> ComputeStatsDic()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        if (m_gameStats != null)
        {
            for (int i = 0; i < m_gameStats.Length; i++)
            {
                dic.Add(i, m_gameStats[i]);
            }
        }
        return dic;
    }

    public override void Init()
    {
        base.Init();
        InstanciateHud();
    }

    protected override void StartGameTimer()
    {
        m_gameLogic.StartGameTime();
    }

    protected void InitGameDataAndBot(MiniGameManager.MiniGames miniGame)
    {
        int playerLevel = 0;
        int miniGameLevel = 0;

        m_nMiniGameDataSelected = MiniGameBalancingData.GetDataId(m_gameBalancing.m_datas, playerLevel);
        m_gameData = m_gameBalancing.m_datas[m_nMiniGameDataSelected];

/*        int nLevel = miniGameLevel;
        m_nGoodPointsWin = MiniGameLevels.MINIGAME_LEVELS_ARRAY[(int)miniGame][nLevel].nPoints;
        m_nGoodPointsWin *= BattleContext.instance.battleMultiplier;*/
        m_nGoodPointsWin = 100;

        m_currentBot = m_gameBalancing.m_botDatas[MiniGameBotData.GetDataId(m_gameBalancing.m_botDatas, BattleContext.instance.botDifficulty)];
        m_currentBot.ComputeGoodActionPointWin(miniGame);
    }

    protected void InstanciateHud()
    {
        HudManager.instance.CreateMiniGameHud(m_miniGameHudPrefab.gameObject);
        m_hud = HudManager.instance.GetHud<TMgHud>(HudManager.GameHudType.miniGame);
        m_hud.UpdateTime(-1);
    }

    protected bool CheckAndUpdateTime()
    {
        int nElapsedTime = (int)(Time.time - m_fStartTimer);

        int totalTime = m_gameData.gameTime;
        //totalTime = 3;
        int nRemain = totalTime - nElapsedTime;
        if (nRemain < 0)
        {
            return false;
        }
        else
        {
            m_hud.UpdateTime(nRemain);
        }
        return true;
    }

    protected virtual void SetupLogicDelegate()
    {
    }
}
