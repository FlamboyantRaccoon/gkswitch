using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IH_Player : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer m_sprite = null;

    private Rigidbody2D rigidbody;
    private Vector3 lastPos;
    private float m_speed = 1000f;
    private Vector3 m_vTarget = Vector3.zero;
    private Vector3 m_vMove = Vector3.zero;

    private void Awake()
    {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    internal void SetInitialPos( Vector3 v)
    {
        lastPos = v;
        rigidbody.MovePosition(v);
    }

    internal void Move( Vector3 v )
    {
        m_vTarget = v;

        Vector3 vCurrent = transform.position;
        Vector3 vDir = (m_vTarget - vCurrent);
        float fDist = (Time.deltaTime * m_speed);
        if (vDir.magnitude > fDist)
        {
            vDir = vDir.normalized * fDist;
        }
        m_vMove = vCurrent + vDir;

    }

    private void FixedUpdate()
    {
        if( m_vMove!=Vector3.zero )
        {
            rigidbody.velocity = Vector2.zero;
            rigidbody.MovePosition(m_vMove);
        }
    }

    internal void SetAvatar(Sprite avatar)
    {
        if (m_sprite != null)
        {
            m_sprite.sprite = avatar;
        }
    }
}
