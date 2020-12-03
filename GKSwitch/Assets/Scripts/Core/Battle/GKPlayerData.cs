using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GKPlayerData : MonoBehaviour
{
    public delegate void onIntValueChangeDlg( int v);
    public enum readyState { none = 0, initReady, rouletteReady, jokerReady, endJoker, jokerApplied, miniGameReady, miniGameEnded }

    public int m_currentScore;
    public int m_totalScore;
    public readyState m_nReadyState = readyState.none;
    public bool isBot { get; set; }

    public onIntValueChangeDlg m_onScoreChange;
}
