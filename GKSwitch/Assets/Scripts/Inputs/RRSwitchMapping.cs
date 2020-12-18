using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.InputSystem;
#if UNITY_SWITCH
using nn.hid;
#endif

public class RRSwitchMapping : MonoBehaviour
{
#if UNITY_SWITCH
    private nn.hid.NpadId nPadId;
    private RRPlayerInput playerInput;

    NpadStyle nStyle;
    NpadState nState;

    public void SetNpadId(nn.hid.NpadId id)
    {
        nPadId = id;
        //nStyle = Npad.GetStyleSet(nPadId);
    }

    void Update()
    {
/*        // search if recalibrate
        Npad.GetState(ref nState, nPadId, nStyle);

        if( nState.buttons!= NpadButton.None )
        {
            Debug.Log("GetState on mapping : " + nState.buttons.ToString());
        }

        if ((nState.buttons & NpadButton.ZR) != 0 || ((nState.buttons & NpadButton.ZL) != 0 ))
        {
            if( playerInput.m_fireDlg==null )
            {
                RRInputManager.instance.ManageInput(RRInputManager.InputActionType.Fire);
            }
            else
            {
                playerInput.UpdateFire(RRPlayerInput.ButtonPhase.press);
            }
        }*/
    }

    internal void SetPlayerInput(RRPlayerInput rRPlayerInput)
    {
        playerInput = rRPlayerInput;
    }



#endif
}
