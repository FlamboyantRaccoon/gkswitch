using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameStateInterRound : GameState
{
    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        HudManager.instance.ShowHud(HudManager.GameHudType.interRound);
    }


    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
        HudManager.instance.HideHud(HudManager.GameHudType.interRound);
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
    }
}
