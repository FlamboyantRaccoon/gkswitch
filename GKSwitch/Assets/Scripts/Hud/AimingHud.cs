using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AimingHud : MonoBehaviour
{
    [SerializeField]
    private PlayerAim playerAimPrefab;

    private Dictionary<int, PlayerAim> m_aimDico;

    private RectTransform m_rt;
    private bool m_bInitialized = false;

    private bool m_bVisible = true;

    public void Setup()
    {
        if( m_bInitialized )
        {
            return;
        }

        m_aimDico = new Dictionary<int, PlayerAim>();
        List<RRPlayerInput> players = RRInputManager.instance.playerList;
        for( int i=0; i<players.Count; i++ )
        {
            PlayerAim aim = GameObject.Instantiate<PlayerAim>(playerAimPrefab, transform);
            aim.Setup(i);
            m_aimDico.Add(players[i].Id, aim);
            players[i].m_updateCursorDlg += UpdateCursor;
        }
        m_rt = GetComponent<RectTransform>();
        m_bInitialized = true;
    }

    public void Show()
    {
        foreach(KeyValuePair<int, PlayerAim> pair in m_aimDico )
        {
            pair.Value.Show();
        }
    }

    public void Hide()
    {
        foreach (KeyValuePair<int, PlayerAim> pair in m_aimDico)
        {
            pair.Value.Hide();
        }
    }

    public void UpdateCursor( int id, Vector2 vPos )
    {
        if( !m_bVisible )
        {
            return;
        }

        //Debug.Log("UpdateCursor " + id + " : " + vPos);
        float fHalfWidth = m_rt.rect.width / 2f;
        float fHalfHeight = m_rt.rect.height / 2f;
        float fX = vPos.x * fHalfWidth;
        float fY = vPos.y * fHalfHeight;

        if( HudManager.sSPLITHUD_COUNT > 1 )
        {
            ComputeSplitPosition(id, ref fX, ref fY);
        }


        PlayerAim playerAim = null;
        if( m_aimDico.TryGetValue( id, out playerAim) )
        {
            playerAim.SetPosition(new Vector2(fX, fY));
        }
    }

    private void ComputeSplitPosition(int id, ref float fX, ref float fY)
    {
        switch( HudManager.sSPLITHUD_TYPE )
        {
            case HudManager.SplitHudType.quarter:
                {
                    float fHalfHeight = m_rt.rect.height / 2f;
                    float fHalfWidth = m_rt.rect.width / 2f;

                    float fStartX = (id % 2) == 0 ? -fHalfWidth / 2f : fHalfWidth / 2f;
                    float fWidth = 0.5f;
                    float fStartY = 0;
                    float fHeight = 1f;

                    if (HudManager.sSPLITHUD_COUNT > 2)
                    {
                        fHeight = 0.5f;
                        fStartY = (id < 2) ? fHalfHeight / 2f : -fHalfHeight / 2f;
                    }

                    fX = fX * fWidth + fStartX;
                    fY = fY * fHeight + fStartY;
                }
                break;
            case HudManager.SplitHudType.vertical:
                {
                    float fHalfWidth = m_rt.rect.width / 2f;
                    float fhallWidth = m_rt.rect.width / HudManager.sSPLITHUD_COUNT;
                    float fStartX = -fHalfWidth + (id - 0.5f) * fhallWidth;
                    fX = fX / HudManager.sSPLITHUD_COUNT + fhallWidth + fStartX;
                }
                break;
        }
    }
}
