using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameBasicHud : MonoBehaviour
{
    [SerializeField]
    private PlayerHud m_playerPrefab;
    [SerializeField]
    private MiniGamePlayerLayout[] m_playerLayout;

    private PlayerHud[] m_playerArray;
    private TMP_Text m_timer;

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
        MiniGamePlayerLayout playerLayout = SelectGoodLayout(bCtx.playerCount);
        m_timer = playerLayout.m_timer;

        int nCount = Mathf.Min(m_playerArray.Length, playerLayout.playersRoot.Length);
        for (int nPlayer = 0; nPlayer < nCount; nPlayer++)
        {
            GKPlayerData player = bCtx.GetPlayer(nPlayer);
            m_playerArray[nPlayer] = GameObject.Instantiate(m_playerPrefab, playerLayout.playersRoot[nPlayer]);
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

    private MiniGamePlayerLayout SelectGoodLayout( int playerCount )
    {
        int nLayoutId = Mathf.Min( m_playerLayout.Length-1, playerCount-1 );
        for( int i=0; i<m_playerLayout.Length; i++ )
        {
            m_playerLayout[i].gameObject.SetActive(i == nLayoutId);
            //m_playerLayout[i].gameObject.SetActive(false);
        }
        return m_playerLayout[nLayoutId];
    }
}
