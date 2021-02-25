using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SA_Snowball : MonoBehaviour
{
    
    public enum BallState { idle, hold, thrown, hit, disappear }

    [SerializeField]
    Animator m_animator;
    [SerializeField]
    CircleCollider2D m_collider;

    public static SA_Snowball sDraggedElt = null;

    System.Func<bool> m_canMove;
    System.Func<Vector3, bool> m_checkInLaunchArea;
    System.Action<int> m_onLaunchAction;
    System.Action<SA_Snowball> m_onDelete;

    public int playerId { get { return m_playerId; } }

    private Vector3 _startPosition;
    private Vector3 _offsetToMouse;
    private List<Vector3> m_vPosArray = new List<Vector3>();

    private BallState m_ballState;
    private Vector3 m_vDir;
    private float m_fSpeed;
    private float m_fSpeedMultiplier;

    private float m_colliderDefaultSize;
    private int m_nTargetTouch;

    private int m_playerId;

    // Use this for initialization
    void Awake()
    {
        m_colliderDefaultSize = m_collider.radius;
    }

    // Update is called once per frame
    void Update()
    {
        if( m_ballState== BallState.thrown )
        {
            UpdateThrown();

            if( transform.position.y > SA_SnowArena.s_gameArea.y || m_vDir.y < 0.25f )
            {
                m_ballState = BallState.disappear;
                m_animator.SetTrigger("Touch");
            }
        }
        else if( m_ballState == BallState.disappear )
        {
            UpdateThrown();
        }

    }

    public void Setup(int _playerId, System.Func<bool> canMove, System.Func<Vector3, bool> checkInLaunchArea, System.Action<int> onLaunchAction, System.Action<SA_Snowball> onDelete, float fSpeedMultiplier )
    {
        m_playerId = _playerId;
        m_canMove = canMove;
        m_checkInLaunchArea = checkInLaunchArea;
        m_onLaunchAction = onLaunchAction;
        m_onDelete = onDelete;
        m_ballState = BallState.idle;
        m_vPosArray.Clear();
        m_collider.radius = 3 * m_colliderDefaultSize;
        m_fSpeedMultiplier = fSpeedMultiplier;
        m_nTargetTouch = 0;
    }

    public void OnEndAnim()
    {
        if (m_onDelete != null)
        {
            m_onDelete(this);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void Hit( bool bGood )
    {
        if (m_ballState != BallState.thrown)
        {
            return;
        }

        m_ballState = BallState.hit;
        if ( bGood )
        {
            m_animator.SetTrigger("Touch");
        }
        else
        {
            m_animator.SetTrigger("TouchWrong");
        }
    }

    public int AddTargetTouch( )
    {
        m_nTargetTouch++;
        return m_nTargetTouch;
    }

 /*   void IBeginDragHandler.OnBeginDrag(PointerEventData eventData)
    {
        SetupBeginDrag(eventData);
    }

    void SetupBeginDrag(PointerEventData eventData)
    {
        if (sDraggedElt != null || m_ballState != BallState.idle )
        {
            return;
        }

        
        if (m_canMove != null && m_canMove())
        {
            m_ballState = BallState.hold;

            Vector3 vPos = transform.position;
            vPos.z = TT_TrickOrTreat.TOASTY_Z;
            m_vPosArray.Add( vPos );
            _startPosition = transform.position;
            _offsetToMouse = _startPosition - Camera.main.ScreenToWorldPoint(
                new Vector3(Input.mousePosition.x, Input.mousePosition.y, 0f)
            );

            m_animator.SetBool("Holding", true);
            sDraggedElt = this;
        }

        
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
            m_vPosArray.Add(vPos);

            if ( m_checkInLaunchArea!=null && !m_checkInLaunchArea(vPos ))
            {
                sDraggedElt = null;
                ThrownBall();
            }
            else
            {
                while (m_vPosArray.Count > 5)
                {
                    m_vPosArray.RemoveAt(0);
                }
            }
        }
    }

    void IEndDragHandler.OnEndDrag(PointerEventData eventData)
    {
        if (sDraggedElt != this)
        {
            return;
        }

        sDraggedElt = null;
        _offsetToMouse = Vector3.zero;

        ThrownBall();

    }

    private void ThrownBall()
    {
        m_animator.SetBool("Holding", false);
        if (m_vPosArray.Count >= 2)
        {
             m_vDir = m_vPosArray[m_vPosArray.Count - 1] - m_vPosArray[0];

            
            m_fSpeed = m_vDir.magnitude * (5 / m_vPosArray.Count) * m_fSpeedMultiplier;
            m_fSpeed = Mathf.Max(m_fSpeed, SA_SnowArena.s_snowBallMinSpeed);
            m_fSpeed = SA_SnowArena.s_snowBallMinSpeed;
           // Debug.Log("fSpeed " + m_fSpeed);
            m_vDir.Normalize();

            m_ballState = BallState.thrown;
            m_collider.radius = m_colliderDefaultSize;

            if ( m_onLaunchAction!=null)
            {
                m_onLaunchAction(m_playerId);
            }


        }
        else
        {
            m_ballState = BallState.idle;
        }
    }*/

    public void ThrownBall( Vector3 vDir )
    {
        m_animator.SetBool("Holding", false);
        m_vDir = vDir;
        m_fSpeed = m_vDir.magnitude * m_fSpeedMultiplier;
        m_fSpeed = Mathf.Max(m_fSpeed, SA_SnowArena.s_snowBallMinSpeed);
        m_fSpeed = SA_SnowArena.s_snowBallMinSpeed;
            // Debug.Log("fSpeed " + m_fSpeed);
        m_vDir.Normalize();

        m_ballState = BallState.thrown;
        m_collider.radius = m_colliderDefaultSize;

        m_onLaunchAction?.Invoke(m_playerId);
    }

    private void UpdateThrown()
    {
        //Vector3 vPos = transform.position + m_vDir * m_fSpeed * Time.deltaTime;
        //transform.position = vPos;

        float fDist = m_fSpeed * Time.deltaTime;
        Vector3 vPos = transform.position;
        Vector3 vNewPos = transform.position;

        //bool bBounce = false;
        while (fDist > 0)
        {
            Vector3 vDir = m_vDir.normalized * fDist;
            

            if (SA_SnowArena.IsPointOutGameArea(vPos, m_colliderDefaultSize * transform.localScale.x, ref vDir, ref vNewPos))
            {
                m_vDir = vDir;
                fDist -= (vNewPos - vPos).magnitude;
                vPos = vNewPos;
                //bBounce = true;
            }
            else
            {
                vPos += vDir;
                fDist = 0f;
            }
        }

        /*if (bBounce)
        {
            m_animator.SetTrigger("bouncing");
        }*/

        transform.position = vPos;

    }
}

