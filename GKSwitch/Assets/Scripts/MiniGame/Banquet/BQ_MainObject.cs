using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_MainObject : MonoBehaviour
{
    [System.Serializable]
    public class OrderLayout
    {
        public BQ_OrderSpot[] m_orderSpots;
    }

    public BQ_Belt[] belt;
    public float m_fDistBetweenPlateAndBubble = 200f;
    public OrderLayout[] m_orderLayouts;

    public void SetSpeed( float fSpeed )
    {
        for( int i=0; i<belt.Length; i++ )
        {
            int sign = i % 2 == 1 ? 1 : -1;
            belt[i].SetSpeed(sign * fSpeed);
        }
    }
}
