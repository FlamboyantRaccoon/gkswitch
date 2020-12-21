using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_PlayerCursor : MonoBehaviour
{
    [SerializeField]
    private SpriteRenderer m_bkg;
    [SerializeField]
    private SpriteRenderer m_avatar;

    public void Setup( int playerId)
    {
        GameSettings gameSettings = GameContext.instance.m_settings;
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;
        GKPlayerData playerData = BattleContext.instance.GetPlayer(playerId);

        m_bkg.color = gameSettings.playerSettings[playerId].color;
        m_avatar.sprite = toasties.GetToasty(playerData.sToastyId).avatar;
    }
}
