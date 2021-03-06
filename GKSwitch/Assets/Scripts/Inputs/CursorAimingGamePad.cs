using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorAimingGamePad : CursorAiming
{
    Vector2 m_position = Vector2.zero;
    
    public override void UpdateVector(Vector2 v)
    {
        m_position = v;
        m_onPositionChangeDlg?.Invoke(playerId, m_position);
    }

    public override Vector2 GetCursorPos()
    {
        return m_position;
    }
}
