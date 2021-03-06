using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerDataComparer : IComparer
{
    public int Compare(object x, object y)
    {
        GKPlayerData p1 = (GKPlayerData)x;
        GKPlayerData p2 = (GKPlayerData)y;
        return p2.m_totalScore - p1.m_totalScore;
    }
}

public class GKPlayerData : MonoBehaviour
{
    public delegate void onIntValueChangeDlg( int v);
    public delegate void onScoreChangeChangeDlg(int v, bool positive);

    public enum readyState { none = 0, initReady, rouletteReady, jokerReady, endJoker, jokerApplied, miniGameReady, miniGameEnded }

    public int Id { get; set; }
    public int m_currentScore;
    public int m_totalScore;
    public readyState m_nReadyState = readyState.none;
    public string sToastyId { get; set; }
    public bool isBot { get; set; }

    public onScoreChangeChangeDlg m_onScoreChangeDlg;
    public onIntValueChangeDlg m_onChangeLeaderDlg;

    public void Clean()
    {
        m_currentScore = 0;
        m_totalScore = 0;
    }

    public void AddCurrentScore(int nAdd)
    {
        int nLeader = BattleContext.instance.GetLeader();
        m_currentScore = Mathf.Max(0, m_currentScore + nAdd);
        int nNewLeader = BattleContext.instance.GetLeader();

        m_onScoreChangeDlg?.Invoke(m_currentScore, nAdd > 0);

        if (nLeader != nNewLeader)
        {
            m_onChangeLeaderDlg?.Invoke(nNewLeader);
        }
    }

    public void SetLocalCurrentScore(int nPoints)
    {
        int nLast = m_currentScore;
        int delta = nPoints - nLast;
        AddCurrentScore(delta);
    }
}
