using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_SnowBallWrapper : MonoBehaviour
{
    [SerializeField]
    SA_Snowball m_ball;

    public void OnEndTouchAnim()
    {
        if (m_ball != null)
        {
            m_ball.OnEndAnim();
        }
    }
}
