using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IH_PlayerInfos
{
    private int m_playerId;
    private Rect m_spaceRect;
    public IH_Player m_player { private set; get; }


    public void Setup(int playerId)
    {
        m_playerId = playerId;
    }

    internal void Clean()
    {
        GameObject.Destroy(m_player.gameObject);
    }

    internal void SetPlayer(IH_Player player, Rect r)
    {
        m_player = player;
        m_spaceRect = r;

        Vector3 vPos = m_player.transform.position;
        vPos.x = m_spaceRect.x + m_spaceRect.width/2f;
        vPos.y = m_spaceRect.y + m_spaceRect.height / 2f;
        m_player.SetInitialPos( vPos );
    }

    internal void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {

        Vector3 vPos = m_player.transform.position;
        vPos.x = m_spaceRect.x + m_spaceRect.width * Mathf.Clamp01(v.x);
        vPos.y = m_spaceRect.y + m_spaceRect.height * Mathf.Clamp01(v.y);
        m_player.Move(vPos);

        /*if (m_toasty != null)
        {
            m_toasty.UpdatePosition(v);
        }*/
    }
}
