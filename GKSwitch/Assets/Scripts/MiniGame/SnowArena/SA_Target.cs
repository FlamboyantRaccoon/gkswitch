using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SA_Target : MonoBehaviour
{
    public enum TargetState { idle, blink, getout };

    [SerializeField]
    Animator m_animator;
    [SerializeField]
    SpriteRenderer m_image;
    [SerializeField]
    Sprite[] m_normalGoldSprite;


    System.Action<SA_Target> m_onEndTimeAction;
    System.Action<SA_Target, SA_Snowball> m_onPickAction;
    System.Action<SA_Target> m_onDelete;

    bool m_bTouched;
    public int m_nCellId;
    public bool m_bGold;
    private TargetState m_state;

    private float m_fDisappearTimer;

    public void Setup(System.Action<SA_Target> onEndTimeAction, int nCellId, bool bGold, float fShowTime, 
                        System.Action<SA_Target, SA_Snowball> onPickAction, System.Action<SA_Target> onDelete)
    {
        m_onEndTimeAction = onEndTimeAction;
        m_onPickAction = onPickAction;
        m_onDelete = onDelete;
        m_nCellId = nCellId;
        m_bGold = bGold;
        m_state = TargetState.idle;
        m_fDisappearTimer = fShowTime == -1f ? -1f : Time.realtimeSinceStartup + fShowTime;
        m_image.sprite = m_normalGoldSprite[bGold ? 1 : 0];
        m_bTouched = false;
    }

    private void Update()
    {
        if (m_fDisappearTimer > 0f && m_state!= TargetState.getout )
        {
            float fRemainTime = m_fDisappearTimer - Time.realtimeSinceStartup;
            if (fRemainTime < 0f)
            {
                m_state = TargetState.getout;
                if (m_onEndTimeAction != null)
                {
                    m_onEndTimeAction(this);
                }
                else
                {
                    GameObject.Destroy(this.gameObject);
                }
            }
            else if (fRemainTime < 1f && m_state != TargetState.blink)
            {
                m_state = TargetState.blink;
                m_animator.SetTrigger("Target_blink");
            }
        }
    }

    public void OnEndAnim()
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

    public void Hit()
    {

        m_animator.SetTrigger("Touched");
    }

    public void GetOut()
    {
        m_animator.SetTrigger("Target_out");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if( m_bTouched )
        {
            return;
        }
        m_bTouched = true;
        if (m_onPickAction != null)
        {
            SA_Snowball snowball = collision.gameObject.GetComponent<SA_Snowball>();
            m_onPickAction(this, snowball );
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }

    }
}
