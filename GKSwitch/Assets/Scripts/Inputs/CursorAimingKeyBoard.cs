using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorAimingKeyBoard : CursorAiming
{
    Vector2 m_position = Vector2.zero;
    float m_fCoeff = 0.01f;


    public override void UpdateVector(Vector2 v)
    {
        m_position = m_position + v * m_fCoeff;
        m_position.x = Mathf.Clamp(m_position.x, -1f, 1f);
        m_position.y = Mathf.Clamp(m_position.y, -1f, 1f);
        m_onPositionChangeDlg?.Invoke(playerId, m_position);
    }

    public override Vector2 GetCursorPos()
    {
        return m_position;
    }
}
