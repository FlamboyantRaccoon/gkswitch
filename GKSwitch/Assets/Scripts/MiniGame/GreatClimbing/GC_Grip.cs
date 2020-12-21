using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GC_Grip : MonoBehaviour {

    public bool isAvailable { get { return m_gripData.nObstacleLife - m_gripData.nHit <= 0; } }

    [SerializeField]
    GC_GripImg m_gripImg;
    [SerializeField]
    GameObject m_obstacleRoot;
    [SerializeField]
    GameObject[] m_obstacleLife;
    [SerializeField]
    Animator m_gripAnimator;
    [SerializeField]
    Animator m_obstacleAnimator;
    [SerializeField]
    GC_GripObstacle m_obstacle;

    private int m_xObstacleMask;
    private System.Action<int> m_onObstacleTouch;

    private GC_GreatClimbing.GripData m_gripData;

    private void Awake()
    {
        m_obstacle.m_onEndTouch = OnEndTouch;
    }

    public void Setup( GC_GreatClimbing.GripData gripData, GC_GripImg.OnGripDlg onGripDlg, System.Action<int> onObstacleTouch)
    {
        m_gripData = gripData;
        m_gripImg.Init(m_gripData.nGripId, this );
        m_gripImg.onGripDlg = onGripDlg;

        m_onObstacleTouch = onObstacleTouch;
        m_obstacleRoot.SetActive( m_gripData.nObstacleLife > 0 );
        if(m_gripData.nObstacleLife > 0 )
        {
            int nRnd = Random.Range(0, 3);
            switch ( m_gripData.nObstacleLife )
            {
                case 1:
                    m_xObstacleMask = 1 << nRnd;
                    break;
                case 2:
                    m_xObstacleMask = 7 - (1 << nRnd);
                    break;
                case 3:
                    m_xObstacleMask = 7;
                    break;
            }
        }

        for( int nObstacleId=0; nObstacleId<m_obstacleLife.Length; nObstacleId++ )
        {
            bool bActive = (m_xObstacleMask & (1 << nObstacleId)) != 0;
            m_obstacleLife[nObstacleId].SetActive(bActive);
        }
    }

    public void SetHold( bool bHold )
    {
        m_gripAnimator.SetBool("Hold", bHold);
    }

    public bool IsTouch( Vector2 vPos )
    {
        // TODO check if finger still on grip
        return true;
    }

    public void HitObstacleLife(int playerId)
    {
        m_gripData.nHit++;
        m_obstacleAnimator.SetBool("IsDead", m_gripData.nObstacleLife - m_gripData.nHit <= 0);
        m_obstacleAnimator.SetTrigger("Touch");
        m_onObstacleTouch?.Invoke(playerId);
    }

    private void OnEndTouch()
    {
        int nRnd = Random.Range(0, 3);
        bool bDone = false;
        int nCounter = 0;
        while (!bDone && nCounter < 4)
        {
            if ((m_xObstacleMask & (1 << nRnd)) != 0)
            {
                m_xObstacleMask &= ~((1 << nRnd));
                m_obstacleLife[nRnd].SetActive(false);
                bDone = true;
            }
            else
            {
                nRnd = (nRnd + 1) % 3;
            }
            nCounter++;
        }
    }
}
