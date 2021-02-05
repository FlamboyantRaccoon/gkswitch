using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PK_Basket : MonoBehaviour
{
    [SerializeField]
    Animator m_animator;

    public System.Action<PK_Basket> onDeleteAction { set { m_onDeleteAction = value; } }

    private System.Action<PK_Basket> m_onDeleteAction;
    public int playerId { get; set; }


    public void PlayAnim(Vector3 vPos)
    {
        vPos.y = 130f;
        transform.position = vPos;
        m_animator.Play("Basket_on");
    }

    public void OnEndAnim()
    {
        if (m_onDeleteAction != null)
        {
            m_onDeleteAction(this);
        }
    }

    internal void PlayPickAnim()
    {
        m_animator.SetTrigger("pick");
    }
}
