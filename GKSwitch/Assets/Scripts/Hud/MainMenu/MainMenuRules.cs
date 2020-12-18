using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class MainMenuRules : MainMenuStateObject
{
    [SerializeField]
    TMP_Text m_gameCountText;
    [SerializeField]
    RectTransform m_playerReminderRoot;
    [SerializeField]
    PlayerSelectionBoard m_playerBoardPrefab;
    [SerializeField]
    MiniGameRoulette m_miniGameRoulette;


    private int m_gameCount = 1;

    public void OnEnable()
    {
        RRInputManager.instance.PushInput(MainMenuModeRulesInput);
        m_gameCount = 1;
        UpdateGameCount();

        lwTools.DestroyAllChildren(m_playerReminderRoot.gameObject);

        int playerCount = BattleContext.instance.playerCount;
        GameSettings gameSettings = GameContext.instance.m_settings;
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;
        for (int i = 0; i < playerCount; i++)
        {
            GKPlayerData playerData = BattleContext.instance.GetPlayer(i);
            PlayerSelectionBoard board = GameObject.Instantiate<PlayerSelectionBoard>(m_playerBoardPrefab, m_playerReminderRoot);
            board.Setup(gameSettings.playerSettings[i].color);
            board.SetAvatar(toasties.GetToasty(playerData.sToastyId));
        }

        m_miniGameRoulette.Init();
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(MainMenuModeRulesInput);
    }

    public void OnPlay()
    {
        BattleContext.instance.SetBattleInfo(m_gameCount);
        BattleContext.instance.selectedMiniGame = m_miniGameRoulette.GetSelectedMiniGame();
        GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
    }

    private void UpdateGameCount()
    {
        m_gameCountText.text = m_gameCount.ToString();
    }

    private bool MainMenuModeRulesInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch (inputActionType)
        {
            case RRInputManager.InputActionType.ButtonRight:
            case RRInputManager.InputActionType.Fire:
                {
                    OnPlay();
                }
                break;
            case RRInputManager.InputActionType.Move:
                {
                    switch( moveDirection )
                    {
                        case RRInputManager.MoveDirection.left:
                            {
                                /*if( m_gameCount>0 )
                                {
                                    m_gameCount--;
                                    UpdateGameCount();
                                }*/
                                m_miniGameRoulette.IncrementSelection(-1);
                            }
                            break;
                        case RRInputManager.MoveDirection.right:
                            {
                                m_miniGameRoulette.IncrementSelection(1);
                                /*
                                m_gameCount++;
                                UpdateGameCount();*/
                            }
                            break;
                    }
                }
                break;
        }
        return true;
    }
}
