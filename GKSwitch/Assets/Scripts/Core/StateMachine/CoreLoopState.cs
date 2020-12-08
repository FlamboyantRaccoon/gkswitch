using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CoreLoopState : GameState
{
    private bool m_bIsInit = false;

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
        {
            yield return null;
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
        if( !m_bIsInit )
        {
            return;
        }
    }
}