using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TutoPlayerBoard : MonoBehaviour
{
    [SerializeField]
    private Image m_avatar = null;
    [SerializeField]
    private Image m_cursor = null;
    [SerializeField]
    private TMP_Text m_label = null;
    [SerializeField]
    private GameObject m_valid = null;
    [SerializeField]
    private Slider m_sensibility = null;

    public void Setup( int playerIndex )
    {
        GameSettings gameSettings = GameContext.instance.m_settings;
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;
        GKPlayerData playerData = BattleContext.instance.GetPlayer(playerIndex);

        m_label.text = "P" + (playerIndex + 1).ToString();
        m_avatar.sprite = toasties.GetToasty(playerData.sToastyId).avatar;
        m_cursor.sprite = gameSettings.playerSettings[playerIndex].cursor;
        m_valid.SetActive(false);
    }

    internal void SetValid()
    {
        m_valid.SetActive(true);
    }

    internal void SetSensibility( int value )
    {
        m_sensibility.value = value;
    }
}
