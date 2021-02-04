using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_PlayerInfos
{
    private int m_playerId;
    public TT_Toasty m_toasty { private set; get; }


    public void Setup(int playerId)
    {
        m_playerId = playerId;
    }

    internal void Clean()
    {
        GameObject.Destroy(m_toasty.gameObject);
    }

    internal void SetToasty(TT_Toasty toasty)
    {
        m_toasty = toasty;
    }

    internal void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if( m_toasty!=null )
        {
            m_toasty.UpdatePosition(v);
        }
    }
}
