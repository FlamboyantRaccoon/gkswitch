using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AdventureState : GameState
{
    private bool m_bIsInit = false;
    private AdventureEngine m_adventure;

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        // m_hud.periodTimeline.setPlayModeDlg = ChangePlayMode;
        m_bIsInit = false;
        GameSingleton.instance.StartCoroutine(InitAsync());
    }

    private IEnumerator InitAsync()
    {
        HudManager hudManager = HudManager.instance;
        hudManager.HideHud(HudManager.GameHudType.gameBkg);
        //        AsyncOperation asyncOperation = SceneManager.LoadSceneAsync(missionData.sSceneName, LoadSceneMode.Additive);
        //        while( !asyncOperation.isDone )

        m_adventure = new AdventureEngine();
        m_adventure.Init();

        while( !m_adventure.isInit)
        {
            yield return null;
        }

        GenericTransitionHud hud = HudManager.instance.GetForeHud<GenericTransitionHud>(HudManager.ForeHudType.genericTransition);
        if (hud != null)
        {
            hud.StartTransitionOut(null);
        }

        //hudManager.ShowHud(HudManager.GameHudType.coreHud);
        //hudManager.SetHudInFront(HudManager.GameHudType.loadingMenu);
        m_bIsInit = true;
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
        // autosave 
        HudManager.instance.ShowHud(HudManager.GameHudType.gameBkg);

        //HudManager.instance.HideHud(HudManager.GameHudType.coreHud);
        //        SceneManager.UnloadSceneAsync(missionData.sSceneName);
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
        if (!m_bIsInit)
        {
            return;
        }
    }
}