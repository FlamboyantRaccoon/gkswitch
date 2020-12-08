using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitState : GameState
{
    private bool m_bLanguageLoaded = false;
    private float m_fEnterTimer;

    private bool bPress { get; set; }
    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        HudManager.instance.ShowHud(HudManager.GameHudType.splashScreen);
        SplashScreenHud splash = HudManager.instance.GetHud<SplashScreenHud>(HudManager.GameHudType.splashScreen);
        splash.Reset();
        bPress = false;
        m_fEnterTimer = Time.realtimeSinceStartup;
        GameSingleton.instance.StartCoroutine(InitRoutine());
        RRInputManager.instance.PushInput(GameInitStateInput);
//        SoundManager.instance.StartAmbiance(SoundManager.SoundAmb.menu);
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
        RRInputManager.RemoveInputSafe(GameInitStateInput);
        HudManager.instance.GetHud<SplashScreenHud>( HudManager.GameHudType.splashScreen).FadeOut();
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
        if( m_fEnterTimer!=-1f && m_bLanguageLoaded)
        {
            m_fEnterTimer = -1f;
        }

        if( m_fEnterTimer==-1 && bPress)
        {
            GameSingleton.instance.gameStateMachine.ChangeState(new MainMenuState());
        }
    }

    private void AddTextFilesIntoLanguageManager()
    {
        lwLanguageManager langManager = lwLanguageManager.instance;

        TextAsset languageListAsset = Resources.Load<TextAsset>(GameConstants.TEXTLIST_PATH );
        if (languageListAsset == null)
        {
            Debug.LogError("Text Initialisation : the file '"+ GameConstants.TEXTLIST_PATH +"' has not been found !");
        }
        else
        {
            string[] sFiles = languageListAsset.text.Split(new char[] { '\n' }, System.StringSplitOptions.RemoveEmptyEntries);

            for (int nLanguageIndex = 0; nLanguageIndex < langManager.nLanguageCount; ++nLanguageIndex)
            {
                string sLangCulture = langManager[nLanguageIndex].m_sLanguageCulture;
                for (int nFileIndex = 0; nFileIndex < sFiles.Length; ++nFileIndex)
                {
                    langManager.AddTextFile(sLangCulture, sLangCulture, sFiles[nFileIndex].Replace("\r", ""));
                }
            }
        }
    }

    private IEnumerator InitRoutine()
    {
        m_bLanguageLoaded = false;
        GameContext.instance.Init();
        lwLanguageManager languageManager = lwLanguageManager.instance;
        AddTextFilesIntoLanguageManager();
        lwLanguageManager.instance.SetDefaultLanguage();
        //        lwLanguageManager.instance.ReloadTexts();

        //while(languageManager.nLanguageCount<=0 )
        {
            yield return null;
        }
        m_bLanguageLoaded = true;
    }

    private bool GameInitStateInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection )
    {
        switch( inputActionType )
        {
            case RRInputManager.InputActionType.Fire:
            case RRInputManager.InputActionType.ButtonRight:
                {
                    bPress = true;
                }
                break;
        }
        return true;
    }

}
