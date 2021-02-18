using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class MiniGame : MonoBehaviour
{ 
    public enum MiniGameState { init, countdown, playing, ended }

    [SerializeField]
    string m_sMusicAmbName = "";

    protected MiniGameState m_miniGameState;
    protected float m_fStartTimer;
    protected byte m_nMiniGameDataSelected;
    private bool m_bInitDone;

    protected float m_fInitTimerTest;
    private bool m_bAllPlayerReady = false;
    public bool isPlaying { get { return m_miniGameState == MiniGameState.playing; } }


    public virtual void Init()
    {
        m_bAllPlayerReady = false;
        m_miniGameState = MiniGameState.init;

        m_fInitTimerTest = Time.time + 0.5f;
        m_bInitDone = false;

       BattleContext bCtx = BattleContext.instance;

        // setup inputs for everyone
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].m_fireDlg += MiniGameFireInput;
            players[i].m_inputActionDlg += MiniGameActionInput;
        }

        // call for test
        OnAllPlayerReady();
    }

    public virtual void Clean()
    {
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].m_fireDlg -= MiniGameFireInput;
            players[i].m_inputActionDlg -= MiniGameActionInput;
        }

        CountdownHud countdownHud = HudManager.instance.GetHud<CountdownHud>(HudManager.GameHudType.countdown);
        if (countdownHud != null && countdownHud.gameObject.activeSelf)
        {
            countdownHud.gameObject.SetActive(false);
        }

        MiniGameBasicHud hud = HudManager.instance.GetHud<MiniGameBasicHud>(HudManager.GameHudType.miniGame);
        hud.Exit();
        GameObject.Destroy(hud.gameObject);
    }

    protected virtual void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
    }

    protected virtual bool MiniGameActionInput(int playerId, RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        return true;
    }

    public virtual Dictionary<int, int> ComputeStatsDic()
    {
        return null;
    }

    public void StartCountdown()
    {
        HudManager.instance.ShowHud(HudManager.GameHudType.countdown);
        CountdownHud hud = HudManager.instance.GetHud<CountdownHud>(HudManager.GameHudType.countdown);
        hud.PlayIntro();
        hud.transform.SetAsLastSibling();
        hud.onEndIntroAnimation = OnEndCountdownIntro;
    }

    private void OnEndCountdownIntro()
    {
        StartCountdown321Animation();
    }

    private void StartCountdown321Animation()
    {
        InitWarmUp();
        m_miniGameState = MiniGameState.countdown;
        CountdownHud hud = HudManager.instance.GetHud<CountdownHud>(HudManager.GameHudType.countdown);
        hud.StartCountdown(StartGame);
    }

    protected virtual void StartGame()
    {
        m_miniGameState = MiniGameState.playing;
        m_fStartTimer = Time.time;

        if (!string.IsNullOrEmpty(m_sMusicAmbName))
        {
            RRSoundManager.instance.PlayPersistentSound(m_sMusicAmbName);
        }
    }

    public void StartEndAnim()
    {
        StopAmbiant();

        CountdownHud hud = HudManager.instance.GetHud<CountdownHud>(HudManager.GameHudType.countdown);
        hud.StartFinalAnimation(EndGame);
    }

    private void StopAmbiant()
    {
        if (!string.IsNullOrEmpty(m_sMusicAmbName))
        {
            RRSoundManager.instance.StopPersistentSound(m_sMusicAmbName);
        }
    }

    private void EndGame()
    {
        BattleContext.instance.ManageEndMiniGame();
    }


    private void OnEndTransitionOut()
    {
        StartCountdown();
    }

    private void Update()
    {
        switch (m_miniGameState)
        {
            case MiniGameState.init:
                if (!m_bInitDone)
                {
                    if (UpdateInit())
                    {
                        m_bInitDone = true;
                        GenericTransitionHud hud = HudManager.instance.GetForeHud<GenericTransitionHud>(HudManager.ForeHudType.genericTransition);
                        if (hud == null)
                        {
                            OnEndTransitionOut();
                        }
                        else
                        {
                            hud.StartTransitionOut(OnEndTransitionOut);
                        }
                    }
                }
                break;
            case MiniGameState.countdown:
                UpdateWarmUp();
                break;
            case MiniGameState.playing:
                if (!UpdateGamePlay())
                {
                    //Time.timeScale = 1f;
                    m_miniGameState = MiniGameState.ended;
                    StartEndAnim();
                    StopPlaying();
                }
                else
                {
                    CheckCheatCode();
                }
                break;
        }
    }

    protected virtual void InitWarmUp()
    {

    }

    protected virtual void UpdateWarmUp()
    {

    }

    protected virtual void StopPlaying()
    {

    }

    protected virtual bool UpdateInit()
    {
        return m_bAllPlayerReady;
    }

    private void OnAllPlayerReady()
    {
        m_bAllPlayerReady = true;
    }

    public virtual void ServerRefreshAsk(int nAskId)
    {

    }

    public virtual void ServerRefreshAskWithIntArray(int nAskId, int[] argsArray)
    {

    }

    public virtual void UpdatePlayerBot(GKPlayerData player)
    {
        if (m_miniGameState == MiniGameState.playing)
        {
            int nRnd = UnityEngine.Random.Range(0, 100);
            if (nRnd == 99)
            {
                player.m_currentScore += 100;
            }
        }
    }

    protected virtual bool UpdateGamePlay()
    {
        return false;
    }

    private void CheckCheatCode()
    {
/*        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            Time.timeScale = 10f;
        }*/
    }


    private void OnEndGameTransitionDone()
    {
    }

    private void WaitEveryBodyBeforeGoToVS()
    {
    }
}
