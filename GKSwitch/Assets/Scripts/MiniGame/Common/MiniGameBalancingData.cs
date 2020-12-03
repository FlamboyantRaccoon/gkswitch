using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class MiniGameBalancingData
{

    public BattleContext.MiniGameDifficulty gameDifficulty;
    [lwMinMaxVector(0, 100, true)]
    public Vector2 m_playerLevelAccepted = new Vector2(0, 20);
#if UNITY_EDITOR
    public string m_sComment = "Bla bla pour le designer";
#endif
    public int gameTime = 30;

    public static byte GetDataId(MiniGameBalancingData[] datas, int nPlayerLevel )
    {
        int nTestId = BattleContext.instance.m_nGameDataTestId;
        if (nTestId != -1 && nTestId < datas.Length)
        {
            return (byte)nTestId;
        }

       
        List<byte> availableId = new List<byte>();

        int nMaxLevel = -1;

        if ( availableId.Count == 0 )
        {
            for (byte nDataId = 0; nDataId < datas.Length; nDataId++)
            {
                if (datas[nDataId].m_playerLevelAccepted.x <= nPlayerLevel+1 &&
                    datas[nDataId].m_playerLevelAccepted.y >= nPlayerLevel+1 &&
                    datas[nDataId].gameDifficulty != BattleContext.MiniGameDifficulty.fiesta )
                {
                    availableId.Add(nDataId);
                }
                else if(datas[nDataId].m_playerLevelAccepted.y > nMaxLevel )
                {
                    nMaxLevel = (int)datas[nDataId].m_playerLevelAccepted.y;
                }
            }
        }

        if (availableId.Count == 0 && nMaxLevel !=-1 )
        {
            for (byte nDataId = 0; nDataId < datas.Length; nDataId++)
            {
                if (datas[nDataId].m_playerLevelAccepted.y == nMaxLevel &&
                    datas[nDataId].gameDifficulty != BattleContext.MiniGameDifficulty.fiesta)
                {
                    availableId.Add(nDataId);
                }
            }
        }

        if (availableId.Count > 0)
        {
            return availableId[Random.Range(0, availableId.Count)];
        }
        return 0;
    }


}
