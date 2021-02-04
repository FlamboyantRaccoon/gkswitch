using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_Candy : MonoBehaviour
{
    public TT_TrickOrTreat.CandyType candyType { get { return m_candyType; } }

    public enum CandyState { idle, blink, getout };

    [SerializeField]
    Animator m_animator;

    System.Action<TT_Candy> m_onEndTimeAction;
    System.Action<TT_Candy, TT_Toasty> m_onPickAction;
    System.Action<TT_Candy> m_onDelete;
    TT_TrickOrTreat.CandyType m_candyType;
    private CandyState m_state;
    private bool m_bAlreadyTake = false;

    private float m_fDisappearTimer;
    
    public void Setup( TT_TrickOrTreat.CandyType _candyType, float fShowTime, System.Action<TT_Candy> onEndTimeAction,
                        System.Action<TT_Candy, TT_Toasty> onPickAction, System.Action<TT_Candy> onDelete)
    {
        m_candyType = _candyType;
        m_onEndTimeAction = onEndTimeAction;
        m_onPickAction = onPickAction;
        m_onDelete = onDelete;
        m_fDisappearTimer = fShowTime == -1f ? -1f : Time.realtimeSinceStartup + fShowTime;
        m_state = CandyState.idle;
        m_bAlreadyTake = false;
    }

    public void SetOut( )
    {
        if( m_state!= CandyState.getout )
        {
            m_state = CandyState.getout;
            m_animator.SetTrigger("out");
        }
    }

    private void Update()
    {
        if (m_fDisappearTimer > 0f && m_state != CandyState.getout )
        {
            float fRemainTime = m_fDisappearTimer - Time.realtimeSinceStartup;
            if (fRemainTime < 0f )
            {
                if (m_onEndTimeAction != null)
                {
                    m_onEndTimeAction(this);
                }
                else
                {
                    GameObject.Destroy(this.gameObject);
                }
            }
            else if (fRemainTime < 1f && m_state != CandyState.blink )
            {
                m_state = CandyState.blink;
                m_animator.SetTrigger("blink");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(m_bAlreadyTake  )
        {
            return;
        }

        TT_Toasty toasty = collision.gameObject.GetComponent<TT_Toasty>();
        if( toasty==null )
        {
            return;
        }

        m_bAlreadyTake = true;
        if (m_onPickAction != null)
        {
            m_onPickAction(this, toasty );
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void OnEndOutAnim()
    {
        if( m_onDelete!=null )
        {
            m_onDelete(this);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }
}
