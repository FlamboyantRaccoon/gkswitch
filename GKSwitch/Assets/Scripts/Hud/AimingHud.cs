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
            m_aimDico.Add(players[i].Id, aim);
            players[i].m_updateCursorDlg += UpdateCursor;
        }
        m_rt = GetComponent<RectTransform>();
        m_bInitialized = true;
    }

    public void UpdateCursor( int id, Vector2 vPos )
    {
        //Debug.Log("UpdateCursor " + id + " : " + vPos);
        float fHalfWidth = m_rt.rect.width / 2f;
        float fHalfHeight = m_rt.rect.height / 2f;
        float fX = vPos.x * fHalfWidth;
        float fY = vPos.y * fHalfHeight;

        PlayerAim playerAim = null;
        if( m_aimDico.TryGetValue( id, out playerAim) )
        {
            playerAim.SetPosition(new Vector2(fX, fY));
        }
    }
}
