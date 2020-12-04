using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuHud : MonoBehaviour
{
    public void OnEnable()
    {
        RRInputManager.instance.PushInput(MainMenuInput);
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(MainMenuInput);
    }

    private bool MainMenuInput(RRInputManager.InputActionType inputActionType)
    {
        switch( inputActionType )
        {
            case RRInputManager.InputActionType.ButtonRight:
            case RRInputManager.InputActionType.Fire:
                {
                    Debug.Log("Right");
                    BattleContext.instance.CreateGKPlayers();
                    GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
                }
                break;
        }
        return true;
    }
}
