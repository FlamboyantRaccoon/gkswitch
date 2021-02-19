using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_EnemyGhost : TT_Enemy
{
    private enum ghostState { appearing, moving, leaving };

    private ghostState m_ghostState;
    private BoxCollider2D m_boxCollider;
    private float m_width = 0f;
    private float m_xGap = 0f;

    public void Awake()
    {
        m_boxCollider = gameObject.GetComponent<BoxCollider2D>();
        m_width = m_boxCollider.size.x;
    }

    public override void Setup(TT_TrickOrTreat.EnemyType _enemyType, Vector3 vDir, System.Func<bool> canMove, System.Action<TT_Enemy> onEndAction,
                        System.Action<TT_Enemy, TT_Toasty> onToastyHit, float fSpeed)
    {
        base.Setup(_enemyType, vDir, canMove, onEndAction, onToastyHit, fSpeed);

        m_xGap = m_width / 2 * Mathf.Abs(transform.localScale.x);
        Vector3 vPos = transform.position + Vector3.right * m_xGap * ( vDir.x > 0 ? -1f : 1f);
        transform.position = vPos;
        m_boxCollider.enabled = false;
        m_ghostState = ghostState.appearing;
    }

    // Update is called once per frame
    protected override void Update()
    {
        if (m_canMove == null || !m_canMove() || m_ghostState!= ghostState.moving )
        {
            return;
        }

        float fDist = m_fSpeed * Time.deltaTime;
        Vector3 vPos = transform.position;
        Vector3 vNewPos = transform.position;
 
        Vector3 vDir = m_vDirection.normalized * fDist;
        
        vPos += vDir;

        if( ( vDir.x > 0 && vPos.x > TT_TrickOrTreat.s_gameArea.x + TT_TrickOrTreat.s_gameArea.width + m_xGap) ||
            (vDir.x < 0 && vPos.x < TT_TrickOrTreat.s_gameArea.x - m_xGap) )
        //if (TT_TrickOrTreat.IsPointOutGameArea(transform.position, ref vDir, ref vNewPos))
        {
            Disappear();
            return;
        }
        transform.position = vPos;
    }

    public void OnInAnimEnd()
    {
        if( m_ghostState!= ghostState.leaving )
        {
            m_ghostState = ghostState.moving;
            m_boxCollider.enabled = true;
        }
    }

    public void OnOutAnimEnd()
    {
        if (m_onEndAction != null)
        {
            m_onEndAction(this);
        }
    }

    private void Disappear()
    {
        m_animator.SetTrigger("out");
        m_boxCollider.enabled = false;
        m_ghostState = ghostState.leaving;
    }

    public override void Kill()
    {
        if( m_ghostState!= ghostState.leaving )
        {
            m_boxCollider.enabled = false;
            m_animator.SetTrigger("outCandy");
            m_ghostState = ghostState.leaving;
        }
    }
}