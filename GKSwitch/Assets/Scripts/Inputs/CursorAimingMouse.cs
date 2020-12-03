using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorAimingMouse : CursorAiming
{
    Vector2 m_position = Vector2.zero;

    public override void UpdateMousePosition(Vector2 v)
    {
        m_position = v;
        float fHalfWidth = Screen.width / 2f;
        float fHalfHeight = Screen.height / 2f;
        m_position.x = (v.x - fHalfWidth) / fHalfWidth;
        m_position.y = (v.y - fHalfHeight) / fHalfHeight;
    }

    public override Vector2 GetCursorPos()
    {
        return m_position;
    }
}
