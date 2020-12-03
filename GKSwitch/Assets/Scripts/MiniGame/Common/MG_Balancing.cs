using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MG_Balancing<TBot, TData> : ScriptableObject
    where TBot : MiniGameBotData
    where TData : MiniGameBalancingData
{
    [SerializeField]
    public TBot[] m_botDatas;
    [SerializeField]
    public TData[] m_datas;
}