using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_MobileObstacle : SA_Obstacle
{
    
    private Vector2[] m_vPath;
    private int m_nCurrentCell;
    private int m_nNextCell;
    private bool m_bReverse;
    private float m_fSpeed;
    private bool m_bLoop;

    public void Setup(SA_SnowArena.ObstacleType _obstacleType, 
        System.Func<bool> canMove, 
        System.Action<SA_Obstacle, SA_Snowball> onBallHit,
        Vector2[] path, int nStartCell, bool bReverse, float fSpeed ) 
    {
        m_vPath = path;
        m_nCurrentCell = nStartCell;
        m_bReverse = bReverse;
        m_fSpeed = fSpeed;
        m_bLoop = path[0].x == path[path.Length - 1].x && path[0].y == path[path.Length - 1].y;
        ComputeNextCell();
        base.Setup(_obstacleType, canMove, onBallHit);
    }

    protected override void Update()
    {
        if (m_canMove == null || !m_canMove())
        {
            return;
        }

        float fMagnitude = m_fSpeed * Time.deltaTime;

        Vector3 vPos = transform.position;
        vPos.z = 0f;
        while( fMagnitude > 0 )
        {
            Vector3 vTarget = m_vPath[m_nNextCell];
            Vector3 vDir = vTarget - vPos;

            if( vDir.magnitude > fMagnitude )
            {
                vPos = vPos + vDir.normalized * fMagnitude;
                fMagnitude = 0f;
            }
            else
            {
                vPos = vTarget;
                fMagnitude -= vDir.magnitude;
                m_nCurrentCell = m_nNextCell;
                ComputeNextCell();
            }
        }
        vPos.z = -(SA_SnowArena.s_gameArea.y - vPos.y) * 10f / SA_SnowArena.s_gameArea.height;
        transform.position = vPos;
    }

    private void ComputeNextCell()
    {
        if( m_bReverse )
        {
            if( m_nCurrentCell == 0 )
            {
                if( m_bLoop )
                {
                    m_nNextCell = m_vPath.Length - 2;
                }
                else
                {
                    m_bReverse = !m_bReverse;
                    m_nNextCell = 1;
                }
            }
            else
            {
                m_nNextCell = m_nCurrentCell - 1;
            }
        }
        else
        {
            if (m_nCurrentCell == m_vPath.Length - 1)
            {
                if (m_bLoop)
                {
                    m_nNextCell = 1;
                }
                else
                {
                    m_bReverse = !m_bReverse;
                    m_nNextCell = m_vPath.Length - 2;
                }
            }
            else
            {
                m_nNextCell = m_nCurrentCell + 1;
            }
        }

    }

}
