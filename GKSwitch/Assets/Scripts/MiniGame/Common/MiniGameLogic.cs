using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameLogic
{
    protected byte m_nGameDataId;
    protected float m_fGameStartTime;

    public float fGameStartTime { get { return m_fGameStartTime; } }

    internal void StartGameTime()
    {
        m_fGameStartTime = Time.time;
    }
}
