using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PlayerSelection : MainMenuStateObject
{
    public class PlayerSelectionInfo
    {
        public int currentSelection;
        public PlayerSelectionCursor cursor;
        public PlayerSelectionBoard board;
    }


    [SerializeField]
    private Transform m_toastyRoot;
    [SerializeField]
    private SelectableToasty m_toastyPrefab;
    [SerializeField]
    private PlayerSelectionCursor m_cursorPrefab;
    [SerializeField]
    private RectTransform m_cursorRoot;
    [SerializeField]
    private PlayerSelectionBoard m_playerBoardPrefab;

    [Header("Layout")]
    [SerializeField]
    private RectTransform[] m_choiceRoot;
    [SerializeField]
    private RectTransform m_choiceObject;
    [SerializeField]
    private GridLayoutGroup m_choiceObjectGrid;
    [SerializeField]
    private PlayerSelectionLayout[] m_playerLayout;

    private Dictionary<int, PlayerSelectionInfo> m_infoDico = new Dictionary<int, PlayerSelectionInfo>();
    private List<SelectableToasty> m_toastysList = new List<SelectableToasty>();
    private int m_rowCount;
    private int m_colCount;
    private float m_rowSize;
    private float m_colSize;
    private int m_playerReadyMask;
    private int m_playerValidMask;

    public void Awake()
    {
        ToastyCollection toastyCollection = GameContext.instance.m_toastyCollection;
        for( int i=0; i<toastyCollection.toastyDatas.Length; i++ )
        {
            SelectableToasty selectableToasty = GameObject.Instantiate<SelectableToasty>(m_toastyPrefab, m_toastyRoot);
            selectableToasty.Setup(toastyCollection.toastyDatas[i]);
            m_toastysList.Add(selectableToasty);
        }
        m_colCount = m_choiceObjectGrid.constraintCount;
        m_rowCount = (int)(Mathf.CeilToInt( m_toastysList.Count / m_colCount ));
        m_rowSize = m_choiceObjectGrid.cellSize.y;
        m_colSize = m_choiceObjectGrid.cellSize.x;
    }

    public override void Setup()
    {
        CleanBoard();
        int playerCount = BattleContext.instance.playerCount;
        m_choiceObject.transform.parent = m_choiceRoot[playerCount == 1 ? 0 : 1];
        m_choiceObject.anchoredPosition = Vector2.zero;
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        GameSettings gameSettings = GameContext.instance.m_settings;

        int layoutId = Mathf.Clamp(playerCount - 1, 0, m_playerLayout.Length - 1);
        PlayerSelectionLayout layout = m_playerLayout[layoutId];

        for ( int i=0; i<playerCount; i++ )
        {
            PlayerSelectionInfo info = new PlayerSelectionInfo();
            info.currentSelection = i;
            info.cursor = GameObject.Instantiate<PlayerSelectionCursor>(m_cursorPrefab, m_cursorRoot);
            info.cursor.Setup(gameSettings.playerSettings[i].color);

            info.board = GameObject.Instantiate<PlayerSelectionBoard>(m_playerBoardPrefab, layout.playersRoot[i]);
            info.board.Setup(gameSettings.playerSettings[i].color);

            m_infoDico.Add(i, info);
            players[i].m_inputActionDlg += OnPlayerSelection;
            SelectToasty(i);
        }
        m_playerReadyMask = 0;
        m_playerValidMask = (1 << playerCount) - 1;
    }


    public override void Clean()
    {
        int playerCount = BattleContext.instance.playerCount;
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        for (int i = 0; i < playerCount; i++)
        {
            PlayerSelectionInfo info = null;
            if (m_infoDico.TryGetValue(i, out info))
            {
                GKPlayerData playerData = BattleContext.instance.GetPlayer(i);
                playerData.sToastyId = m_toastysList[info.currentSelection].m_toastyData.sId;
            }
            players[i].m_inputActionDlg -= OnPlayerSelection;
        }
    }


    private bool OnPlayerSelection(int playerId, RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        PlayerSelectionInfo info = null;
        if( ( !m_infoDico.TryGetValue( playerId, out info)) || (m_playerReadyMask & (1<<playerId))!=0 )
        {
            return false;
        }

        switch( inputActionType)
        {
            case RRInputManager.InputActionType.Move:
                {
                    int previous = info.currentSelection;
                    int next = previous;
                    switch( moveDirection )
                    {
                        case RRInputManager.MoveDirection.bottom:
                            {
                                if( previous + m_colCount < m_toastysList.Count )
                                {
                                    next = previous + m_colCount;
                                }
                            }
                            break;
                        case RRInputManager.MoveDirection.left:
                            {
                                if (previous % m_colCount != 0)
                                {
                                    next = previous -1;
                                }
                            }
                            break;
                        case RRInputManager.MoveDirection.right:
                            {
                                if (previous + 1 < m_toastysList.Count && (previous % m_colCount) != m_colCount-1)
                                {
                                    next = previous + 1;
                                }
                            }
                            break;
                        case RRInputManager.MoveDirection.top:
                            {
                                if (previous - m_colCount >= 0)
                                {
                                    next = previous - m_colCount;
                                }
                            }
                            break;
                    }
                    if( next!=previous)
                    {
                        info.currentSelection = next;
                        SelectToasty(playerId);
                    }
                }
                break;
            case RRInputManager.InputActionType.Fire:
            case RRInputManager.InputActionType.ButtonRight:
                {
                    m_playerReadyMask |= (1 << playerId);
                    info.board.Validate();
                    if( m_playerReadyMask == m_playerValidMask)
                    {
                        ChangeMainMenuState(MainMenuHud.MainMenuState.RulesSettings);
                    }
                }
                break;

        }


        return true;
    }

    private void CleanBoard()
    {
        foreach( KeyValuePair<int, PlayerSelectionInfo> pair in m_infoDico )
        {
            GameObject.Destroy(pair.Value.board);
            GameObject.Destroy(pair.Value.cursor);
        }
        m_infoDico.Clear();
    }

    private void SelectToasty( int playerId )
    {
        PlayerSelectionInfo info = null;
        if (!m_infoDico.TryGetValue(playerId, out info))
        {
            return;
        }
        SetCursorOnSelection(info.cursor, info.currentSelection);
        info.board.SetAvatar(m_toastysList[info.currentSelection].m_toastyData);
    }


    private void SetCursorOnSelection(PlayerSelectionCursor cursor, int selection )
    {
        int nX = selection % m_colCount;
        int nY = -(int)(selection / m_colCount);
        cursor.SetPosition(nX * m_colSize, nY * m_rowSize);
    }
}
