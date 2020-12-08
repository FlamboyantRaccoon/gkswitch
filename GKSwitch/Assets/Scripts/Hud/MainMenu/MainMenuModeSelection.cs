using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuModeSelection : MainMenuStateObject
{
    [SerializeField]
    private RRNavigationButton m_defaultButton;

    private RRNavigationButton m_selectedButton;

    public void OnEnable()
    {
        RRInputManager.instance.PushInput(MainMenuModeSelectionInput);
        m_selectedButton = m_defaultButton;
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
        m_defaultButton.Select();
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(MainMenuModeSelectionInput);
    }

    public void OnPlay()
    {
        BattleContext.instance.CreateGKPlayers();
        //GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
        ChangeMainMenuState(MainMenuHud.MainMenuState.ToastySelection);
    }

    public void OnOptions()
    {

    }

    private bool MainMenuModeSelectionInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch (inputActionType)
        {
            case RRInputManager.InputActionType.ButtonRight:
            case RRInputManager.InputActionType.Fire:
                {
                    /*Debug.Log("Right");
                    BattleContext.instance.CreateGKPlayers();
                    GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());*/
                    if (m_selectedButton != null)
                    {
                        m_selectedButton.SimulatePress();
                    }
                }
                break;
            case RRInputManager.InputActionType.Move:
                {
                    RRNavigationButton button = m_selectedButton.SelectNext(moveDirection);
                    if (button != null && button != m_selectedButton)
                    {
                        m_selectedButton = button;
                        m_selectedButton.Select();
                    }
                }
                break;
        }
        return true;
    }
}
