using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_GhostWrapper : MonoBehaviour
{
    [SerializeField]
    TT_EnemyGhost m_ghost;

    public void OnInAnimEnd()
    {
        if (m_ghost != null)
        {
            m_ghost.OnInAnimEnd();
        }
    }

    public void OnOutAnimEnd()
    {
        if (m_ghost != null)
        {
            m_ghost.OnOutAnimEnd();
        }
    }

}
