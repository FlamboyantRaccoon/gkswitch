using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuFreeMode : MainMenuStateObject
{
    [System.Serializable] public class MiniGameSprite : lwEnumArray<MiniGameManager.MiniGames, Sprite> { }; // dummy definition to use Unity serialization

    [SerializeField]
    MiniGameRoulette m_miniGameRoulette;

    private List<int> m_SelectedMiniGame = new List<int>(1) { -1 };

    private bool m_gameLaunch = false;
    
    private int m_currentGameSelection = 0;

    public void OnEnable()
    {
        RRInputManager.instance.PushInput(MainMenuFreeModeInput);

        m_miniGameRoulette.Init();
        m_gameLaunch = false;
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(MainMenuFreeModeInput);
    }

    public void OnPlay()
    {
        if (m_gameLaunch)
        {
            return;
        }
        m_gameLaunch = true;
        HudManager.instance.ShowForeHud(HudManager.ForeHudType.genericTransition);
        GenericTransitionHud transitionHud = HudManager.instance.GetForeHud<GenericTransitionHud>(HudManager.ForeHudType.genericTransition);
        transitionHud.StartTransitionIn(LaunchGame);
    }

    private void LaunchGame()
    {
        BattleContext.instance.SetBattleInfo(1, m_SelectedMiniGame);
        BattleContext.instance.selectedMiniGame = m_miniGameRoulette.GetSelectedMiniGame();
        GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
    }

    private bool MainMenuFreeModeInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
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
                    switch (moveDirection)
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
                    }
                }
                break;
        }
        return true;
    }

    private void IncrementSelection(int increment)
    {
        m_miniGameRoulette.IncrementSelection(-increment);
        m_SelectedMiniGame[0] = (int)m_miniGameRoulette.GetSelectedMiniGame();
    }

}
