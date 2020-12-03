
#define USE_CHEATCODE

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSingleton : lwSingletonMonoBehaviour<GameSingleton>
{
    public GameStateMachine gameStateMachine { get { return m_gameStateMachine; } }

    public GUISkin m_guiSkin = null;


    GameStateMachine m_gameStateMachine;

#if USE_CHEATCODE
    private lwCheatCodes m_cheatCodes = null;


    // For some cheat codes
    private lwFPS m_fps = null;
    private lwShowSize m_showSize = null;
    private lwTextureMemory m_memory = null;
#endif
    // Use this for initialization
    void Awake()
    {

        PlayerData playerData = PlayerData.instance;
        playerData.Load();



        // CheatCodes Init
#if USE_CHEATCODE
        m_cheatCodes = gameObject.AddComponent<lwCheatCodes>();
        m_cheatCodes.m_eTriggerMode = lwCheatCodes.TriggerMode.Touch3;
        m_cheatCodes.AddCheatCodeCbk(OnCheatCode);
       // m_cheatCodes.AddSpecialCheatCodeCbk(OnSpecialCheatCode);
        for (int i = 0; i < (int)GameConstants.CheatCodesList.Count; i++)
        {
            string sInput = ((GameConstants.CheatCodesList)i).ToString().ToLower().Replace("_", string.Empty);
            m_cheatCodes.m_sCodeList.Add(sInput);
        }

        /*for (int i = 0; i < (int)GameConstants.SpecialCheatCodesList.Count; i++)
        {
            string sKey;
            int nLength;
            if ( ConvertSpecialCheatCodeToKey((GameConstants.SpecialCheatCodesList)i, out sKey, out nLength))
            {
                m_cheatCodes.m_dictSpecialCodes.Add(sKey, nLength);
            }
        }*/
#endif

        AudioListener.volume = 1f;

        m_gameStateMachine = new GameStateMachine();

    }

    protected override void Start()
    {
        base.Start();
        RRInputManager.instance.StartCoroutine(RRInputManager.instance.InitInput());
    }

    protected override void Update()
    {
        base.Update();
        m_gameStateMachine.Update();

    }

#if USE_CHEATCODE
    // ----------------------------------------------------------------------------------
    // Created PCT 20/01/2018
    // ----------------------------------------------------------------------------------
    private void OnCheatCode(int nCode)
    {
        string sAdditionalLog = string.Empty;

        GameConstants.CheatCodesList eCode = (GameConstants.CheatCodesList)nCode;
        switch (eCode)
        {
            case GameConstants.CheatCodesList.FPS:
                if (m_fps == null)
                {
                    m_fps = gameObject.AddComponent<lwFPS>();
                    m_fps.m_skin = m_guiSkin;
                    m_fps.m_rectGui = new Rect(5, 5, 200, 40);
                    sAdditionalLog = "OK";
                }
                else
                {
                    GameObject.Destroy(m_fps);
                    m_fps = null;
                    sAdditionalLog = "NONE";
                }
                break;
            case GameConstants.CheatCodesList.SIZE:
                if (m_showSize == null)
                {
                    m_showSize = gameObject.AddComponent<lwShowSize>();
                    m_showSize.m_skin = m_guiSkin;
                    sAdditionalLog = "OK";
                }
                else
                {
                    GameObject.Destroy(m_showSize);
                    m_showSize = null;
                    sAdditionalLog = "NONE";
                }
                break;
           case GameConstants.CheatCodesList.MEMORY:
                if (m_memory == null)
                {
                    m_memory = gameObject.AddComponent<lwTextureMemory>();
                    m_memory.m_skin = m_guiSkin;
                    m_memory.m_rectTextGui = new Rect(0, 0, Screen.width, Screen.height * 0.95f);
                    m_memory.m_rectButtonGui = new Rect(0, Screen.height * 0.95f, Screen.width, Screen.height * 0.05f);
                    sAdditionalLog = "OK";
                }
                else
                {
                    GameObject.Destroy(m_memory);
                    m_memory = null;
                    sAdditionalLog = "NONE";
                }
                break;
            case GameConstants.CheatCodesList.TIME_5:
                if (Time.timeScale == 1f)
                    Time.timeScale = 5f;
                else
                    Time.timeScale = 1f;
                sAdditionalLog = Time.timeScale.ToString();
                break;
            case GameConstants.CheatCodesList.SLOW:
                if (Time.timeScale == 1f)
                    Time.timeScale = 0.1f;
                else
                    Time.timeScale = 1f;
                sAdditionalLog = Time.timeScale.ToString();
                break;
            case GameConstants.CheatCodesList.FRENCH:
                {
                    lwLanguageManager.instance.SetLanguage(0);
                }
                break;
            case GameConstants.CheatCodesList.ENGLISH:
                {
                    lwLanguageManager.instance.SetLanguage(1);

                }
                break;
        }
 //       Debug.Log("GK " + eCode.ToString() + " " + sAdditionalLog);
    }

    /*private bool OnSpecialCheatCode(string sLeft, string sRight)
    {
        GameConstants.SpecialCheatCodesList eCheatCode = ConvertSpecialCheatCode(sLeft, sRight);
        bool bProcessed = OnSpecialCheatCode(eCheatCode, sRight);
        if (bProcessed) Debug.Log("GK " + sLeft.ToUpper() + " " + sRight);
        return bProcessed;
    }

    private bool OnSpecialCheatCode(GameConstants.SpecialCheatCodesList eCheatCode, string sRight)
    {
        switch (eCheatCode)
        {
            case GameConstants.SpecialCheatCodesList.GEMS_XXXX:
                OnItemCheatCode(GameItem.ItemType.Currency, (int)GameItem.Currency.Real, sRight);
                return true;
            case GameConstants.SpecialCheatCodesList.COINS_XXXX:
                OnItemCheatCode(GameItem.ItemType.Currency, (int)GameItem.Currency.Virtual, sRight);
                return true;
            case GameConstants.SpecialCheatCodesList.LEVEL_XX:
                m_networkBDDManager.ChangeLevel(lwParseTools.ParseIntSafe(sRight));
                return true;

        }
        return false;
    }*/

    /* public static bool ConvertSpecialCheatCodeToKey(GameConstants.SpecialCheatCodesList cheatCode, out string sKey, out int nLength)
     {
         string sCheat = cheatCode.ToString();
         string[] sSplit = sCheat.Split('_');
         if (sSplit.Length >= 2)
         {
             sKey = string.Join("", sSplit, 0, sSplit.Length - 1).ToLower();
             nLength = sKey.Length + sSplit[sSplit.Length - 1].Length;
             return true;
         }
         else
         {
             sKey = null;
             nLength = 0;
             return false;
         }
     }

     public static GameConstants.SpecialCheatCodesList ConvertSpecialCheatCode(string sLeft, string sRight)
     {
         int nRightLength = sRight.Length;

         GameConstants.SpecialCheatCodesList[] cheatCodes = (GameConstants.SpecialCheatCodesList[])System.Enum.GetValues(typeof(GameConstants.SpecialCheatCodesList));
         int cheatCodeIndex = 0;
         GameConstants.SpecialCheatCodesList result = GameConstants.SpecialCheatCodesList.Count;
         while (cheatCodeIndex < cheatCodes.Length && result == GameConstants.SpecialCheatCodesList.Count)
         {
             string sKey;
             int nLength;
             if (ConvertSpecialCheatCodeToKey(cheatCodes[cheatCodeIndex], out sKey, out nLength))
             {
                 if (string.CompareOrdinal(sLeft, sKey) == 0 && nRightLength == nLength - sKey.Length)
                 {
                     result = cheatCodes[cheatCodeIndex];
                 }
             }

             ++cheatCodeIndex;
         }

         return result;
     }*/


#endif


}
