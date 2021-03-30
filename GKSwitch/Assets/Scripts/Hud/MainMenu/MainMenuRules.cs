using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenuRules : MainMenuStateObject
{
    public enum RulesSelection { gameCount, gameType, minigame, confirm }

    [System.Serializable] private class RulesSelectionObject : lwEnumArray<RulesSelection, GameObject> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class MiniGameSprite : lwEnumArray<MiniGameManager.MiniGames, Sprite> { }; // dummy definition to use Unity serialization

    [SerializeField]
    TMP_Text m_gameCountText;
    [SerializeField]
    RectTransform m_playerReminderRoot;
    [SerializeField]
    PlayerSelectionBoard m_playerBoardPrefab;
    [SerializeField]
    MiniGameRoulette m_miniGameRoulette;

    [SerializeField]
    RectTransform m_selectedGameRoot;
    [SerializeField]
    Image m_selectedGamePrefab;
    [SerializeField]
    MiniGameSprite m_miniGameSprites;
    [SerializeField]
    Sprite m_randomSprite;

    [Header("navigation")]
    [SerializeField]
    RulesSelectionObject m_selectedObject;

    private List<int> m_SelectedMiniGame = new List<int>();
    private Image[] m_selectedMiniGameImage = null;

    private int m_gameCount = 1;
    private bool m_gameLaunch = false;
    private RulesSelection m_currentSelection;
    private VerticalLayoutGroup m_gameImageLayout;

    private int m_currentGameSelection = 0;

    public void Awake()
    {
        m_gameImageLayout = m_selectedGameRoot.GetComponent<VerticalLayoutGroup>();
    }

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
        m_gameLaunch = false;
        SelectRulesSelection(RulesSelection.gameCount);
        RecomputeMiniGameList();
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(MainMenuModeRulesInput);
    }

    public void OnPlay()
    {
        if(m_gameLaunch )
        {
            return;
        }
        m_gameLaunch = true;
        HudManager.instance.ShowForeHud(HudManager.ForeHudType.genericTransition);
        GenericTransitionHud transitionHud = HudManager.instance.GetForeHud<GenericTransitionHud>( HudManager.ForeHudType.genericTransition);
        transitionHud.StartTransitionIn(LaunchGame);
    }

    private void LaunchGame()
    {
        BattleContext.instance.SetBattleInfo(m_gameCount, m_SelectedMiniGame);
        BattleContext.instance.selectedMiniGame = m_miniGameRoulette.GetSelectedMiniGame();
        GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
    }

    private void UpdateGameCount()
    {
        m_gameCountText.text = m_gameCount.ToString();
        RecomputeMiniGameList();
        m_currentGameSelection = Mathf.Min(m_currentGameSelection, m_gameCount - 1);
    }

    private void SelectRulesSelection( RulesSelection selection )
    {
        for( int i=0; i<m_selectedObject.nLength;i++ )
        {
            m_selectedObject[i].SetActive(i == (int)selection);
        }
        m_currentSelection = selection;
    }

    private bool MainMenuModeRulesInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch (inputActionType)
        {
            case RRInputManager.InputActionType.ButtonRight:
            case RRInputManager.InputActionType.Fire:
                {
                    if( m_currentSelection== RulesSelection.confirm )
                    {
                        OnPlay();
                    }
                    if( m_currentSelection == RulesSelection.minigame )
                    {
                        m_SelectedMiniGame[m_currentGameSelection] = (int)m_miniGameRoulette.GetSelectedMiniGame();
                        m_selectedMiniGameImage[m_currentGameSelection].sprite = m_miniGameSprites[m_SelectedMiniGame[m_currentGameSelection]];
                        m_currentGameSelection = Mathf.Min(m_currentGameSelection+1, m_gameCount - 1);
                    }
                }
                break;
            case RRInputManager.InputActionType.Move:
                {
                    switch( moveDirection )
                    {
                        case RRInputManager.MoveDirection.left:
                            {
                                IncrementSelection(-1);
                            }
                            break;
                        case RRInputManager.MoveDirection.right:
                            {
                                IncrementSelection(1);
                            }
                            break;
                        case RRInputManager.MoveDirection.top:
                            {
                                if( m_currentSelection != RulesSelection.gameCount )
                                {
                                    SelectRulesSelection((RulesSelection)((int)m_currentSelection - 1));
                                }
                            }
                            break;
                        case RRInputManager.MoveDirection.bottom:
                            {
                                if (m_currentSelection != RulesSelection.confirm)
                                {
                                    SelectRulesSelection((RulesSelection)((int)m_currentSelection + 1));
                                }
                            }
                            break;
                    }
                }
                break;
        }
        return true;
    }

    private void IncrementSelection( int increment )
    {
        switch( m_currentSelection )
        {
            case RulesSelection.gameCount:
                {
                    if(m_gameCount + increment >= 1 )
                    {
                        m_gameCount += increment;
                        UpdateGameCount();
                    }
                }
                break;
            case RulesSelection.minigame:
                {
                    m_miniGameRoulette.IncrementSelection(-increment);
                }
                break;
        }
    }

    private void RecomputeMiniGameList()
    {
        while( m_SelectedMiniGame.Count > m_gameCount )
        {
            m_SelectedMiniGame.RemoveAt(m_SelectedMiniGame.Count - 1);
        }

        while (m_SelectedMiniGame.Count < m_gameCount)
        {
            m_SelectedMiniGame.Add(-1);
        }

        lwTools.DestroyAllChildren(m_selectedGameRoot.gameObject);
        m_selectedMiniGameImage = new Image[m_gameCount];
        for ( int i=0; i<m_gameCount; i++ )
        {
            m_selectedMiniGameImage[i] = GameObject.Instantiate<Image>(m_selectedGamePrefab, m_selectedGameRoot);
            if( m_SelectedMiniGame[i]==-1 )
            {
                m_selectedMiniGameImage[i].sprite = m_randomSprite;
            }
            else
            {
                m_selectedMiniGameImage[i].sprite = m_miniGameSprites[i];
            }
        }

        if( m_gameCount>1 )
        {
            float cardHeight = m_selectedGamePrefab.GetComponent<RectTransform>().sizeDelta.y;
            float space = ((m_selectedGameRoot.sizeDelta.y - cardHeight) / (m_gameCount - 1)) - cardHeight;
            m_gameImageLayout.spacing = space;
        }
    }

}
