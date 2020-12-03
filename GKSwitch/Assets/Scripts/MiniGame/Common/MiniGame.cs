﻿using System;
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
        }
    }

    protected virtual void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
    }

    public virtual Dictionary<int, int> ComputeStatsDic()
    {
        return null;
    }

    public void StartCountdown()
    {
    }

    private void OnEndCountdownIntro()
    {
        StartCountdown321Animation();
    }

    private void StartCountdown321Animation()
    {
        InitWarmUp();
        m_miniGameState = MiniGameState.countdown;
    }

    protected virtual void StartGame()
    {
        m_miniGameState = MiniGameState.playing;
        m_fStartTimer = Time.time;

        if (!string.IsNullOrEmpty(m_sMusicAmbName))
        {
        }
    }

    public void StartEndAnim()
    {
        StopAmbiant();
    }

    private void StopAmbiant()
    {
    }

    private void EndGame()
    {
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
                        StartGame();
  /*                      GenericTransitionHud hud = HudManager.instance.GetHud<GenericTransitionHud>(HudManager.GameHudType.genericTransition);
                        if (hud == null)
                        {
                            OnEndTransitionOut();
                        }
                        else
                        {
                            hud.StartTransitionOut(OnEndTransitionOut);
                        }*/
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