using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EX_Logic : MiniGameLogic
{
    public class ItemSpawnInfo
    {
        public float fSpawnTime;
        public int nComeInCurve;
        public int nSpotId;
        public ushort nExpressionMask;
        public float fAppearTime;
        public float fIdleTime;
        public float fDisAppearTime;


        public ItemSpawnInfo(float _fSpawnTime, int _nComeInCurve, int _nSpotId, ushort _nExpressionMask, float _fAppearTime, float _fIdleTime, float _fDisAppearTime)
        {
            fSpawnTime = _fSpawnTime;
            nComeInCurve = _nComeInCurve;
            nSpotId = _nSpotId;
            nExpressionMask = _nExpressionMask;
            fIdleTime = _fIdleTime;
            fDisAppearTime = _fDisAppearTime;
            fAppearTime = _fAppearTime;
        }

        public ItemSpawnInfo(ushort _fSpawnTime, byte _nDataMask, ushort _nExpressionMask, ushort _fAppearTime, ushort _fIdleTime, ushort _fDisAppearTime)
        {
            fSpawnTime = ((float)(_fSpawnTime)) / 1000;
            nComeInCurve = _nDataMask >> 4;
            nSpotId = (_nDataMask & 15);
            nExpressionMask = _nExpressionMask;
            fIdleTime = ((float)(_fIdleTime)) / 1000;
            fDisAppearTime = ((float)(_fDisAppearTime)) / 1000;
            fAppearTime = ((float)(_fAppearTime)) / 1000;
        }

        public byte GetDataMask()
        {
            return (byte)(nComeInCurve << 4 | nSpotId);
        }
    }

    public class ItemBotInfo
    {
        public float fSpawnTime;
        public bool bGood;
        public float fEndTime;
        public float fIATime;

        public ItemBotInfo(float _fSpawnTime, bool _bGood, float _fEndTime, float _fIATime)
        {
            fSpawnTime = _fSpawnTime;
            bGood = _bGood;
            fEndTime = _fEndTime;
            fIATime = _fIATime;
        }
    }

    public ushort[] goodExpressionArray { get { return m_nGoodExpressionArray; } }
    public EX_Expression m_expressionRoot;
    private EX_Expression.ExpressionData m_gameData;

    private uint m_nInitSeed;

    private Queue<ItemSpawnInfo> m_itemSpawnQueue = new Queue<ItemSpawnInfo>();
    private ushort[] m_nGoodExpressionArray;
    private float[] m_fGoodPresentTime;
    private bool m_bAllowMultipleSameGoodExpression = true;

    public void Init(EX_Expression.ExpressionData gameData, byte nGameData, EX_Expression expressionsRoot)
    {
        m_fGameStartTime = Time.time;
        m_nGameDataId = nGameData;
        m_gameData = gameData;

        m_nInitSeed = (uint)(Random.Range(1, int.MaxValue));
        InitData(m_nInitSeed, expressionsRoot);
    }

    public void InitData(uint nSeed, EX_Expression expressionsRoot)
    {
        RrRndHandler.RndSeed(nSeed);

        InitPlayerChoice();
        InitGameItemAppearData(expressionsRoot);
    }

    public ItemSpawnInfo CheckIfItemToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_itemSpawnQueue.Count > 0)
        {
            ItemSpawnInfo spawn = m_itemSpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_itemSpawnQueue.Dequeue();
                if (m_itemSpawnQueue.Count == 0)
                {
                    Debug.LogError("bad initial compute");
                }
                return spawn;
            }
        }


        return null;
    }

    public List<ItemBotInfo> ComputeItemBotInfo(AnimationCurve characterAnimationRatioReactionTime, float fReactRandomGap)
    {
        ItemSpawnInfo[] spawnInfos = new ItemSpawnInfo[m_itemSpawnQueue.Count];
        m_itemSpawnQueue.CopyTo(spawnInfos, 0);

        List<ItemBotInfo> botInfos = new List<ItemBotInfo>();
        for (int i = 0; i < spawnInfos.Length; i++)
        {
            float fEndTime = spawnInfos[i].fSpawnTime + spawnInfos[i].fAppearTime + spawnInfos[i].fIdleTime + spawnInfos[i].fDisAppearTime;
            float fRatioTime = characterAnimationRatioReactionTime.Evaluate((spawnInfos[i].fSpawnTime / m_gameData.gameTime));
            fRatioTime += -fReactRandomGap + Random.Range(0f, 2f * fReactRandomGap);
            fRatioTime = Mathf.Clamp01(fRatioTime);
            float fIATime = spawnInfos[i].fSpawnTime + fRatioTime * (spawnInfos[i].fAppearTime + spawnInfos[i].fIdleTime + spawnInfos[i].fDisAppearTime);

            botInfos.Add(new ItemBotInfo(spawnInfos[i].fSpawnTime, IsGood(spawnInfos[i].nExpressionMask, m_nGoodExpressionArray.Length), fEndTime, fIATime));
        }
        return botInfos;
    }

    private void InitGameItemAppearData(EX_Expression expressionsRoot)
    {
        float fGameTime = 0;
        float fEndTime = m_gameData.gameTime + 10f;

        int nHideCount = expressionsRoot.m_bkg.m_hideSpotArray.Length;
        float[] fHideBusyEndDate = new float[nHideCount];

        int nSequenceId = 0;
        bool bRowIsGood = false;
        int nInRow = 0;

        while (fGameTime < fEndTime)
        {
            EX_Expression.ExpressionsDataSequence sequence = m_gameData.sequencesArray[nSequenceId];
            float fSequenceEndTime = nSequenceId < m_gameData.sequencesArray.Length - 1 ? m_gameData.sequencesArray[nSequenceId + 1].startTime : fEndTime;


            while (fGameTime < fSequenceEndTime)
            {
                float fSpawnTime = RrRndHandler.RndRange(sequence.timebetweenSpawn.x, sequence.timebetweenSpawn.y);
                float fIdleTime = RrRndHandler.RndRange(sequence.idleTime.x, sequence.idleTime.y);
                float fAppearTime = RrRndHandler.RndRange(sequence.apparitionTime.x, sequence.apparitionTime.y);
                float fDisappearTime = RrRndHandler.RndRange(sequence.disappearTime.x, sequence.disappearTime.y);
                float fItemEndDate = fGameTime + fAppearTime + fIdleTime + fDisappearTime;
                int nComeInId = RrRndHandler.RndRange(0, expressionsRoot.m_apparitionAnimationDataArray.Length);
                bool bGood = ComputeIfGoodChoice(sequence, ref bRowIsGood, ref nInRow);
                ushort nExpressionMask = 0;
                int nSelectedGoodId = 0;
                bool bValid = GenerateExpressionMask(fGameTime, bGood, out nExpressionMask, out nSelectedGoodId);

                if (bValid)
                {
                    int nSpotId = ComputeItemSpotId(nHideCount, fGameTime, fItemEndDate, ref fHideBusyEndDate);
                    if (nSpotId >= 0)
                    {
                        // in good case update  the godd present time
                        if (nSelectedGoodId != -1)
                        {
                            m_fGoodPresentTime[nSelectedGoodId] = fItemEndDate;
                        }
                        m_itemSpawnQueue.Enqueue(new ItemSpawnInfo(fGameTime, nComeInId, nSpotId, nExpressionMask, fAppearTime, fIdleTime, fDisappearTime));
                    }
                }
                fGameTime += fSpawnTime;
            }
            nSequenceId++;
        }
    }

    private int ComputeItemSpotId(int nHideCount, float fGameTime, float fItemEndDate, ref float[] fHideBusyEndDate)
    {
        List<int> nAvailableId = new List<int>();

        for (int i = 0; i < nHideCount; i++)
        {
            if (fHideBusyEndDate[i] < fGameTime)
            {
                nAvailableId.Add(i);
            }
        }

        if (nAvailableId.Count > 0)
        {
            int nSpotId = nAvailableId[RrRndHandler.RndRange(0, nAvailableId.Count)];
            fHideBusyEndDate[nSpotId] = fItemEndDate;
            return nSpotId;
        }
        else
        {
            return -1;
        }
    }

    private void InitPlayerChoice()
    {
        int nCount = m_gameData.nPlayerChoiceCount;
        m_nGoodExpressionArray = new ushort[nCount];
        m_fGoodPresentTime = new float[nCount];
        int nSelectedExpressionId = 0;
        for (int nId = 0; nId < nCount; nId++)
        {
            GenerateExpressionMask(0f, false, out m_nGoodExpressionArray[nId], out nSelectedExpressionId, nId);
        }
    }

    private bool GenerateExpressionMask(float fGameTime, bool bGood, out ushort nExpressionMask, out int nSelectedExpressionId, int nControlLength = -1)
    {
        nSelectedExpressionId = -1;
        nExpressionMask = 0;
        if (bGood)
        {
            List<int> availableExpression = new List<int>();
            for (int i = 0; i < m_nGoodExpressionArray.Length; i++)
            {
                if (m_fGoodPresentTime[i] < fGameTime || m_bAllowMultipleSameGoodExpression)
                {
                    availableExpression.Add(i);
                }
            }

            if (availableExpression.Count > 0)
            {
                nSelectedExpressionId = availableExpression[RrRndHandler.RndRange(0, availableExpression.Count)];
                nExpressionMask = m_nGoodExpressionArray[nSelectedExpressionId];
                return true;
            }

            return false;

        }

        EX_Expression.ExpressionsDataSequence sequence = GetSequenceForTime(0f);

        int nCounter = 0;
        do
        {
            int nEyeLeft = RrRndHandler.RndRange(0, sequence.nEyeDifferentSpriteCount);
            int nEyeRight = RrRndHandler.RndRange(0, sequence.nEyeDifferentSpriteCount);
            int nMouth = RrRndHandler.RndRange(0, sequence.nMouthDifferentSpriteCount);

            nExpressionMask = (ushort)((nMouth << 8) + (nEyeRight << 4) + nEyeLeft);
            nCounter++;
        } while (IsGood(nExpressionMask, nControlLength == -1 ? m_nGoodExpressionArray.Length : nControlLength) && nCounter < 100);

        return true;
    }

    private bool IsGood(ushort nTestedExpression, int nLength)
    {
        for (int i = 0; i < nLength; i++)
        {
            if (m_nGoodExpressionArray[i] == nTestedExpression)
            {
                return true;
            }
        }
        return false;
    }

    private bool ComputeIfGoodChoice(EX_Expression.ExpressionsDataSequence sequence, ref bool bRowIsGood, ref int nInRow)
    {
        Vector2 vMinMax = bRowIsGood ? sequence.goodInRow : sequence.badInRow;
        // check if min not reached
        if (nInRow < vMinMax.x)
        {
            nInRow++;
            return bRowIsGood;
        }

        // check if max exceeded
        if (nInRow >= vMinMax.y)
        {
            bRowIsGood = !bRowIsGood;
            nInRow = 1;
            return bRowIsGood;
        }

        int nRnd = RrRndHandler.RndRange(0, 100);
        bool bGood = nRnd <= sequence.goodExpressionPercent;
        if (bGood == bRowIsGood)
        {
            nInRow++;
        }
        else
        {
            bRowIsGood = bGood;
            nInRow = 1;
        }

        return bRowIsGood;
    }

    private EX_Expression.ExpressionsDataSequence GetSequenceForTime(float fCurrentTime)
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

}
