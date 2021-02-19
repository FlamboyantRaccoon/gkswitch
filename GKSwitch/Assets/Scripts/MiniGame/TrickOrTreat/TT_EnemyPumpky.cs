using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_EnemyPumpky : TT_Enemy
{

    // Update is called once per frame
    protected override void Update()
    {
        if (m_canMove == null || !m_canMove())
        {
            return;
        }

        float fDist = m_fSpeed * Time.deltaTime;
        Vector3 vPos = transform.position;
        Vector3 vNewPos = transform.position;
        bool bBounce = false;
        while (fDist > 0)
        {
            Vector3 vDir = m_vDirection.normalized * fDist;
            vPos += vDir;

            if (TT_TrickOrTreat.IsPointOutGameArea(transform.position, ref vDir, ref vNewPos))
            {
                m_vDirection = vDir;
                fDist -= (vNewPos - vPos).magnitude;
                vPos = vNewPos;
                bBounce = true;
            }
            else
            {
                fDist = 0f;
            }
        }

        if( bBounce )
        {
            m_animator.SetTrigger("bouncing");
        }

        transform.position = vPos;
    }
}
