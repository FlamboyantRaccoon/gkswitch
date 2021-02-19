using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TT_CandyWrapper : MonoBehaviour
{
    [SerializeField]
    TT_Candy m_candy;

    public void OnEndOutAnim()
    {
        m_candy.OnEndOutAnim();
    }
}
