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
                {
                    Debug.Log("Right");
                    GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
                }
                break;
        }
        return true;
    }
}
