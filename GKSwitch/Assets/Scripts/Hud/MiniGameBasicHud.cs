using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameBasicHud : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_timer;
    [SerializeField]
    private Transform[] m_playerPos = new Transform[2];
    [SerializeField]
    private PlayerHud m_playerPrefab;

    private PlayerHud[] m_playerArray;

    public void UpdateTime(int nTime)
    {
        Debug.Assert(m_timer != null);
        m_timer.text = nTime == -1 ? "" : nTime.ToString();
    }

    protected virtual void Awake()
    {
        InitPlayer();
    }

    void InitPlayer()
    {
        Reset();
        BattleContext bCtx = BattleContext.instance;
        m_playerArray = new PlayerHud[bCtx.playerCount];
        int nCount = Mathf.Min(m_playerArray.Length, m_playerPos.Length);
        for (int nPlayer = 0; nPlayer < nCount; nPlayer++)
        {
            GKPlayerData player = bCtx.GetPlayer(nPlayer);
            m_playerArray[nPlayer] = GameObject.Instantiate(m_playerPrefab, m_playerPos[nPlayer]);
            m_playerArray[nPlayer].SetInfos(nPlayer);

            bCtx.GetPlayer(nPlayer).m_onScoreChangeDlg += m_playerArray[nPlayer].SetScore;
            player.m_onChangeLeaderDlg = OnChangeLeader;
        }
    }

    public virtual void Exit()
    {
        BattleContext bCtx = BattleContext.instance;
        for (int nPlayer = 0; nPlayer < bCtx.playerCount; nPlayer++)
        {
            bCtx.GetPlayer(nPlayer).m_onScoreChangeDlg -= m_playerArray[nPlayer].SetScore;
        }
        Reset();
    }

    private void Reset()
    {
        if (m_playerArray != null)
        {
            for (int nPlayer = 0; nPlayer < m_playerArray.Length; nPlayer++)
            {
                GameObject.Destroy(m_playerArray[nPlayer]);
            }
            m_playerArray = null;
        }

    }


    private void OnChangeLeader(int nLeaderId)
    {
        if (m_playerArray != null)
        {
            for (int nPlayer = 0; nPlayer < m_playerArray.Length; nPlayer++)
            {
                if (m_playerArray[nPlayer] != null)
                {
                    m_playerArray[nPlayer].SetWinning(nLeaderId == nPlayer);
                }
            }
        }
    }

}
