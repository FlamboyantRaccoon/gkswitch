using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_PlayerInfos
{
    private const int POSITION_COMPUTE_NEED = 5;
    private const float MIN_MAGNITUDE = 50;
    private const float MIN_TIME_BETWEEN_THROWN = 0.3f;

    private int m_playerId;
    public SA_Snowball m_ball;

    public Rect m_zoneRect;

    private Vector3 m_vLastPos;
    private List<Vector3> m_positions = new List<Vector3>();
    private float m_fNextThrownTimer = -1f;

    public void Setup(int playerId)
    {
        m_playerId = playerId;
        m_vLastPos = Vector3.zero;
    }

    internal void Clean()
    {

    }

    internal void SetBall( SA_Snowball snowball )
    {
        m_ball = snowball;
        if (m_vLastPos == Vector3.zero)
        {
            m_vLastPos.x = m_zoneRect.x + m_zoneRect.width * 0.5f;
            m_vLastPos.y = m_zoneRect.y + m_zoneRect.height * 0.5f;
        }
        m_ball.transform.position = m_vLastPos;
    }

    internal void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        Vector3 v3 = new Vector3(m_zoneRect.x + m_zoneRect.width * v.x,
                                    m_vLastPos.y = m_zoneRect.y + m_zoneRect.height * v.y,
                                    0f);

        if( m_positions.Count > POSITION_COMPUTE_NEED )
        {
            m_positions.RemoveAt(0);
        }
        m_positions.Add(v3);

        m_vLastPos = v3;

        if( m_ball!=null)
        {
            bool thrown = false;
            Vector3 dir = Vector3.zero;
            if ( m_positions.Count >= POSITION_COMPUTE_NEED && Time.time >m_fNextThrownTimer)
            {
                dir = m_positions[m_positions.Count - 1] - m_positions[0];
                if( dir.y > 0 && dir.magnitude>= MIN_MAGNITUDE )
                {
                    thrown = true;
                }
            }

            if( thrown )
            {
                SA_Snowball ball = m_ball;
                m_ball = null;
                ball.ThrownBall(dir);
                m_positions.Clear();
                m_fNextThrownTimer = Time.time + MIN_TIME_BETWEEN_THROWN;
            }
            else
            {
                m_ball.transform.position = m_vLastPos;
            }
        }
    }
}
