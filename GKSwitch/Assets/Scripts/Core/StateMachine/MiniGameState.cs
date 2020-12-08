using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameState : GameState
{
    private MiniGame m_miniGame;
    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        HudManager.instance.HideHud(HudManager.GameHudType.gameBkg);
        m_miniGame = MiniGameManager.instance.InstantiateMiniGame(BattleContext.instance.GetMiniGame());

        BattleContext battle = BattleContext.instance;
        for (int nPlayerId = 0; nPlayerId < battle.playerCount; nPlayerId++)
        {
        }

        m_miniGame.Init();
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
        m_miniGame.Clean();
        BattleContext battle = BattleContext.instance;
        for (int nPlayerId = 0; nPlayerId < battle.playerCount; nPlayerId++)
        {

        }
        GameObject.Destroy(m_miniGame.gameObject);
        HudManager.instance.ShowHud(HudManager.GameHudType.gameBkg);
    }
}
