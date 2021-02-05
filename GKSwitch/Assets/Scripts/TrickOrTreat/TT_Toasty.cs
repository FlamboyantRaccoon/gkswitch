using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class TT_Toasty : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField]
    Animator m_animator = null;
    [SerializeField]
    SpriteRenderer m_sprite = null;

    public static TT_Toasty sDraggedElt = null;

    public System.Func<bool> canMove { set { m_canMove = value; } }
    public float fInvincibleTime { set { m_fInvincibleTime = value; } }
    public int playerId { get; set; }


    System.Func<bool> m_canMove;
    private Vector3 _startPosition;
    private Vector3 _offsetToMouse;
    private float m_fHitTimer = -1f;
    private float m_fInvincibleTime;

    // Use this for initialization
    void Start ()
    {
        m_animator.SetBool("Invincibility", false);
    }
	
	// Update is called once per frame
	void Update ()
    {
        if(m_fHitTimer > 0 )
        {
            if( Time.time > m_fHitTimer )
            {
                m_fHitTimer = -1f;
                m_animator.SetBool("Invincibility", false );
            }
        }
		
	}

    public void GetCandy()
    {
        m_animator.SetTrigger("GetCandy");
    }

    public void GetHit()
    {
        m_fHitTimer = Time.time + m_fInvincibleTime;
        m_animator.SetBool("Invincibility", true);
        m_animator.SetTrigger("Wrong");
    }

    public bool IsInvincible()
    {
        return m_fHitTimer > 0f;
    }

    public void UpdatePosition( Vector2 v )
    {
        Vector3 worldPos = Camera.main.ViewportToWorldPoint(new Vector3(v.x, v.y, 0));
        worldPos.z = TT_TrickOrTreat.TOASTY_Z;
        transform.position = worldPos;
    }

    void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        SetupBeginDrag(eventData);
    }

    void SetupBeginDrag(PointerEventData eventData)
    {
        if (sDraggedElt != null)
        {
            return;
        }

        if (m_canMove != null && m_canMove())
        {
            _startPosition = transform.position;
            _offsetToMouse = _startPosition - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)
            );

        }


        m_animator.SetBool("Hold", true);
        sDraggedElt = this;
    }

    void IDragHandler.OnDrag(PointerEventData eventData)
    {
        if (sDraggedElt != this)
        {
            SetupBeginDrag(eventData);
            return;
        }

        if (m_canMove != null && m_canMove())
        {
            //        Debug.Log("OnDrag");
            Vector3 vPos = Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, +30f)
                ) + _offsetToMouse;
            vPos.z = TT_TrickOrTreat.TOASTY_Z;
            transform.position = vPos;
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (sDraggedElt != this)
        {
            return;
        }
        m_animator.SetBool("Hold", false);
        sDraggedElt = null;
        _offsetToMouse = Vector3.zero;
    }

    internal void SetAvatar(Sprite avatar)
    {
        if(m_sprite!=null )
        {
            m_sprite.sprite = avatar;
        }
    }
}
