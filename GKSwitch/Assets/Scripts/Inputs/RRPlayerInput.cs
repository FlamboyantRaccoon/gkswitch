﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class RRPlayerInput : MonoBehaviour
{
    public enum ButtonPhase { off, press, on, release }

    [SerializeField]
    private PlayerInput m_playerInput;

    public delegate void InputDelegate(InputAction.CallbackContext context);
    public delegate void UpdateCursorDlg(int playerId, Vector2 v);
    public delegate void UpdateFireDlg(int playerId, Vector2 v, ButtonPhase phase);

    public InputDelegate m_inputDlg;
    public RRInputManager.ManageInputDelegate m_inputActionDlg;
    public UpdateCursorDlg m_updateCursorDlg;
    public UpdateFireDlg m_fireDlg;

    public int Id { get { return m_nId; } }

    private bool m_bOnFire = false;

#if UNITY_SWITCH
    public nn.hid.NpadId nPadId { get; set; }
#endif

    private int m_nId = -1;
    private CursorAiming m_cursorAiming;

    public void OnEnable()
    {
        Debug.Log("####### RRPlayerInput created !!! " + m_playerInput.currentControlScheme );
        m_nId = RRInputManager.instance.AddPlayerInput(this);

        switch( m_playerInput.currentControlScheme )
        {
            case "Keyboard":
                {
                    m_cursorAiming = gameObject.AddComponent<CursorAimingMouse>();
                }
                break;
            case "GamePad":
                {
                    m_cursorAiming = gameObject.AddComponent<CursorAimingGamePad>();
                }
                break;
            case "SwitchPro":
                {
                    m_cursorAiming = gameObject.AddComponent<CursorAimingJoycon>();
                    ((CursorAimingJoycon)m_cursorAiming).SetNpadId(nPadId);
                }
                break;
        }
    }

    public void Update()
    {
        if( m_cursorAiming!=null )
        {
            m_updateCursorDlg?.Invoke(Id, m_cursorAiming.GetCursorPos());
        }

        if( m_bOnFire )
        {
            UpdateFire( ButtonPhase.on );
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Debug.Log("####### OnMove " );
        if (m_inputDlg!=null )
        {
            m_inputDlg(context);
        }
        else
        {
            RRInputManager.instance.Move(context);
        }
    }

    public void OnRightStick(InputAction.CallbackContext context)
    {
        Vector2 v2 = context.ReadValue<Vector2>();
        if( m_cursorAiming!=null )
        {
            m_cursorAiming.UpdateVector(v2);
        }
    }

    public void OnMouseMove(InputAction.CallbackContext context)
    {
        Vector2 v2 = context.ReadValue<Vector2>();
        //Debug.Log("OnMouseMove " + v2);
        if (m_cursorAiming != null)
        {
            m_cursorAiming.UpdateMousePosition(v2);
        }
    }

    public void OnFire(InputAction.CallbackContext context)
    {
        Debug.Log("OnFire " + context.action + " // " + context.phase + " // " + context.interaction );
        ButtonPhase phase = ButtonPhase.off;
        switch( context.phase )
        {
            case InputActionPhase.Performed:
                {
                    phase = ButtonPhase.on;
                }
                break;
            case InputActionPhase.Started:
                {
                    phase = ButtonPhase.press;
                }
                break;
            default:
                phase = ButtonPhase.release;
                break;
        }

        m_bOnFire = phase == ButtonPhase.on;
        UpdateFire(phase);
    }

    private void UpdateFire( ButtonPhase phase )
    {
        Vector2 vScreenPos = Vector2.zero;
        if (m_cursorAiming != null)
        {
            vScreenPos = (m_cursorAiming.GetCursorPos() + new Vector2(1f, 1f)) / 2f;
        }
        m_fireDlg?.Invoke(Id, vScreenPos, phase);
    }

    public void OnButton(InputAction.CallbackContext context)
    {
        Debug.Log("OnButton : " + context.control.name + " /// " + context.control.displayName);
        if (m_inputDlg != null)
        {
            m_inputDlg(context);
        }
        else
        {
            RRInputManager.instance.Manageinput(context);
        }
    }

    internal bool ManageInput(RRInputManager.InputActionType inputActionType)
    {
        if( m_inputActionDlg!=null )
        {
            return m_inputActionDlg(inputActionType);
        }
        return false;
    }

    internal void CreateSwitchAiming()
    {
        m_cursorAiming = gameObject.AddComponent<CursorAimingJoycon>();
        ((CursorAimingJoycon)m_cursorAiming).SetNpadId(nPadId);
    }
}