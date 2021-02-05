using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PK_Logic : MiniGameLogic
{
    public enum BotAction { bad, good, perfect };

    public float fWarmUpSpeedMultiplier { get { return m_fWarmUpSpeedMultiplier; } }
    public PK_Picking.PickingData gameData { set { m_gameData = value; } }
    public bool bIsInitialized { get { return m_gameData != null; } }

    private PK_Picking.PickingData m_gameData;
    private PK_Picking.itemTypeFloat m_itemSpeedRatio;
    private float m_fWarmUpSpeedMultiplier = 1f;

    public class ItemSpawnInfo
    {
        public float fSpawnTime;
        public int nLineId;
        public int nLineId2;
        public PK_Picking.itemType itemType;

        public ItemSpawnInfo(float _fSpawnTime, int _nLine, int _nLine2, PK_Picking.itemType _itemType)
        {
            fSpawnTime = _fSpawnTime;
            nLineId = _nLine;
            nLineId2 = _nLine2;
            itemType = _itemType;
        }

        public ItemSpawnInfo(ushort _fSpawnTime, byte xDataMask)
        {
            fSpawnTime = ((float)(_fSpawnTime)) / 1000;
            nLineId = (xDataMask >> 3) & (7);
            nLineId2 = xDataMask & (7);
            itemType = (PK_Picking.itemType)(xDataMask >> 6);
        }

        public byte CompactInfos()
        {
            //int n = ((((byte)itemType) & 3) << 6 | (nLineId << 3) | nLineId2);
            return (byte)((((byte)itemType) & 3) << 6 | (nLineId << 3) | nLineId2);
        }
    }

    public class ItemBotInfo
    {
        public float fIATime;
        public BotAction botAction;

        public ItemBotInfo(float _fIATime, BotAction _botAction)
        {
            fIATime = _fIATime;
            botAction = _botAction;
        }
    }

    private uint m_nInitSeed;
    private Queue<ItemSpawnInfo> m_itemSpawnQueue = new Queue<ItemSpawnInfo>();

    public void Init(PK_Picking.PickingData gameData, byte nGameData, PK_Picking.itemTypeFloat itemSpeedRatio)
    {
        m_fGameStartTime = Time.time;
        m_nGameDataId = nGameData;
        m_gameData = gameData;
        m_itemSpeedRatio = itemSpeedRatio;
        InitGameItemFallData();
    }


    public void Init(PK_Picking.itemTypeFloat itemSpeedRatio)
    {
        m_itemSpeedRatio = itemSpeedRatio;
    }

    public void InitGameItemFallData()
    {
        m_nInitSeed = (uint)Random.Range(1, int.MaxValue);
        InitGameItemFallDataWithSeed(m_nInitSeed);
    }

    public void InitGameItemFallDataWithSeed(uint nSeed)
    {
        float fGameTime = 0;
        float fEndTime = m_gameData.gameTime + 10f;

        RrRndHandler.RndSeed(nSeed);

        int nLaneCount = ComputeLaneCount();
        float[] fLaneBusyEndDate = new float[nLaneCount];
        float fTimeForFirstItem = float.MaxValue;

        int nSequenceId = 0;
        while (fGameTime < fEndTime)
        {
            PK_Picking.PickingDataDataSequence sequence = m_gameData.sequencesArray[nSequenceId];
            float fSequenceEndTime = nSequenceId < m_gameData.sequencesArray.Length - 1 ? m_gameData.sequencesArray[nSequenceId + 1].startTime : fEndTime;
            float fSpawnTime = (1f / (float)(sequence.itemsBySecond));


            while (fGameTime < fSequenceEndTime)
            {
                PK_Picking.itemType itemType = ComputeItemTypeFromSequence(sequence);
                float fItemEndDate = ComputeEndItemDate(fGameTime, itemType);
                float fItemSecondEndDate = itemType == PK_Picking.itemType.doubleTouch ? ComputeSecondEndItemDate(fGameTime) : 0f;
                float fItemSizeTime = ComputeItemSizeTime(itemType);
                int nLaneId = ComputeItemLaneId(nLaneCount, sequence.nLanes, fItemEndDate, fItemSizeTime, ref fLaneBusyEndDate);

                int nLane2 = 0;
                if (fItemSecondEndDate != 0f)
                {
                    nLane2 = ComputeItemLaneId(nLaneCount, sequence.nLanes, fItemSecondEndDate, fItemSizeTime, ref fLaneBusyEndDate);
                }
                else if (itemType == PK_Picking.itemType.doubleTouch) // if we can't place double touch, remove it
                {
                    nLaneId = -1;
                }

                if (nLaneId >= 0)
                {
                    if (fLaneBusyEndDate[nLaneId] < fTimeForFirstItem)
                    {
                        fTimeForFirstItem = fLaneBusyEndDate[nLaneId];
                    }

                    m_itemSpawnQueue.Enqueue(new ItemSpawnInfo(fGameTime, nLaneId, nLane2, itemType));
                }
                fGameTime += fSpawnTime;
            }
            nSequenceId++;
        }

        m_fWarmUpSpeedMultiplier = fTimeForFirstItem / 4f;
    }

    public ItemSpawnInfo CheckIfItemToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_itemSpawnQueue.Count > 0)
        {
            //bool bGoodMeal = m


            ItemSpawnInfo spawn = m_itemSpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_itemSpawnQueue.Dequeue();
                if (m_itemSpawnQueue.Count == 0 )
                {
                    Debug.LogError("bad initial compute");
                }

                return spawn;
            }
        }


        return null;
    }

    
    private float ComputeEndItemDate(float fStartTime, PK_Picking.itemType itemType)
    {
        float fRemainDistance = PK_Picking.fITEM_START_Y - PK_Picking.fITEM_END_Y;
        float fSpeed = m_itemSpeedRatio[(int)itemType] * m_gameData.referenceSpeed;
        float fFallTime = fRemainDistance / fSpeed;
        return fStartTime + fFallTime;
    }

    private float ComputePickingZoneItemDate(float fStartTime, PK_Picking.itemType itemType)
    {
        float fRemainDistance = PK_Picking.fITEM_START_Y - PK_Picking.fPICKZONEY;
        float fSpeed = m_itemSpeedRatio[(int)itemType] * m_gameData.referenceSpeed;
        float fFallTime = fRemainDistance / fSpeed;
        return fStartTime + fFallTime;
    }

    private float ComputeItemDateWithWarmupMultiplicator(float fStartTime, float fDist, PK_Picking.itemType itemType)
    {
        float fRemainDistance = fDist; // PK_Picking.fITEM_START_Y - PK_Picking.fPICKZONEY;
        float fWarmupTime = 3f - fStartTime;
        float fSpeed = m_itemSpeedRatio[(int)itemType] * m_gameData.referenceSpeed;
        if (fWarmupTime > 0)
        {
            fRemainDistance -= fWarmupTime * m_fWarmUpSpeedMultiplier * fSpeed;
            fStartTime = 3f;
        }

        float fFallTime = fRemainDistance / fSpeed;
        return fStartTime + fFallTime;
    }

    private float ComputeSecondEndItemDate(float fStartTime)
    {
        float fRemainDistance = PK_Picking.fITEM_START_Y - PK_Picking.fITEM_END_Y + PK_Picking.fSECONDTOUCH_JUMP;
        float fSpeed = m_itemSpeedRatio[(int)PK_Picking.itemType.doubleTouch] * m_gameData.referenceSpeed;
        float fFallTime = fRemainDistance / fSpeed;

        float fFlyingTime = PK_Picking.fSECONDTOUCH_JUMP / (m_itemSpeedRatio[(int)PK_Picking.itemType.doubleTouchOther] * m_gameData.referenceSpeed);
        return fStartTime + fFallTime + fFlyingTime;
    }

    private float ComputeItemSizeTime(PK_Picking.itemType itemType)
    {
        float fItemSize = PK_Picking.fITEM_SIZE_X;
        float fSpeed = m_itemSpeedRatio[(int)itemType] * m_gameData.referenceSpeed;
        return fItemSize / fSpeed;
    }

    private PK_Picking.itemType ComputeItemTypeFromSequence(PK_Picking.PickingDataDataSequence sequence)
    {
        float fTotalWeight = 0f;
        for (int i = 0; i < sequence.itemsWeight.nLength; i++)
        {
            fTotalWeight += sequence.itemsWeight[i];
        }
        bool bUseWeihght1 = fTotalWeight == 0f;
        float fRnd = bUseWeihght1 ? RrRndHandler.RndRange(0f, (float)sequence.itemsWeight.nLength) : RrRndHandler.RndRange(0f, fTotalWeight);
        bool bFound = false;
        int nItemId = 0;
        while (!bFound && nItemId < sequence.itemsWeight.nLength)
        {
            float fTest = (bUseWeihght1 ? 1f : sequence.itemsWeight[nItemId]);
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
        return (PK_Picking.itemType)nItemId;
    }

    public int ComputeLaneCount()
    {
        /*int[] nLaneCountArray = new int[m_gameData.sequencesArray.Length];
        for( int i=0; i<m_gameData.sequencesArray.Length; i++ )
        {
            nLaneCountArray[i] = m_gameData.sequencesArray[i].nLanes;
        }
        if( nLaneCountArray.Length==1 )
        {
            return nLaneCountArray[0];
        }
        else
        {
            return lwTools.ComputePPCM(nLaneCountArray);
        }*/
        // search just the max
        int nMax = 0;
        for (int i = 0; i < m_gameData.sequencesArray.Length; i++)
        {
            if (m_gameData.sequencesArray[i].nLanes > nMax)
            {
                nMax = m_gameData.sequencesArray[i].nLanes;
            }
        }
        return nMax;
    }

    public List<ItemBotInfo> ComputeItemBotInfo(AnimationCurve goodItemChance, AnimationCurve badItemChance, AnimationCurve perfectChance)
    {
        ItemSpawnInfo[] spawnInfos = new ItemSpawnInfo[m_itemSpawnQueue.Count];
        m_itemSpawnQueue.CopyTo(spawnInfos, 0);

        List<ItemBotInfo> botInfos = new List<ItemBotInfo>();
        for (int i = 0; i < spawnInfos.Length; i++)
        {
            float fEndTime = ComputeItemDateWithWarmupMultiplicator(spawnInfos[i].fSpawnTime, PK_Picking.fITEM_START_Y - PK_Picking.fITEM_END_Y, spawnInfos[i].itemType);
            float fPickingTime = ComputeItemDateWithWarmupMultiplicator(spawnInfos[i].fSpawnTime, PK_Picking.fITEM_START_Y - PK_Picking.fPICKZONEY, spawnInfos[i].itemType);

            float fMoveRatio = fPickingTime / m_gameData.gameTime;
            float fRnd = (Random.Range(0, 100)) / 100f;
            if (spawnInfos[i].itemType == PK_Picking.itemType.bad)
            {
                float fPercent = badItemChance.Evaluate(fMoveRatio);
                if (fRnd > fPercent)
                {
                    botInfos.Add(new ItemBotInfo(fPickingTime, BotAction.bad));
                }
                else
                {
                    botInfos.Add(new ItemBotInfo(fEndTime, BotAction.bad));
                }
            }
            else
            {
                float fPercent = goodItemChance.Evaluate(fMoveRatio);
                if (fRnd <= fPercent) // Good
                {
                    float fRndPerfect = (Random.Range(0, 100)) / 100f;
                    float fPerfectPercent = perfectChance.Evaluate(fMoveRatio);
                    BotAction result = fRndPerfect <= fPerfectPercent ? BotAction.perfect : BotAction.good;
                    botInfos.Add(new ItemBotInfo(fPickingTime, result));

                    if (spawnInfos[i].itemType == PK_Picking.itemType.doubleTouch)
                    {
                        fPickingTime = ComputeSecondEndItemDate(fPickingTime);
                        fMoveRatio = fPickingTime / m_gameData.gameTime;
                        fRnd = (Random.Range(0, 100)) / 100f;
                        fPercent = goodItemChance.Evaluate(fMoveRatio);

                        if (fRnd <= fPercent) // Good
                        {
                            fRndPerfect = (Random.Range(0, 100)) / 100f;
                            fPerfectPercent = perfectChance.Evaluate(fMoveRatio);
                            result = fRndPerfect <= fPerfectPercent ? BotAction.perfect : BotAction.good;
                            botInfos.Add(new ItemBotInfo(fPickingTime, result));
                        }
                    }
                }
            }
        }
        botInfos.Sort(ItemBotInfoSort);
        return botInfos;
    }

    int ItemBotInfoSort(ItemBotInfo a, ItemBotInfo b)
    {
        if (a.fIATime < b.fIATime)
        {
            return -1;
        }
        else if (a.fIATime > b.fIATime)
        {
            return 1;
        }
        return 0;
    }

    private PK_Picking.PickingDataDataSequence GetSequenceForTime(float fCurrentTime)
    {
        int nSequenceId = 0;
        while (nSequenceId < m_gameData.sequencesArray.Length - 1 &&
           fCurrentTime >= m_gameData.sequencesArray[nSequenceId + 1].startTime)
        {
            nSequenceId++;
        }
        Debug.Assert(nSequenceId >= 0 && nSequenceId < m_gameData.sequencesArray.Length);
        return m_gameData.sequencesArray[nSequenceId];
    }

    private int ComputeItemLaneId(int nTotalLane, int nSequenceLane, float fItemEndDate, float fItemSizeTime, ref float[] fLaneBusyEndDate)
    {
        List<int> nAvailableId = new List<int>();

        for (int i = 0; i < nSequenceLane; i++)
        {
            if (fLaneBusyEndDate[i] + fItemSizeTime < fItemEndDate)
            {
                nAvailableId.Add(i);
            }
        }

        if (nAvailableId.Count > 0)
        {
            int nLaneId = nAvailableId[RrRndHandler.RndRange(0, nAvailableId.Count)];
            fLaneBusyEndDate[nLaneId] = fItemEndDate;
            return nLaneId;
        }
        else
        {
            return -1;
        }

    }

   
}
