using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GC_GripImg : MonoBehaviour
{
    public delegate void OnGripDlg(int playerId, Vector2 vPos, GC_Grip grip);

    [SerializeField]
    private SpriteRenderer m_spriteRenderer;
    [SerializeField]
    private Sprite[] m_GripSpriteArray;

    private OnGripDlg m_onGripDlg;

    private GC_Grip m_grip;
    private CircleCollider2D m_circleCollider;
    private int m_nSpriteId = -1;

    public OnGripDlg onGripDlg { set { m_onGripDlg = value; } }

    public void Init( int nSpriteId, GC_Grip grip )
    {
        m_grip = grip;
        if( nSpriteId!=m_nSpriteId )
        {
            m_nSpriteId = nSpriteId;
            Debug.Assert(nSpriteId >= 0 && nSpriteId < m_GripSpriteArray.Length);
            m_spriteRenderer.sprite = m_GripSpriteArray[nSpriteId];

            if(m_circleCollider==null )
            {
                m_circleCollider = gameObject.GetComponent<CircleCollider2D>();
            }

            
            if (m_circleCollider != null)
            {
                m_circleCollider.radius = Mathf.Max(m_GripSpriteArray[nSpriteId].bounds.size.x, m_GripSpriteArray[nSpriteId].bounds.size.y) / 2f;
            }
        }
       
    }

   
    public void OnPlayerInput (int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if (m_grip.isAvailable)
        {
            m_onGripDlg?.Invoke(playerId, v, m_grip);
        }
        else
        {
            m_grip.HitObstacleLife(playerId);
        }
        /*        Touch[] touchs = Input.touches;
                foreach (var t in Input.touches)
                {
                    if (t.position == eventData.position)
                    {
                        Debug.Log("OnPointerDown! " + t.fingerId);
                    }
                }*/
    }
}
