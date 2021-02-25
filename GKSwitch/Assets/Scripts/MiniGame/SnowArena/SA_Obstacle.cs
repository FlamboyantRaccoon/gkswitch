using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_Obstacle : MonoBehaviour
{
    public SA_SnowArena.ObstacleType obstacleType { get { return m_obstacleType; } }

    [SerializeField]
    protected Animator m_animator;

    protected System.Action<SA_Obstacle, SA_Snowball> m_onBallHit;
    protected System.Func<bool> m_canMove;
    protected SA_SnowArena.ObstacleType m_obstacleType;
    
    public void Setup(SA_SnowArena.ObstacleType _obstacleType, System.Func<bool> canMove, System.Action<SA_Obstacle, SA_Snowball> onBallHit )
    {
        m_obstacleType = _obstacleType;
        m_onBallHit = onBallHit;
        m_canMove = canMove;
    }

    protected virtual void Update()
    {
        if (m_canMove == null || !m_canMove())
        {
            return;
        }
    }

    public void CatchBall()
    {
        if (m_animator != null)
        {
            m_animator.SetTrigger("gotcha");
        }
    }

    public virtual void Kill()
    {
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_onBallHit != null)
        {
            SA_Snowball snowball = collision.gameObject.GetComponent<SA_Snowball>();

            m_onBallHit(this, snowball );
        }
    }
    
}
