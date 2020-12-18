using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ResultHudRank : MonoBehaviour
{
    [SerializeField]
    private Image m_avatar = null;
    [SerializeField]
    private TMP_Text m_score = null;

    public void Setup( GKPlayerData playerData )
    {
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;
        m_avatar.sprite = toasties.GetToasty(playerData.sToastyId).avatar;

        m_score.text = (playerData.m_totalScore + playerData.m_currentScore).ToString();
        Color color = GameContext.instance.m_settings.playerSettings[playerData.Id].color;
        m_score.color = color;
    }
}
