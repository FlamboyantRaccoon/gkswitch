using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class TT_Logic : MiniGameLogic
{
    public class CandySpawnInfo
    {
        public float fSpawnTime;
        public float fX;
        public float fY;
        public TT_TrickOrTreat.CandyType candyType;

        public CandySpawnInfo(TT_TrickOrTreat.CandyType _candyType, float _fSpawnTime, float _fX, float _fY )
        {
            fSpawnTime = _fSpawnTime;
            fX = _fX;
            fY = _fY;
            candyType = _candyType;
        }
    }

    public class PumpkySpawnInfo
    {
        public float fX;
        public float fY;
        public float fDirX;
        public float fDirY;
        public float fSpeed;

        public PumpkySpawnInfo( float _fX, float _fY, float _fDirX, float _fDirY, float _fSpeed )
        {
            fX = _fX;
            fY = _fY;
            fDirX = _fDirX;
            fDirY = _fDirY;
            fSpeed = _fSpeed;
        }
    }

    public class FrankySpawnInfo
    {
        public float fX;
        public float fY;
        public float fSpeed;

        public FrankySpawnInfo(float _fX, float _fY, float _fSpeed)
        {
            fX = _fX;
            fY = _fY;
            fSpeed = _fSpeed;
        }
    }

    public class GhostSpawnInfo
    {
        public float fSpawnTime;
        public float fY;
        public bool bReverse;
        public float fSpeed;

        public GhostSpawnInfo( float _fSpawnTime, float _fY, bool _bReverse, float _fSpeed )
        {
            fSpawnTime = _fSpawnTime;
            fY = _fY;
            bReverse = _bReverse;
            fSpeed = _fSpeed;
        }
    }

    public class ItemBotInfo
    {
        public float fSpawnTime;
        public float fTakeTime;
        public TT_TrickOrTreat.CandyType candyType;

        public ItemBotInfo(float _fSpawnTime, float _fTakeTime, TT_TrickOrTreat.CandyType _candyType )
        {
            fSpawnTime = _fSpawnTime;
            fTakeTime = _fTakeTime;
            candyType = _candyType;
        }
    }

public List<PumpkySpawnInfo> pumpkyList { get { return m_pumpkyList; } }
    public List<FrankySpawnInfo> frankyList { get { return m_frankyList; } }

    public System.Func<int, TT_TrickOrTreat.TrickOrTreatData> recoverGameData { set { m_recoverGameData = value; } }
    public System.Action onInitialDataGet { set { m_onInitialDataGet = value; } }

    private System.Func<int, TT_TrickOrTreat.TrickOrTreatData> m_recoverGameData;
    private System.Action m_onInitialDataGet;
    private TT_TrickOrTreat.TrickOrTreatData m_gameData;
    private Queue<CandySpawnInfo> m_candySpawnQueue = new Queue<CandySpawnInfo>();
    private Queue<GhostSpawnInfo> m_ghostSpawnQueue = new Queue<GhostSpawnInfo>();
    private List<PumpkySpawnInfo> m_pumpkyList = new List<PumpkySpawnInfo>();
    private List<FrankySpawnInfo> m_frankyList = new List<FrankySpawnInfo>();

    private uint m_nInitSeed;

    public void Init(TT_TrickOrTreat.TrickOrTreatData gameData, byte nGameData )
    {
        m_nGameDataId = nGameData;
        m_gameData = gameData;
        m_fGameStartTime = Time.time;

        m_nInitSeed = (uint)(Random.Range(1, int.MaxValue));
        InitData(m_nInitSeed);
    }

    public CandySpawnInfo CheckIfCandyToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_candySpawnQueue.Count > 0)
        {
            CandySpawnInfo spawn = m_candySpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_candySpawnQueue.Dequeue();
                if (m_candySpawnQueue.Count == 0 )
                {
                    Debug.LogError("bad initial compute");
                }

                return spawn;
            }
        }

        return null;
    }

    public GhostSpawnInfo CheckIfGhostToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_ghostSpawnQueue.Count > 0)
        {
            GhostSpawnInfo spawn = m_ghostSpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_ghostSpawnQueue.Dequeue();
                if (m_ghostSpawnQueue.Count == 0 )
                {
                    Debug.LogError("bad initial compute");
                }

                return spawn;
            }
        }

        return null;
    }

    public List<ItemBotInfo> ComputeItemBotInfo( AnimationCurve percentChanceToTakeFlashCandy, Vector2 timeToTakeCandy, float goldCandyTime )
    {
        CandySpawnInfo[] spawnInfos = new CandySpawnInfo[m_candySpawnQueue.Count];
        m_candySpawnQueue.CopyTo(spawnInfos, 0);

        List<ItemBotInfo> botInfos = new List<ItemBotInfo>();
        List<ItemBotInfo> currentCandy = new List<ItemBotInfo>();
        for (int i = 0; i < spawnInfos.Length; i++)
        {
            // check how many candy
            float fSpawnTime = spawnInfos[i].fSpawnTime;
            for( int j=currentCandy.Count-1; j>=0; j-- )
            {
                if( fSpawnTime > currentCandy[j].fTakeTime )
                {
                    currentCandy.RemoveAt(j);
                }
            }

            if( currentCandy.Count < m_gameData.nCandyMax )
            {
                bool bTake = spawnInfos[i].candyType != TT_TrickOrTreat.CandyType.gold;
                if (!bTake)
                {
                    float fRatioTime = percentChanceToTakeFlashCandy.Evaluate((spawnInfos[i].fSpawnTime / m_gameData.gameTime));
                    float fPercent = Random.Range(0f, 1f);
                    bTake = fPercent < fRatioTime;
                }

                if (bTake)
                {
                    float fTakeTime = spawnInfos[i].fSpawnTime + Random.Range(timeToTakeCandy.x, timeToTakeCandy.y);
                    ItemBotInfo item = new ItemBotInfo(spawnInfos[i].fSpawnTime, fTakeTime, spawnInfos[i].candyType);
                    botInfos.Add(item);
                    currentCandy.Add(item);
                }
                else
                {
                    float fTakeTime = spawnInfos[i].fSpawnTime + goldCandyTime;
                    ItemBotInfo item = new ItemBotInfo(spawnInfos[i].fSpawnTime, fTakeTime, spawnInfos[i].candyType);
                    currentCandy.Add(item);
                }
            }
            
        }
        return botInfos;
    }


    private void InitData( uint nSeed )
    {
        RrRndHandler.RndSeed(nSeed);
        InitCandys();
        InitGhost();
        InitPumpkys();
        InitFrankys();
    }

    private void InitCandys()
    {
        float fGameTime = 0;
        float fEndTime = m_gameData.gameTime + 10f;

        int nSequenceId = 0;

        int nGridColumnCount = m_gameData.nGridColumnCount;
        int nGridRowCount = m_gameData.nGridRowCount;
        float fColumnSize = TT_TrickOrTreat.s_gameArea.width / (float)nGridColumnCount;
        float fRowSize = TT_TrickOrTreat.s_gameArea.height / (float)nGridRowCount;

        lwRndArray rndArray = new lwRndArray((uint)(nGridColumnCount * nGridRowCount));
        List<int> middleCells = new List<int>();
        int nMiddleCol0 = nGridColumnCount / 2, nMiddleCol1 = nGridColumnCount / 2;
        if( nGridColumnCount%2 == 0 )
        {
            nMiddleCol0--;
        }
        else
        {
            nMiddleCol0--;
            nMiddleCol1++;
        }

        int nMiddleRow0 = nGridRowCount / 2, nMiddleRow1 = nGridRowCount / 2;
        if (nGridRowCount % 2 == 0)
        {
            nMiddleRow0--;
        }
        else
        {
            nMiddleRow0--;
            nMiddleRow1++;
        }

        for ( int i=nMiddleCol0; i<=nMiddleCol1; i++ )
        {
            for (int j = nMiddleRow0; j <= nMiddleRow1; j++)
            {
                middleCells.Add(j * nGridColumnCount + i);
            }
        }

        while (fGameTime < fEndTime)
        {
            TT_TrickOrTreat.TrickOrTreatDataSequence sequence = m_gameData.sequencesArray[nSequenceId];
            float fSequenceEndTime = nSequenceId < m_gameData.sequencesArray.Length - 1 ? m_gameData.sequencesArray[nSequenceId + 1].startTime : fEndTime;

            float fSpawnTime = (1f / (float)(sequence.itemsBySecond));


            while (fGameTime < fSequenceEndTime)
            {
                TT_TrickOrTreat.CandyType candyType = ComputeCandyTypeFromSequence(sequence);
                int nCellId = -1;
                while( nCellId==-1 || ( middleCells.Contains(nCellId) && fGameTime < 5f ) )
                {
                    nCellId = (int)rndArray.ChooseValue(true);
                }
                
                float fX = RrRndHandler.RndRange(0f, fColumnSize) + (nCellId % nGridColumnCount) * fColumnSize + TT_TrickOrTreat.s_gameArea.x;
                float fY = RrRndHandler.RndRange(0f, fRowSize) + ((int)(nCellId / nGridColumnCount)) * fRowSize + TT_TrickOrTreat.s_gameArea.y;
                m_candySpawnQueue.Enqueue(new CandySpawnInfo(candyType, fGameTime, fX, fY ));
                
                fGameTime += fSpawnTime;
            }
            nSequenceId++;


        }
    }

    private void InitGhost()
    {
        float fGameTime = 0;
        float fEndTime = m_gameData.gameTime + 10f;

        int nSequenceId = 0;

        int nGridRowCount = m_gameData.nGridRowCount;
        float fRowSize = TT_TrickOrTreat.s_gameArea.height / (float)nGridRowCount;

        lwRndArray rndArray = new lwRndArray((uint)(nGridRowCount));


        while (fGameTime < fEndTime)
        {
            TT_TrickOrTreat.TrickOrTreatDataSequence sequence = m_gameData.sequencesArray[nSequenceId];
            float fSequenceEndTime = nSequenceId < m_gameData.sequencesArray.Length - 1 ? m_gameData.sequencesArray[nSequenceId + 1].startTime : fEndTime;

            if(sequence.ghostBySecond==0 )
            {
                fGameTime = fSequenceEndTime;
            }
            else
            {
                float fSpawnTime = (1f / (float)(sequence.ghostBySecond));
                while (fGameTime < fSequenceEndTime)
                {
                    int nCellId = (int)rndArray.ChooseValue(true);
                    float fY = RrRndHandler.RndRange(0f, fRowSize) + ((int)(nCellId)) * fRowSize + TT_TrickOrTreat.s_gameArea.y;
                    bool bReverse = RrRndHandler.RndRange(0, 2) == 1;
                    float fSpeed = RrRndHandler.RndRange(sequence.fGhostSpeed.x, sequence.fGhostSpeed.y);
                    m_ghostSpawnQueue.Enqueue(new GhostSpawnInfo(fGameTime, fY, bReverse, fSpeed));

                    fGameTime += fSpawnTime;
                }
            }
            nSequenceId++;
        }
    }

    private void InitPumpkys()
    {
        if( m_gameData.nPumpkyCount==0)
        {
            return;
        }

        int nGridColumnCount = m_gameData.nGridColumnCount;
        int nGridRowCount = m_gameData.nGridRowCount;
        float fColumnSize = TT_TrickOrTreat.s_gameArea.width / (float)nGridColumnCount;
        float fRowSize = TT_TrickOrTreat.s_gameArea.height / (float)nGridRowCount;

        lwRndArray rndArray = new lwRndArray((uint)(nGridColumnCount * 2 + nGridRowCount * 2 - 4));

        for( int i=0; i<m_gameData.nPumpkyCount; i++ )
        {
            int nX, nY;
            GetEnnemyCellId(ref rndArray, out nX, out nY);

            float fX = RrRndHandler.RndRange(0f, fColumnSize) + (nX) * fColumnSize + TT_TrickOrTreat.s_gameArea.x;
            float fY = RrRndHandler.RndRange(0f, fRowSize) + (nY) * fRowSize + TT_TrickOrTreat.s_gameArea.y;
            float fDirX = RrRndHandler.RndRange(-10f, 10f);
            float fDirY = RrRndHandler.RndRange(-10f, 10f);
            float fSpeed = RrRndHandler.RndRange( m_gameData.fPumpkySpeed.x, m_gameData.fPumpkySpeed.y );
            m_pumpkyList.Add(new PumpkySpawnInfo(fX, fY, fDirX, fDirY, fSpeed));
        }
    }

    private void InitFrankys()
    {
        if (m_gameData.nFrankyCount == 0)
        {
            return;
        }

        int nGridColumnCount = m_gameData.nGridColumnCount;
        int nGridRowCount = m_gameData.nGridRowCount;
        float fColumnSize = TT_TrickOrTreat.s_gameArea.width / (float)nGridColumnCount;
        float fRowSize = TT_TrickOrTreat.s_gameArea.height / (float)nGridRowCount;

        lwRndArray rndArray = new lwRndArray((uint)(nGridColumnCount * 2 + nGridRowCount * 2 - 4));

        for (int i = 0; i < m_gameData.nFrankyCount; i++)
        {
            int nX, nY;
            GetEnnemyCellId(ref rndArray, out nX, out nY);
            float fX = RrRndHandler.RndRange(0f, fColumnSize) + (nX) * fColumnSize + TT_TrickOrTreat.s_gameArea.x;
            float fY = RrRndHandler.RndRange(0f, fRowSize) + (nY) * fRowSize + TT_TrickOrTreat.s_gameArea.y;
            float fSpeed = RrRndHandler.RndRange(m_gameData.fFrankySpeed.x, m_gameData.fFrankySpeed.y);
            m_frankyList.Add(new FrankySpawnInfo(fX, fY, fSpeed ));
        }
    }

    private void GetEnnemyCellId( ref lwRndArray rndArray, out int nX, out int nY )
    {
        int nGridColumnCount = m_gameData.nGridColumnCount;
        int nGridRowCount = m_gameData.nGridRowCount;

        int nCellId = (int)rndArray.ChooseValue(true);
        nX = 0;
        nY = 0;
        if (nCellId < nGridColumnCount)
        {
            nX = nCellId;
        }
        else if (nCellId < nGridColumnCount + nGridRowCount - 1)
        {
            nCellId = nCellId - nGridColumnCount + 1;
            nX = nGridColumnCount - 1;
            nY = nCellId;
        }
        else if (nCellId < 2 * nGridColumnCount + nGridRowCount - 2)
        {
            nCellId = nCellId - nGridColumnCount - nGridRowCount + 2;
            nX = nCellId;
            nY = nGridRowCount - 1;
        }
        else
        {
            nCellId = nCellId - 2 * nGridColumnCount - nGridRowCount + 3;
            nX = 0;
            nY = nCellId;
        }
    }

    private TT_TrickOrTreat.CandyType ComputeCandyTypeFromSequence(TT_TrickOrTreat.TrickOrTreatDataSequence sequence)
    {
        float fTotalWeight = 0f;
        for (int i = 0; i < sequence.candyWeight.nLength; i++)
        {
            fTotalWeight += sequence.candyWeight[i];
        }
        bool bUseWeihght1 = fTotalWeight == 0f;
        float fRnd = bUseWeihght1 ? RrRndHandler.RndRange(0f, (float)sequence.candyWeight.nLength) : RrRndHandler.RndRange(0f, fTotalWeight);
        bool bFound = false;
        int nItemId = 0;
        while (!bFound && nItemId < sequence.candyWeight.nLength)
        {
            float fTest = (bUseWeihght1 ? 1f : sequence.candyWeight[nItemId]);
            if (fRnd < fTest)
            {
                bFound = true;
            }
            else
            {
                fRnd -= fTest;
                nItemId++;
            }
        }
        return (TT_TrickOrTreat.CandyType)nItemId;
    }
}
