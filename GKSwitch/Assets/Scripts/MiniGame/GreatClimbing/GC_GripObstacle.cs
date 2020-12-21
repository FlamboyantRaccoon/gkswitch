using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC_GripObstacle : MonoBehaviour
{
    public System.Action m_onEndTouch;

    public void OnEndTouchAnimation()
    {
        if(m_onEndTouch!=null )
        {
            m_onEndTouch();
        }
    }
}
