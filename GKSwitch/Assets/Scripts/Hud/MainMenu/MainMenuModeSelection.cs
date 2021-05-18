using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuModeSelection : MainMenuStateObject
{
    [SerializeField]
    private RRNavigationButton m_defaultButton;

    private RRNavigationButton m_selectedButton = null;

    public void OnEnable()
    {
        RRInputManager.instance.PushInput(MainMenuModeSelectionInput);
    }

    public override void Setup()
    {
        
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(MainMenuModeSelectionInput);
    }

    public void OnTournament()
    {
        BattleContext.instance.CreateGKPlayers();
        //GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
        ChangeMainMenuState(MainMenuHud.MainMenuState.ToastySelection);
    }

    public void OnAdventure()
    {
        HudManager.instance.ShowForeHud(HudManager.ForeHudType.genericTransition);
        GenericTransitionHud transitionHud = HudManager.instance.GetForeHud<GenericTransitionHud>(HudManager.ForeHudType.genericTransition);
        transitionHud.StartTransitionIn(LaunchAdventure);

        
    }

    private void LaunchAdventure()
    {
        GameSingleton.instance.gameStateMachine.ChangeState(new AdventureState());
    }


    public void OnOptions()
    {

    }

    private void Update()
    {
        if (m_selectedButton == null)
        {
            m_selectedButton = m_defaultButton;
            UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(null);
            m_selectedButton.Select();
        }
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
                    Debug.Log("Move " + moveDirection);
                    if (button != null && button != m_selectedButton)
                    {
                        m_selectedButton = button;
                        m_selectedButton.Select();
                        Debug.Log("Select " + m_selectedButton.name);
                    }
                }
                break;
        }
        return true;
    }
}
