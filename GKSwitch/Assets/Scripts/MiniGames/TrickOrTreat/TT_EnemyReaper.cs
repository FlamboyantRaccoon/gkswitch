using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_EnemyReaper : TT_Enemy
{

    // Update is called once per frame
    protected override void Update ()
    {
        if (m_canMove == null || !m_canMove())
        {
            return;
        }

        Transform nearest = GetNearestTarget();
        if (nearest != null)
        {
            Vector3 vDir = nearest.position - transform.position;
            vDir.z = 0f;
            vDir = vDir.normalized * m_fSpeed * Time.deltaTime;
            Vector3 vScale = transform.localScale;
            vScale.x = Mathf.Abs(vScale.x) * Mathf.Sign(vDir.x);
            transform.localScale = vScale;
            transform.position = transform.position + vDir;
        }

    }
}
