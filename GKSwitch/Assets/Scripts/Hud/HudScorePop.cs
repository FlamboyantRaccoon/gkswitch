using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class HudScorePop : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_scoreText;

    System.Action<HudScorePop> m_deleteAction;

    public void Setup(int nScore, 
        System.Action<HudScorePop> deleteAction,
        string sPrefix = "",
        string sSuffix = "",
        Material material = null)
    {
        if( material!=null )
        {
            m_scoreText.fontMaterial = material;
        }

        m_scoreText.text = sPrefix + nScore.ToString() + sSuffix;
        m_deleteAction = deleteAction;
    }

    public void OnEndAnimation()
    {
        if( m_deleteAction!=null )
        {
            m_deleteAction( this );
        }
        else
        {
            GameObject.Destroy(gameObject);
        }
    }
}
