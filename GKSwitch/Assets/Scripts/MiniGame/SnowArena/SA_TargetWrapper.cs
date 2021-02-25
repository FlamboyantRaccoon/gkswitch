using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SA_TargetWrapper : MonoBehaviour
{
    [SerializeField]
    SA_Target m_target;

    public void OnEndTouchAnim()
    {
        if( m_target!=null )
        {
            m_target.OnEndAnim();
        }
    }
}
