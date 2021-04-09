using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FtuePopup : MonoBehaviour
{
    [System.Serializable] private class MiniGameAnimationPrefab : lwEnumArray<MiniGameManager.MiniGames, GameObject> { }; // dummy definition to use Unity serialization

    private enum FtuePopupState { waiting, askToClose, Closing }

    [SerializeField]
    MiniGameAnimationPrefab m_miniGamePrefabs;
    [SerializeField]
    Transform m_miniGameRoot;
    [SerializeField]
    float m_fTimeBeforeOkButtonAppear = 1f;

    [SerializeField]
    private TutoPlayerBoard m_tutoplayerBoardPrefab = null;
    [SerializeField]
    private Rect m_classicPlayerBoardRect;
    [SerializeField]
    private Rect m_quarterPlayerBoardRect;

    private int m_playerValidFlag = 0;
    private int m_playerValidGoal = 0;
    private TutoPlayerBoard[] m_playersBoard;
    private RRPlayerInput[] m_playersInputs;

    private bool m_bUsePopupRootAnimator;
    private System.Action m_onCloseDlg;
    private GameObject m_miniGameAnimation;
    private float m_fOpenTimer;

    private FtuePopupState m_state;

    public void Setup( MiniGameManager.MiniGames miniGame, System.Action onCloseDlg, bool bUsePopupRootAnimator=true )
    {
        m_onCloseDlg = onCloseDlg;
        m_bUsePopupRootAnimator = bUsePopupRootAnimator;
        if (m_miniGameAnimation!=null )
        {
            GameObject.Destroy(m_miniGameAnimation);
        }
        Debug.Assert(m_miniGamePrefabs[miniGame] != null);
        m_miniGameAnimation = GameObject.Instantiate(m_miniGamePrefabs[miniGame], m_miniGameRoot);
        m_fOpenTimer = Time.time + m_fTimeBeforeOkButtonAppear;
        
        // setup inputs for everyone
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        m_playersInputs = players.ToArray();
        for (int i = 0; i < players.Count; i++)
        {
            players[i].m_inputActionDlg += FtueActionInput;
        }
        SetupPlayerBoard();
        m_state = FtuePopupState.waiting;
    }

    private void Update()
    {
        if( m_state == FtuePopupState.askToClose )
        {
            Close();
            m_state = FtuePopupState.Closing;
        }

        // Perhaps use this timer to wait before player input active
        if( m_fOpenTimer!=-1 && Time.time>m_fOpenTimer)
        {
            m_fOpenTimer = -1f;
        }
    }


    public void Close()
    {
        if (m_miniGameAnimation != null)
        {
            GameObject.Destroy(m_miniGameAnimation);
        }

        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        for (int i = 0; i < players.Count; i++)
        {
            players[i].m_inputActionDlg -= FtueActionInput;
        }
        // Clean player board
        for ( int i=0; i<m_playersBoard.Length;i++ )
        {
            GameObject.Destroy(m_playersBoard[i].gameObject);
        }

        HudManager.instance.ClosePopup(HudManager.PopupType.tutorial, false);
        //HudManager.instance.HideNoMainPlacePopup(HudManager.NoMainPlacePopup.FtuePopup, m_bUsePopupRootAnimator);
        if( m_onCloseDlg!=null )
        {
            m_onCloseDlg();
        }
    }

    private void SetupPlayerBoard()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playersBoard = new TutoPlayerBoard[playerCount];
        m_playerValidFlag = 0;
        m_playerValidGoal = (1 << playerCount) - 1;

        Vector2[] positions = ComputeBoardPosition(playerCount);

        for( int i=0; i<playerCount; i++ )
        {
            m_playersBoard[i] = GameObject.Instantiate<TutoPlayerBoard>(m_tutoplayerBoardPrefab, transform);
            m_playersBoard[i].GetComponent<RectTransform>().anchoredPosition = positions[i];
            m_playersBoard[i].Setup(i );
            m_playersBoard[i].SetSensibility(m_playersInputs[i].cursorAiming.sensibility);
        }
       
    }

    private Vector2[] ComputeBoardPosition(int playerCount)
    {
        Vector2[] positions = new Vector2[playerCount];
        bool classic = HudManager.sSPLITHUD_COUNT == 1 || HudManager.sSPLITHUD_TYPE != HudManager.SplitHudType.quarter || playerCount < 3;

        if( classic ) // use classic Rect
        {
            if( playerCount == 1 )
            {
                float fX = (m_classicPlayerBoardRect.width / 2f) + m_classicPlayerBoardRect.x;
                positions[0] = new Vector2(fX, m_classicPlayerBoardRect.y);
            }
            else
            {
                float fSize = m_classicPlayerBoardRect.width / (playerCount-1);
                for (int i = 0; i < playerCount; i++)
                {
                    float fX = (fSize*i)  + m_classicPlayerBoardRect.x;
                    positions[i] = new Vector2(fX, m_classicPlayerBoardRect.y);
                }
            }
        }
        else
        {
            positions[0] = new Vector2(m_quarterPlayerBoardRect.x, m_quarterPlayerBoardRect.y + m_quarterPlayerBoardRect.height);
            positions[1] = new Vector2(m_quarterPlayerBoardRect.x + m_quarterPlayerBoardRect.width, m_quarterPlayerBoardRect.y + m_quarterPlayerBoardRect.height);
            positions[2] = new Vector2(m_quarterPlayerBoardRect.x, m_quarterPlayerBoardRect.y );
            if( playerCount>3)
            {
                positions[3] = new Vector2(m_quarterPlayerBoardRect.x + m_quarterPlayerBoardRect.width, m_quarterPlayerBoardRect.y);
            }
        }
        return positions;
    }

    private bool FtueActionInput(int playerId, RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch ( inputActionType )
        {
            case RRInputManager.InputActionType.ButtonRight:
                {
                    if(( m_playerValidFlag & (1<<playerId)) == 0 )
                    {
                        m_playersBoard[playerId].SetValid();
                        m_playerValidFlag |= (1 << playerId);
                        if( m_playerValidFlag == m_playerValidGoal )
                        {
                            m_state = FtuePopupState.askToClose;
                        }
                    }
                }
                break;
            case RRInputManager.InputActionType.ButtonTop:
                {
                    List<RRPlayerInput> players = RRInputManager.instance.playerList;
                    players[playerId].Recalibrate();
                }
                break;
            case RRInputManager.InputActionType.Move:
                {
                    switch( moveDirection )
                    {
                        case RRInputManager.MoveDirection.left:
                            {
                                IncreaseSensibility(playerId, -1);
                            }
                            break;
                        case RRInputManager.MoveDirection.right:
                            {
                                IncreaseSensibility(playerId, 1);
                            }
                            break;
                    }
                }
                break;
        }
        return true;
    }

    private void IncreaseSensibility( int playerId, int increment )
    {
        if( m_playersInputs[playerId].cursorAiming.IncreaseSensibility(increment))
        {
            m_playersBoard[playerId].SetSensibility(m_playersInputs[playerId].cursorAiming.sensibility);
        }
    }
}
