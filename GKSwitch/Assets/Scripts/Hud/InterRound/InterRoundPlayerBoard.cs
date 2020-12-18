using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InterRoundPlayerBoard : MonoBehaviour
{
    [SerializeField]
    private Image m_avatar;
    [SerializeField]
    private Image m_bkgImage;
    [SerializeField]
    private TMP_Text m_score;

    public void Setup( GKPlayerData gKPlayerData )
    {
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;
        m_avatar.sprite = toasties.GetToasty(gKPlayerData.sToastyId).avatar;

        m_score.text = ( gKPlayerData.m_totalScore + gKPlayerData.m_currentScore ).ToString();
        Color color = GameContext.instance.m_settings.playerSettings[gKPlayerData.Id].color;
        color.a = m_bkgImage.color.a;
        m_bkgImage.color = color;
    }

}
