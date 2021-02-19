using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ExpressionSlot : MonoBehaviour
{
    [SerializeField]
    private Animator m_animator;
    [SerializeField]
    private Image[] m_expressionImg;
    [SerializeField]
    private TMP_Text m_wantedPrice;

    public ushort nExpressionMask { get { return m_nExpressionMask; } }

    private ushort m_nExpressionMask;

    public void Setup(ushort nExpressionMask, Sprite[] spritesArray, int nPointsWin)
    {
        m_nExpressionMask = nExpressionMask;
        
        Debug.Assert(spritesArray.Length == m_expressionImg.Length);
        for( int i=0; i<spritesArray.Length; i++ )
        {
            if( spritesArray[i]!=null )
            {
                m_expressionImg[i].sprite = spritesArray[i];
            }
        }

        m_wantedPrice.text = lwLanguageManager.instance.GetString("str_expressions_reward").Replace("%VALUE%", nPointsWin.ToString());

    }

    public void PlayGoodAnim()
    {
        m_animator.SetTrigger("Found");
    }
}
