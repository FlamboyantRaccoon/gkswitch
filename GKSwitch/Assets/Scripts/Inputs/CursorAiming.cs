using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorAiming : MonoBehaviour
{
    private const int SENSIBILITY_MIN = 0;
    private const int SENSIBILITY_MAX = 10;

    public int sensibility { get { return m_sensibility; } }


    protected int m_sensibility = 5;

    public bool IncreaseSensibility(int increment)
    {
        int newValue = Mathf.Clamp(m_sensibility + increment, SENSIBILITY_MIN, SENSIBILITY_MAX);
        if (newValue == m_sensibility)
        {
            return false;
        }
        m_sensibility = newValue;
        UpdateSensibility();
        return true;
    }

    public virtual void UpdateVector( Vector2 v)
    {

    }

    public virtual void UpdateMousePosition(Vector2 v)
    {

    }

    public virtual Vector2 GetCursorPos()
    {
        return Vector2.zero;
    }

    public virtual void UpdateSensibility()
    {

    }
}
