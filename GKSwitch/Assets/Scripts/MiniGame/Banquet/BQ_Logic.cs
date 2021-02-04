using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_Logic : MiniGameLogic
{
    public enum RefreshAsk { speed = 0, recoverGameData, order, refillOrder };

    public float gameTime { get { return Time.time - m_fGameStartTime; } }

    public class OrderSpawn
    {
        public int nItemId;
        public int nColorId;
        /// <summary>
        /// Boolean to know if we want this element or if we accept all element except this one
        /// </summary>
        public bool bIsElement;

        public OrderSpawn(int _nItemId, int _nColorId, bool _bIsElement)
        {
            nColorId = _nColorId;
            nItemId = _nItemId;
            bIsElement = _bIsElement;
        }

        public bool IsMealValid(int _nItemId, int _nColorId)
        {
            bool bElt = bIsElement ? nItemId == _nItemId : nItemId != _nItemId;
            bool bColor = nColorId == -1 || _nColorId == nColorId;

            return bElt && bColor;
        }
    }

    public class BanquetSpawn
    {
        public float fSpawnerRatioPos;
        public float fSpawnTime;
        public int nItemId;
        public int nColorId;

        public BanquetSpawn(float _fSpawnerRatioPos, float _fSpawnTime, int _nItemId, int _nColorId)
        {
            fSpawnerRatioPos = _fSpawnerRatioPos;
            nColorId = _nColorId;
            fSpawnTime = _fSpawnTime;
            nItemId = _nItemId;
        }
    }
    /*    [SyncVar]
        private int m_nGameDataId;*/
    public class MealSpawn
    {
        public bool bGood;
        public float fSpawnTime;

        public MealSpawn(float _fSpawnTime, bool _bGood)
        {
            bGood = _bGood;
            fSpawnTime = _fSpawnTime;
        }
    }

    public System.Action<float> onConveyorSpeedChange { set { m_onConveyorSpeedChange = value; } }
    public int[] reorderEltId { get { return m_nReorderEltId; } }

    private Queue<MealSpawn> m_mealSpawnQueue = new Queue<MealSpawn>();
    private Queue<OrderSpawn> m_orderSpawnQueue = new Queue<OrderSpawn>();

    private BQ_Banquet.BanquetData m_gameData;
    private int m_gameDataCurrentSequenceId = 0;
    private int m_nBadElementInRow = 0;
    private int[] m_nReorderEltId;

    private System.Action<float> m_onConveyorSpeedChange;

    private List<OrderSpawn> m_orderInfosList;
    private List<OrderSpawn> m_currentOrderList;

    private int m_nLastColorId;
    private int m_nLastItemId;

    private uint m_nGlobalInitSeed = 1;
    private uint m_nOrderInitSeed = 1;
    private uint m_nMealInitSeed = 1;
    private uint m_nReorderInitSeed = 1;


    internal void Init(byte nGameData, BQ_Banquet.BanquetData gameData)
    {
        m_fGameStartTime = Time.time;
        m_currentOrderList = new List<OrderSpawn>();

        m_nGameDataId = nGameData;
        m_gameData = gameData;
        m_nGlobalInitSeed = (uint)Random.Range(1, int.MaxValue);
        RrRndHandler.RndSeed(m_nGlobalInitSeed);
        m_nReorderInitSeed = RrRndHandler.RndRange(1, uint.MaxValue);
        m_nMealInitSeed = RrRndHandler.RndRange(1, uint.MaxValue);
        m_nOrderInitSeed = RrRndHandler.RndRange(1, uint.MaxValue);

        SetGameRefill();
        GenerateOrderWithSeed(m_nOrderInitSeed);
        GenerateReorderElts(m_nReorderInitSeed);

        GetCurrentSequence();
        float fNewSpeed = m_gameData.sequencesArray[m_gameDataCurrentSequenceId].conveyorSpeed;
        SetConveyorSpeed(fNewSpeed);
    }

    private void GenerateReorderElts(uint nSeed)
    {
        RrRndHandler.RndSeed(nSeed);
        m_nReorderEltId = new int[6];
        for (int i = 0; i < 6; i++)
        {
            m_nReorderEltId[i] = i;
        }
        System.Array.Sort(m_nReorderEltId, RandomSort);
    }

    // this function just returns a number in the range -1 to +1
    // and is used by Array.Sort to 'shuffle' the array
    int RandomSort(int a, int b)
    {
        return RrRndHandler.RndRange(-1, 2);

    }


    public BanquetSpawn CheckIfMealToSpawn()
    {
        float fTime = Time.time - m_fGameStartTime;
        if (m_mealSpawnQueue.Count > 0)
        {
            //bool bGoodMeal = m


            MealSpawn spawn = m_mealSpawnQueue.Peek();
            if (spawn.fSpawnTime < fTime)
            {
                m_mealSpawnQueue.Dequeue();
                if (m_mealSpawnQueue.Count == 0 )
                {
                    Refill();
                }

                bool bGood = spawn.bGood;
                int nItemId = 0;
                int nColorId = 0;
                SearchMeal(bGood, ref nItemId, ref nColorId);
                return new BanquetSpawn(Random.Range(0f, 1f), spawn.fSpawnTime, nItemId, nColorId);
            }
        }
        else
        {
            Refill();
        }

        return null;
    }

    public OrderSpawn CheckIfOrderToSpawn()
    {
        if (m_orderSpawnQueue.Count == 0 || m_gameData == null)
        {
            return null;
        }


        GetCurrentSequence();
        int nOrderNeed = m_gameData.sequencesArray[m_gameDataCurrentSequenceId].orderShown;
        int nCurrent = m_currentOrderList.Count;
        if (nCurrent < nOrderNeed)
        {
            OrderSpawn spawn = m_orderSpawnQueue.Peek();
            m_orderSpawnQueue.Dequeue();
            m_currentOrderList.Add(spawn);

            if (m_orderSpawnQueue.Count <= 1)
            {
                GenerateOrder();
            }
            return spawn;
        }

        return null;
    }


    public void ReleaseOrder(int nEltId, int nColorId, bool bIsElt)
    {
        bool bFound = false;
        int nOrderId = 0;

        while (!bFound && nOrderId < m_currentOrderList.Count)
        {
            if (m_currentOrderList[nOrderId].nItemId == nEltId &&
                m_currentOrderList[nOrderId].nColorId == nColorId &&
                m_currentOrderList[nOrderId].bIsElement == bIsElt)
            {
                bFound = true;
            }
            else
            {
                nOrderId++;
            }
        }

        if (bFound)
        {
            m_currentOrderList.RemoveAt(nOrderId);
        }
    }

    public void Update()
    {
        if (m_gameData != null)
        {
            int nCurrentSequence = m_gameDataCurrentSequenceId;
            GetCurrentSequence();
            if (nCurrentSequence != m_gameDataCurrentSequenceId)
            {
                // Check speed change
                float fPreviousSpeed = nCurrentSequence >= 0 ? m_gameData.sequencesArray[nCurrentSequence].conveyorSpeed : 0;
                float fNewSpeed = m_gameData.sequencesArray[m_gameDataCurrentSequenceId].conveyorSpeed;

                if (fPreviousSpeed != fNewSpeed)
                {
                    SetConveyorSpeed(fNewSpeed);
                }
            }
        }
    }


    public void SetGameRefill()
    {
        SetGameRefillWithSeed(m_nMealInitSeed);
    }

    public void Refill()
    {
        uint seed = RrRndHandler.RndRange(1, uint.MaxValue);
        SetGameRefillWithSeed(seed);
    }

    public void SetGameRefillWithSeed(uint nSeed)
    {
        float fGameTime = Time.time - m_fGameStartTime;
        RrRndHandler.RndSeed(nSeed);


        int nMealCount = 50;
        float fSpawnTime = fGameTime;
        bool bGoodSpawn;

        for (int nMealSpawn = 0; nMealSpawn < nMealCount; nMealSpawn++)
        {
            BQ_Banquet.BanquetDataSequence dataSequence = GetSequence(fSpawnTime);
            float fTime = 1f / dataSequence.mealBySecond;
            fSpawnTime += fTime;

            int nRnd = RrRndHandler.RndRange(0, 100);
            bool bSearch = m_nBadElementInRow >= dataSequence.maxBadMealInRow ? true : nRnd < dataSequence.goodMealPercent;
            m_nBadElementInRow = bSearch ? 0 : m_nBadElementInRow + 1;
            bGoodSpawn = bSearch;
            m_mealSpawnQueue.Enqueue(new MealSpawn(fSpawnTime, bGoodSpawn));
        }
    }

    public float GetCurrentSpeed()
    {
        if (m_gameData == null)
        {
            return -1f;
        }
        GetCurrentSequence();
        return m_gameData.sequencesArray[m_gameDataCurrentSequenceId].conveyorSpeed;
    }

    public void GenerateOrder()
    {
        m_nOrderInitSeed = (uint)Random.Range(1, int.MaxValue);
        GenerateOrderWithSeed(m_nOrderInitSeed);

    }

    public void GenerateOrderWithSeed(uint nSeed)
    {
        BQ_Banquet.BanquetDataSequence sequence = GetCurrentSequence();
        RrRndHandler.RndSeed(nSeed);

        //HudManager.instance.SetDebugText(" nSeed : " + nSeed + " ## " +RrRndHandler.seed() + " ## " + int.MaxValue + " value " + ((uint)(((double)(nSeed * 16807)) % int.MaxValue)));
        int nItemId;
        int nColorId;
        bool bIsElt;

        int nOrderCount = 100;

        //HudManager.instance.AddDebugText(" GenerateOrderWithSeed : " + m_gameData.nElementsCount + " / " + (sequence!=null));
        string s = "";

        for (int nOrderSpawn = 0; nOrderSpawn < nOrderCount; nOrderSpawn++)
        {
            nItemId = RrRndHandler.RndRange(0, m_gameData.nElementsCount);

            int nColorRnd = RrRndHandler.RndRange(0, 100);
            nColorId = nColorRnd < sequence.anyColorPercent ? -1 : RrRndHandler.RndRange(0, m_gameData.nColorsCount);

            int nRnd = RrRndHandler.RndRange(0, 100);
            bIsElt = (nRnd >= sequence.isNotEltPercent);
            // s += " nItemId : " + nItemId + " / " + m_gameData.nElementsCount + " nRnd : " + nRnd + " seed " + RrRndHandler.seed() + "\n" ;

            m_orderSpawnQueue.Enqueue(new OrderSpawn(nItemId, nColorId, bIsElt));
        }

        //HudManager.instance.AddDebugText(s);
    }


    
    private BQ_Banquet.BanquetDataSequence GetCurrentSequence()
    {
        float fCurrentTime = Time.time - m_fGameStartTime;
        while (m_gameDataCurrentSequenceId < m_gameData.sequencesArray.Length - 1 &&
            fCurrentTime >= m_gameData.sequencesArray[m_gameDataCurrentSequenceId + 1].startTime)
        {
            m_gameDataCurrentSequenceId++;
        }
        Debug.Assert(m_gameDataCurrentSequenceId >= 0 && m_gameDataCurrentSequenceId < m_gameData.sequencesArray.Length);
        return m_gameData.sequencesArray[m_gameDataCurrentSequenceId];
    }

    private BQ_Banquet.BanquetDataSequence GetSequence(float fTime)
    {
        int nSequenceId = 0;
        while (nSequenceId < m_gameData.sequencesArray.Length - 1 &&
            fTime >= m_gameData.sequencesArray[nSequenceId + 1].startTime)
        {
            nSequenceId++;
        }
        Debug.Assert(nSequenceId >= 0 && nSequenceId < m_gameData.sequencesArray.Length);
        return m_gameData.sequencesArray[nSequenceId];
    }

    private void SetConveyorSpeed(float fSpeed)
    {
        m_onConveyorSpeedChange?.Invoke(fSpeed);
    }



    #region GameBalancing

    private void SearchMeal(bool bSearch, ref int nEltId, ref int nColorId)
    {
        bool[] allowedMeal = new bool[m_gameData.nElementsCount * m_gameData.nColorsCount];
        bool[] withoutPreviousAllowedMeal = new bool[m_gameData.nElementsCount * m_gameData.nColorsCount];
        int nStayingOrderCount = 0;
        for (int elt = 0; elt < m_gameData.nElementsCount * m_gameData.nColorsCount; elt++)
        {
            allowedMeal[elt] = false;
            withoutPreviousAllowedMeal[elt] = false;
        }

        List<OrderSpawn> orderList = m_currentOrderList;
        for (int nOrderId = 0; nOrderId < orderList.Count; nOrderId++)
        {
            if (!orderList[nOrderId].IsMealValid(m_nLastItemId, m_nLastColorId))
            {
                nStayingOrderCount++;
                UpdateAllowedMeal(ref withoutPreviousAllowedMeal, orderList[nOrderId], m_gameData.nElementsCount, m_gameData.nColorsCount);
            }
            UpdateAllowedMeal(ref allowedMeal, orderList[nOrderId], m_gameData.nElementsCount, m_gameData.nColorsCount);
        }

        /*  string s = "";
          int nId = 0;
          for( int i=0; i<m_gameData.nElementsCount; i++ )
          {
              for (int j = 0; j < m_gameData.nColorsCount; j++)
              {
                  s += " " + (allowedMeal[nId] ? "1" : "0");
                  nId++;
              }
              s += "\n";
          }
          Debug.Log(s);*/

        int nRnd = Random.Range(0, allowedMeal.Length);
        bool[] searchArray = nStayingOrderCount > 0 && bSearch ? withoutPreviousAllowedMeal : allowedMeal;
        int nSelectedId = nRnd;
        bool bFound = false;
        do
        {
            if (searchArray[nSelectedId] == bSearch)
            {
                bFound = true;
            }
            else
            {
                nSelectedId = (nSelectedId + 1) % searchArray.Length;
            }
        }
        while (!bFound && nRnd != nSelectedId);

        /*       if( !bFound )
               {
                   Debug.LogWarning("condition not found for : " + bSearch);
               }*/

        nEltId = nSelectedId % m_gameData.nElementsCount;
        nColorId = (int)(nSelectedId / m_gameData.nElementsCount);

        m_nLastColorId = nColorId;
        m_nLastItemId = nEltId;
    }

    public bool HasColorInGame()
    {
        if (m_gameData == null)
        {
            return true;
        }

        bool bColor = false;
        int nSequenceId = 0;
        while (nSequenceId < m_gameData.sequencesArray.Length && !bColor)
        {
            if (m_gameData.sequencesArray[nSequenceId].anyColorPercent < 100)
            {
                bColor = true;
            }
            else
            {
                nSequenceId++;
            }
        }
        return bColor;
    }

    public void UpdateAllowedMeal(ref bool[] allowedMeal, OrderSpawn order, int nEltCount, int nClrCount)
    {
        if (order.bIsElement)
        {
            if (order.nColorId == -1)
            {
                // Elt
                for (int nClrId = 0; nClrId < nClrCount; nClrId++)
                {
                    allowedMeal[nEltCount * nClrId + order.nItemId] = true;
                }
            }
            else
            {
                allowedMeal[nEltCount * order.nColorId + order.nItemId] = true;
            }


        }
        else
        {
            if (order.nColorId == -1)
            {
                // Elt
                for (int nClrId = 0; nClrId < nClrCount; nClrId++)
                {
                    // Color
                    for (int nEltId = 0; nEltId < nEltCount; nEltId++)
                    {
                        if (nEltId != order.nItemId)
                        {
                            allowedMeal[nEltCount * nClrId + nEltId] = true;
                        }
                    }
                }
            }
            else
            {
                // Color
                for (int nEltId = 0; nEltId < nEltCount; nEltId++)
                {
                    if (nEltId != order.nItemId)
                    {
                        allowedMeal[nEltCount * order.nColorId + nEltId] = true;
                    }
                }
            }
        }

    }
    #endregion

}
