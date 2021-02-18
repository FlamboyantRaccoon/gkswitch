using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TMP_Logic : MiniGameLogic
{
    private TMP_Template.TmpData m_gameData;

    private uint m_nInitSeed;


    public void Init(TMP_Template.TmpData gameData, byte nGameData )
    {
        m_fGameStartTime = Time.time;
        m_nGameDataId = nGameData;
        m_gameData = gameData;
    }
  
}
