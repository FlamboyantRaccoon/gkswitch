using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_Enemy : MonoBehaviour
{
    public TT_TrickOrTreat.EnemyType enemyType { get { return m_enemyType; } }

    [SerializeField]
    protected Animator m_animator;

    protected System.Action<TT_Enemy> m_onEndAction;
    protected System.Action<TT_Enemy, TT_Toasty> m_onToastyHit;
    protected System.Func<bool> m_canMove;
    protected TT_TrickOrTreat.EnemyType m_enemyType;
    protected Transform[] m_target;
    protected Vector3 m_vDirection;
    protected float m_fSpeed = 100f;

    public void Setup(TT_TrickOrTreat.EnemyType _enemyType, Transform[] target, System.Func<bool> canMove, System.Action<TT_Enemy> onEndAction,
                        System.Action<TT_Enemy, TT_Toasty> onToastyHit, float fSpeed )
    {
        m_enemyType = _enemyType;
        m_onEndAction = onEndAction;
        m_onToastyHit = onToastyHit;
        m_canMove = canMove;
        m_target = target;
        m_fSpeed = fSpeed;
        m_vDirection = Vector3.zero;
    }

    public virtual void Setup(TT_TrickOrTreat.EnemyType _enemyType, Vector3 vDir, System.Func<bool> canMove, System.Action<TT_Enemy> onEndAction,
                        System.Action<TT_Enemy, TT_Toasty> onToastyHit, float fSpeed)
    {
        m_enemyType = _enemyType;
        m_onEndAction = onEndAction;
        m_onToastyHit = onToastyHit;
        m_canMove = canMove;
        m_target = null;
        m_fSpeed = fSpeed;
        m_vDirection = vDir;
    }

    protected virtual void Update()
    {
        if( m_canMove==null || !m_canMove() )
        {
            return;
        }

        Transform nearest = GetNearestTarget();
        if (nearest != null )
        {
            Vector3 vDir = nearest.position - transform.position;
            vDir.z = 0f;
            vDir = vDir.normalized * m_fSpeed* Time.deltaTime;
            transform.position = transform.position + vDir;
        }

        if( m_vDirection.x !=0 || m_vDirection.y !=0 )
        {
            float fDist = m_fSpeed * Time.deltaTime;
            Vector3 vPos = transform.position;
            Vector3 vNewPos = transform.position;

            while ( fDist > 0)
            {
                Vector3 vDir = m_vDirection.normalized * fDist;
                vPos += vDir;

                if (TT_TrickOrTreat.IsPointOutGameArea(transform.position, ref vDir, ref vNewPos))
                {
                    if( m_enemyType == TT_TrickOrTreat.EnemyType.ghosty )
                    {
                        if( m_onEndAction!=null )
                        {
                            m_onEndAction(this);
                            return;
                        }
                    }
                    else
                    {
                        m_vDirection = vDir;
                        fDist -= (vNewPos - vPos).magnitude;
                        vPos = vNewPos;
                    }
                }
                else
                {
                    fDist = 0f;
                }
            }

            transform.position = vPos;
        }
    }

    public void CatchPlayer()
    {
        if( m_animator!=null )
        {
            m_animator.SetTrigger("gotcha");
        }
    }

    public virtual void Kill()
    {
    }

    protected Transform GetNearestTarget()
    {
        if( m_target==null)
        {
            return null;
        }
        float squareDistance = -1f;
        Transform nearest = null;
        for( int i=0; i<m_target.Length;i++)
        {
            float sqrDist = Vector3.SqrMagnitude(m_target[i].position - transform.position);
            if( nearest==null || sqrDist<squareDistance)
            {
                nearest = m_target[i];
                squareDistance = sqrDist;
            }
        }
        return nearest;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        TT_Toasty toasty = collision.gameObject.GetComponent<TT_Toasty>();
        if (toasty == null)
        {
            return;
        }

        if (m_onToastyHit!=null )
        {
            m_onToastyHit?.Invoke(this, toasty);
        }
    }
}
