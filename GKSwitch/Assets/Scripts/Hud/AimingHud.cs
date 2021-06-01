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

    private Rect[] m_specificCursorZone = null;

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
            players[0].cursorAiming.addOnPosChangeDlg = CursorMove;
//            players[i].m_updateCursorDlg += UpdateCursor;
        }

  /*      PlayerAim aimFollow = GameObject.Instantiate<PlayerAim>(playerAimPrefab, transform);
        aimFollow.Setup(3);
        m_aimDico.Add(3, aimFollow);
        players[0].cursorAiming.addOnPosChangeDlg = CursorMove;*/
        
        m_rt = GetComponent<RectTransform>();
        m_bInitialized = true;
    }

    private void CursorMove(Vector2 v)
    {
        UpdateCursor(0, v);
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

    public void InitSpecificZone( int zoneCount )
    {
        m_specificCursorZone = new Rect[zoneCount];
    }

    public void SetSpecificZone( int id, Rect r )
    {
        Debug.Assert(m_specificCursorZone != null && id < m_specificCursorZone.Length, "ERROR ! Aiming Specific zone was not initialized");
        m_specificCursorZone[id] = r;
    }


    public void ResetSpecificZone()
    {
        m_specificCursorZone = null;
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

        if( m_specificCursorZone!=null )
        {
            ComputeSpecificZone(id, vPos, ref fX, ref fY);
        }
        else if( HudManager.sSPLITHUD_COUNT > 1 )
        {
            ComputeSplitPosition(id, ref fX, ref fY);
        }


        PlayerAim playerAim = null;
        if( m_aimDico.TryGetValue( id, out playerAim) )
        {
            playerAim.SetPosition(new Vector2(fX, fY));
        }
    }

    private void ComputeSpecificZone(int id, Vector2 vPos, ref float fX, ref float fY)
    {
        Debug.Assert(m_specificCursorZone != null && id < m_specificCursorZone.Length && m_specificCursorZone[id] != null, "Specific cursor zone was baddly initialized");

        vPos.x = Mathf.Clamp01((vPos.x / 2f) + 0.5f);
        vPos.y = Mathf.Clamp01((vPos.y / 2f) + 0.5f);

        fX = (m_specificCursorZone[id].x) + (m_specificCursorZone[id].width * (vPos.x));
        fY = (m_specificCursorZone[id].y) + (m_specificCursorZone[id].height * (vPos.y));


        //Debug.Log("ComputeSpecificZone " + vPos + " // " + m_specificCursorZone[id].x + " // " + m_specificCursorZone[id].width + " // " + fX);

        Vector2 v = HudManager.instance.ComputeHudPosFromWorldPosition(new Vector3(fX, fY, 0f));
        fX = v.x;
        fY = v.y;
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
