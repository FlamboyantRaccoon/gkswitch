using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuState : GameState
{
    private float m_fEnterTimer;

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        HudManager.instance.ShowHud(HudManager.GameHudType.mainMenu );
        HudManager.instance.ShowForeHud(HudManager.ForeHudType.aimingHud);
        AimingHud aimingHud = HudManager.instance.GetForeHud<AimingHud>(HudManager.ForeHudType.aimingHud);
        aimingHud.Setup();
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
       HudManager.instance.HideHud(HudManager.GameHudType.mainMenu);
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
        
    }

}