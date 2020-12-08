using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameLogoState : GameState
{
    private float m_fEnterTimer;

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        HudManager.instance.ShowHud(HudManager.GameHudType.gameBkg);
        HudManager.instance.ShowHud(HudManager.GameHudType.logoScreen);
        m_fEnterTimer = Time.realtimeSinceStartup;
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
        HudManager.instance.GetHud<LogoScreenHud>(HudManager.GameHudType.logoScreen).FadeOut();
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
        if (m_fEnterTimer != -1f && Time.realtimeSinceStartup - m_fEnterTimer > 1.8f)
        {
            m_fEnterTimer = -1f;
            GameSingleton.instance.gameStateMachine.ChangeState(new GameInitState());
        }
    }

   
}