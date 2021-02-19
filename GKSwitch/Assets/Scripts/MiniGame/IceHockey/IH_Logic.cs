using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IH_Logic : MiniGameLogic
{
    private IH_IceHockey.IceHockeyData m_gameData;

    private uint m_nInitSeed;


    public void Init(IH_IceHockey.IceHockeyData gameData, byte nGameData )
    {
        m_fGameStartTime = Time.time;
        m_nGameDataId = nGameData;
        m_gameData = gameData;
    }
  
}
