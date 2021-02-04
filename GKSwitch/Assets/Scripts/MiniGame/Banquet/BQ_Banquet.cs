using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class BQ_Banquet : MiniGameTemplate<BQ_Logic, BQ_Banquet.BanquetData, BQ_Banquet.BanquetBotData, MiniGameBasicHud, BQ_Balancing>
{
    public const float fMEAL_START_Y = 800f;
    public const float fMEAL_END_Y = -800f;

    public enum BanquetLoadable { bkg=1, meal=2, mealElt = 4 }


    [System.Serializable]
    public class BanquetData : MiniGameBalancingData
    {
        public int nElementsCount = 4;
        public int nColorsCount = 4;
        public BanquetDataSequence[] sequencesArray;
    }

    [System.Serializable]
    public class BanquetBotData : MiniGameBotData
    {
        public AnimationCurve clickBySecond;
        public AnimationCurve goodClickPercent;
    }

    [System.Serializable]
    public class BanquetDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float startTime;
        [Header("Meal")]
        public float conveyorSpeed;
        public float mealBySecond;
        public int goodMealPercent;
        public int maxBadMealInRow;
        [Header("Order")]
        public int orderShown;
        public int isNotEltPercent = 100;
        public int anyColorPercent = 100;
    }

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;
    [SerializeField]
    AssetReference[] m_mealEltReferences;
    [SerializeField]
    AssetReference m_mealReference;

    [Header("Game Config")]
    [SerializeField]
    private int m_nBeltSize = 100;

    private lwObjectPool<BQ_Meal> m_mealPool;
    private GameObject m_mealRoot;
    private BQ_MealElt[] m_mealEltPrefabs;
    private BQ_Meal m_mealPrefab;
    private BQ_MainObject m_bkg;
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(BanquetLoadable)).Length)) - 1;
    
    private float m_fConveyorSpeed;

    private BQ_PlayerInfos[] m_playerInfos;

    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.Banquet);
        m_gameLogic = new BQ_Logic();
        m_gameLogic.Init(m_nMiniGameDataSelected, m_gameData);
        SetupLogicDelegate();

        m_gameStats = new int[2] { 0, 0 };
    }

    public override void Init()
    {
        base.Init();
        InitPlayerInfos();
        StartLoading();
    }

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new BQ_PlayerInfos[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            m_playerInfos[i] = new BQ_PlayerInfos();
            m_playerInfos[i].Setup(i);
        }
    }

    protected override bool UpdateInit()
    {
        if(( m_loadMask & (int)BanquetLoadable.mealElt) == 0 && m_mealEltPrefabs!=null)
        {
            bool complete = true;
            for( int i=0; i<m_mealEltPrefabs.Length;i++ )
            {
                if( m_mealEltPrefabs[i]==null )
                {
                    complete = false;
                }
            }

            if( complete )
            {
                m_loadMask |= (int)BanquetLoadable.mealElt;
            }
        }
        


        if( m_loadMask!=m_loadMaskComplete)
        {
            return false;
        }
        return base.UpdateInit();
    }

    public override void Clean()
    {
        m_mealPool.Destroy();
        GameObject.Destroy(m_mealRoot);

        GameObject.Destroy(m_bkg);
        base.Clean();
    }

    protected override void UpdateWarmUp()
    {
        m_gameLogic.Update();
        CheckSpawn();
        CheckOrder();
    }

    protected override bool UpdateGamePlay()
    {
        CheckSpawn();
        CheckOrder();
        m_gameLogic.Update();
        return CheckAndUpdateTime();
    }

    protected override void SetupLogicDelegate()
    {
        m_gameLogic.onConveyorSpeedChange = SetConveyorSpeed;
    }

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        BQ_PlayerInfos player = m_playerInfos[playerId];
        player.ManageFireInput(v, buttonPhase);
    }

    private void SetConveyorSpeed(float fSpeed)
    {
        m_bkg.belt.SetSpeed(fSpeed);
        m_fConveyorSpeed = fSpeed;
    }

    private void StartLoading()
    {
        // bkg
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_bkgReference, OnBkgLoad);

        // meal
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_mealReference, OnMealLoad);

        // meal Elt
        m_mealEltPrefabs = new BQ_MealElt[m_mealEltReferences.Length];
        for ( int i=0; i<m_mealEltReferences.Length; i++ )
        {
            int id = i;
            RR_AdressableAsset.instance.LoadAsset<GameObject>(m_mealEltReferences[i], (AsyncOperationHandle<GameObject> obj) => {
                m_mealEltPrefabs[id] = obj.Result.GetComponent<BQ_MealElt>(); } );
        }
    }

    private void OnMealEltLoad(AsyncOperationHandle<BQ_MealElt> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            Debug.Log("Meal Elt load : " + obj.Result.name);
        }
        m_loadMask |= (int)BanquetLoadable.mealElt;
    }

    private void OnMealLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_mealPrefab = obj.Result.GetComponent<BQ_Meal>();
            m_loadMask |= (int)BanquetLoadable.meal;

            m_mealPool = new lwObjectPool<BQ_Meal>();
            m_mealRoot = new GameObject("MealRoot");
            m_mealPool.Init(m_mealPrefab, 20, m_mealRoot.transform);
        }
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            Debug.Log("Successfully loaded object.");
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<BQ_MainObject>();
            m_loadMask |= (int)BanquetLoadable.bkg;
        }

    }

    private void CheckSpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            BQ_Logic.BanquetSpawn spawn = m_gameLogic.CheckIfMealToSpawn();
            if (spawn != null)
            {
                SpawnMeal(spawn.fSpawnerRatioPos, spawn.nItemId, spawn.nColorId);
            }
            else
            {
                bContinue = false;
            }

        }
    }

    public void SpawnMeal(float fSpawnerRatioPos, int nItemId, int nColorId)
    {
        BQ_Meal meal = m_mealPool.GetInstance(transform);
        float fZ = 3f;
        meal.transform.localPosition = new Vector3((fSpawnerRatioPos - 0.5f) * m_nBeltSize, fMEAL_START_Y, fZ);
        meal.Setup(nItemId, nColorId, GetBeltSpeed, DeleteMeal, CanMoveMeal, IsMealOnTheBelt, m_gameLogic.HasColorInGame(), m_gameLogic.reorderEltId[nItemId],
            m_mealEltPrefabs[m_gameLogic.reorderEltId[nItemId]]);
    }

    private float GetBeltSpeed()
    {
        return m_fConveyorSpeed;
    }

    private void DeleteMeal(BQ_Meal meal)
    {
        //       Debug.Log("Real end time : " + m_networkConfig.gameTime );
        m_mealPool.PoolObject(meal);
    }

    private bool CanMoveMeal()
    {
        return m_miniGameState == MiniGameState.playing;
    }

    private bool IsMealOnTheBelt(Vector3 vPos)
    {
        return Mathf.Abs(vPos.x) - m_nBeltSize / 2f <= 0f;
    }

    private void TellValidMeal(bool bValid, int nSpotId)
    {
        /*int nPoints = m_nGoodPointsWin;
        if (bValid)
        {
            BattleContext.instance.AddPoint(nPoints);
            HudManager.instance.SpawnWinScore(m_orderSpots[nSpotId].transform.position, nPoints);
        }
        else
        {
            BattleContext.instance.AddPoint(-m_nBadPointsLost);
            HudManager.instance.SpawnLoseScore(m_orderSpots[nSpotId].transform.position, -m_nBadPointsLost);
        }

        m_conveyorBeltStats[bValid ? 0 : 1]++;
        CB_Order order = m_orderViews[nSpotId].GetOrder();
        m_networkConfig.ReleaseOrder(order.eltId, order.colorId, order.isElt);*/
    }

    private void CheckOrder()
    {
        /*bool bContinue = ComputeFreeSpotCount() > 0;

        while (bContinue)
        {
            BQ_Logic.OrderSpawn spawn = m_gameLogic.CheckIfOrderToSpawn();
            if (spawn != null)
            {
                int nSpot = GetSpotId();
                if (nSpot != -1)
                {

                    BQ_Order order = new BQ_Order(spawn.nItemId, spawn.nColorId, spawn.bIsElement, m_gameLogic.reorderEltId[spawn.nItemId]);
                    m_orderViews[nSpot].SetOrder(order);
                    //CB_OrderHud orderHud = m_hud.CreateOrder(m_mealPrefab.GetEltSprite(spawn.nItemId), m_mealPrefab.GetColorSprite(spawn.nColorId), spawn.bOrCommand, spawn.nCount);
                    //order.SetOrderHud(orderHud);
                    m_orderList.Add(order);
                    bContinue = ComputeFreeSpotCount() > 0;
                }
                else
                {
                    bContinue = false;
                }
            }
            else
            {
                bContinue = false;
            }
        }*/

    }
}
