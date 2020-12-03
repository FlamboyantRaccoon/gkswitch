using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameBasicHud : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_timer;

    public void UpdateTime(int nTime)
    {
        Debug.Assert(m_timer != null);
        m_timer.text = nTime == -1 ? "" : nTime.ToString();
    }
}
