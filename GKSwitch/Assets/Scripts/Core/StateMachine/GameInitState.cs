using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameInitState : GameState
{
    private bool m_bLanguageLoaded = false;
    private float m_fEnterTimer;

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Enter()
    {
        HudManager.instance.ShowHud(HudManager.GameHudType.splashScreen);
        SplashScreenHud splash = HudManager.instance.GetHud<SplashScreenHud>(HudManager.GameHudType.splashScreen);
        splash.Reset();
        m_fEnterTimer = Time.realtimeSinceStartup;
        GameSingleton.instance.StartCoroutine(InitRoutine());

//        SoundManager.instance.StartAmbiance(SoundManager.SoundAmb.menu);
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Exit()
    {
        HudManager.instance.GetHud<SplashScreenHud>( HudManager.GameHudType.splashScreen).FadeOut();
    }

    // ------------------------------------------------------------------
    // Created PC 16/03/12
    // ------------------------------------------------------------------
    public override void Update()
    {
        if( m_fEnterTimer!=-1f && Time.realtimeSinceStartup - m_fEnterTimer > 1.2f && m_bLanguageLoaded)
        {
            m_fEnterTimer = -1f;
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

}
