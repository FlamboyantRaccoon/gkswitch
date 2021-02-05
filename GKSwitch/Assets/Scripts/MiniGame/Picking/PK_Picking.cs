using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class PK_Picking : MiniGameTemplate<PK_Logic, PK_Picking.PickingData, PK_Picking.PickingBotData, MiniGameBasicHud, PK_Balancing>
{
    public const float fITEM_START_Y = 750;
    public const float fITEM_END_Y = -450f;
    public const float fITEM_SIZE_X = 130f;
    public const float fITEM_LAUNCH_WIDTH = 1040f; // 900f, 600f, 450f };
    public readonly float[] fCAMERA_SIZE = new float[] { 600f, 800f, 900f, 1000f }; // 900f, 600f, 450f };
    public readonly float[] fCAMERA_Y = new float[] { 0f, 200f, 300f, 400f }; // 900f, 600f, 450f };
    public const float fPICKZONEY = -380f;
    public const float fSECONDTOUCH_JUMP = 560f;

    public enum PivkingLoadable { bkg = 1, playerCanvas = 2, item = 4, basket = 8 }
    public enum itemType { basic, fast, doubleTouch, bad, doubleTouchOther }

    [System.Serializable] public class itemTypeFloat : lwEnumArray<itemType, float> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class itemTypeString : lwEnumArray<itemType, string> { }; // dummy definition to use Unity serialization


    //    event:/Picking/PickingCherryGoodPerfect
    [System.Serializable]
    public class PickingData : MiniGameBalancingData
    {
        [Header("Pick")]
        public float fTolerance = 100f;
        public float fPerfectTolerance = 50f;
        [Tooltip("the double touch other speed if for the flying move")]
        public float referenceSpeed = 100f;
        public PickingDataDataSequence[] sequencesArray;
    }

    [System.Serializable]
    public class PickingDataDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float startTime;
        [Range(1, 6)]
        public int nLanes;
        [Header("Fruit")]
        public float itemsBySecond = 1f;
        public itemTypeFloat itemsWeight;

    }

    [System.Serializable]
    public class PickingBotData : MiniGameBotData
    {
        public AnimationCurve clickBySecond;
        public AnimationCurve goodClickPercent;

        public bool bUseNewBehavior = false;
        public AnimationCurve goodItemChance;
        public AnimationCurve badItemChance;
        public AnimationCurve perfectChance;
    }

    [Header("Game Config")]
    [SerializeField]
    private int m_nBadPickingPointsLost = 15;
    [SerializeField]
    private itemTypeFloat m_itemSpeedRatio;

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;
    [SerializeField]
    private AssetReference m_playerCanvasReference;
    [SerializeField]
    private AssetReference m_itemReference;
    [SerializeField]
    private AssetReference m_basketReference;



    private PK_PlayerCanvas[] m_playerInfos;

    PK_PlayerCanvas m_playerCanvasPrefab;
    PK_Item m_itemPrefab;
    PK_Basket m_basketPrefab;

    private lwObjectPool<PK_Item> m_itemPool;
    private GameObject m_itemRoot;
    private int m_nTotalLane = -1;

    private PK_MainObject m_bkg;
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(PivkingLoadable)).Length)) - 1;

    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.TrickOrTreat);
    }

    protected override bool UpdateInit()
    {

        if (m_loadMask != m_loadMaskComplete)
        {
            return false;
        }

        bool bReturn = base.UpdateInit();
        if (bReturn)
        {
            InitAfterLoad();
        }
        return bReturn;
    }

    public override void Init()
    {
        base.Init();
        
        StartLoading();
        m_gameStats = new int[2] { 0, 0 };
    }

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new PK_PlayerCanvas[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            m_playerInfos[i] = GameObject.Instantiate<PK_PlayerCanvas>(m_playerCanvasPrefab, transform );
            m_playerInfos[i].transform.position = new Vector3(GetPlayerOffsetX(i), 0f, 0f);
            m_playerInfos[i].Setup(i, fCAMERA_SIZE[playerCount-1], fCAMERA_Y[playerCount-1]);
            m_playerInfos[i].SetCameraRegion(playerCount);
        }
        HudManager.sSPLITHUD_COUNT = playerCount;
        HudManager.sSPLITHUD_TYPE = HudManager.SplitHudType.vertical;
    }

    private float GetPlayerOffsetX( int playerId )
    {
        return 4000f * (playerId + 1);
    }

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if (m_playerInfos != null)
        {
            PK_PlayerCanvas player = m_playerInfos[playerId];
            player.ManageFireInput(v, buttonPhase);
        }
    }

    public override void Clean()
    {
        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            m_playerInfos[i].Clean();
            GameObject.Destroy(m_playerInfos[i].gameObject);
        }

        m_itemPool.Destroy();
        GameObject.Destroy(m_itemRoot);

        GameObject.Destroy(m_bkg.gameObject);
        Addressables.ClearResourceLocators();
        base.Clean();
    }

    void InitAfterLoad()
    {
        
        InitPlayerInfos();

        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            PK_Basket basket = GameObject.Instantiate<PK_Basket>(m_basketPrefab, transform);
            basket.playerId = i;
            //GKPlayerData playerData = BattleContext.instance.GetPlayer(i);
            //toasty.SetAvatar(toasties.GetToasty(playerData.sToastyId).avatar);

            m_playerInfos[i].SetBasket( basket );
        }


        m_itemPool = new lwObjectPool<PK_Item>();
        m_itemRoot = new GameObject("ItemRoot");
        m_itemPool.Init(m_itemPrefab, 20, m_itemRoot.transform);

        m_gameLogic = new PK_Logic();
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected, m_itemSpeedRatio);
        m_nTotalLane = m_gameLogic.ComputeLaneCount();
    }

    protected override void UpdateWarmUp()
    {
        CheckSpawn();
    }

    protected override bool UpdateGamePlay()
    {
        CheckSpawn();

        return CheckAndUpdateTime();
    }

    #region item
    private void CheckSpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            PK_Logic.ItemSpawnInfo spawn = m_gameLogic.CheckIfItemToSpawn();
            if (spawn != null)
            {
                SpawnItem(spawn.nLineId, spawn.nLineId2, spawn.itemType);
                //SpawnMeal(spawn.fSpawnerRatioPos, spawn.nItemId, spawn.nColorId);
            }
            else
            {
                bContinue = false;
            }

        }
    }

    private void SpawnItem(int nLaneId, int nReboundLaneId, itemType itemType)
    {
        for( int i=0; i<m_playerInfos.Length; i++ )
        {
            SpawnItemForPlayer(i, nLaneId, nReboundLaneId, itemType);
        }
    }

    private PK_Item SpawnItemForPlayer( int playerId, int nLaneId, int nReboundLaneId, itemType itemType )
    {
        float fSpeed = m_gameData.referenceSpeed * m_itemSpeedRatio[(int)itemType];
        PK_Item item = m_itemPool.GetInstance(m_itemRoot.transform);
        item.transform.localPosition = new Vector3(ComputeItemXFromLaneId(nLaneId) + GetPlayerOffsetX(playerId), fITEM_START_Y, 0f);
        float fEndZoneY = fPICKZONEY - m_gameData.fTolerance;
        item.Setup(itemType, fSpeed, fEndZoneY, playerId, DeleteItem, OnItemFallout, nLaneId, nReboundLaneId, GetMoveMultiplier, OnItemPickup);
        return item;
    }

    private void OnItemPickup(PK_Item item, PK_Basket basket )
    {
        if (isPlaying)
        {
            bool bGood = item.itemType != itemType.bad;
            AddPoint(bGood, basket.playerId,  item.transform.position - m_playerInfos[basket.playerId].transform.position, m_nGoodPointsWin);

            if (item.itemType == itemType.doubleTouch && item.nReboundLineId >= 0)
            {
                int nReboundLaneId = item.nReboundLineId;
                float fXRebound = ComputeItemXFromLaneId(nReboundLaneId) + GetPlayerOffsetX(basket.playerId);
                PK_Item secondTouch = SpawnItemForPlayer( basket.playerId, nReboundLaneId, -1, itemType.doubleTouchOther);
                secondTouch.fSpeed = item.fSpeed;
                secondTouch.transform.position = item.transform.position;
                secondTouch.PlayRebound(fXRebound, m_gameData.referenceSpeed * m_itemSpeedRatio[(int)itemType.doubleTouchOther]);
                item.RemoveSecondSprite();
            }

            //item.bMoving = false;
            item.PlayEndAnim(true);
            basket.PlayPickAnim();
            /*
            bool bSoundPlayed = false;
            if (bPerfect && !string.IsNullOrEmpty(m_itemsSoundPerfect[item.itemType]))
            {
                bSoundPlayed = true;
                // Debug.Log("Play perfect sound : " + m_itemsSoundPerfect[item.itemType]);
                FMODUnity.RuntimeManager.PlayOneShot(m_itemsSoundPerfect[item.itemType]);
            }

            if (!bSoundPlayed && !string.IsNullOrEmpty(m_itemsSound[item.itemType]))
            {
                //Debug.Log("Play normal sound : " + m_itemsSound[item.itemType]);
                FMODUnity.RuntimeManager.PlayOneShot(m_itemsSound[item.itemType]);
            }*/
        }
    }

    private void AddPoint(bool bGood, int playerId, Vector3 vPos, int nPoints)
    {
        if (bGood)
        {
            BattleContext.instance.AddPoint(nPoints, playerId);
            HudManager.instance.SpawnWinScore(vPos, nPoints, playerId);
        }
        else
        {
            BattleContext.instance.AddPoint(-nPoints, playerId);
            HudManager.instance.SpawnLoseScore(vPos, -nPoints, playerId);
            bGood = false;
        }
        m_gameStats[bGood ? 0 : 1]++;
    }

    private float GetMoveMultiplier()
    {
        if (m_miniGameState == MiniGameState.countdown)
        {
            return m_gameLogic.fWarmUpSpeedMultiplier;
        }
        return 1f;
    }

    private float ComputeItemXFromLaneId(int nLaneId)
    {
        float width = fITEM_LAUNCH_WIDTH; //[m_playerInfos.Length];
        return (((float)nLaneId + 0.5f) * width / (float)m_nTotalLane) - (width / 2f);

    }

    private void DeleteItem(PK_Item item)
    {
        //       Debug.Log("Real end time : " + m_networkConfig.gameTime );
        m_itemPool.PoolObject(item);
    }

    private void OnItemFallout(PK_Item item, int playerId )
    {
        if (isPlaying)
        {
            AddPoint(item.itemType == itemType.bad, playerId, item.transform.position - m_playerInfos[playerId].transform.position, m_nBadPickingPointsLost );
            item.PlayEndAnim(item.itemType == itemType.bad);
        }
        else
        {
            DeleteItem(item);
        }
    }
    #endregion


    #region loading
    private void StartLoading()
    {
        // bkg
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_bkgReference, OnBkgLoad);

        // Canvas
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_playerCanvasReference, OnCanvasLoad);

        // Item
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_itemReference, OnItemLoad);

        // Basket
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_basketReference, OnBasketLoad);
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            Debug.Log("Successfully loaded object.");
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<PK_MainObject>();
            m_loadMask |= (int)PivkingLoadable.bkg;
        }

    }

    private void OnCanvasLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_playerCanvasPrefab = obj.Result.GetComponent<PK_PlayerCanvas>();
        }

        m_loadMask |= (int)PivkingLoadable.playerCanvas;
    }

    private void OnItemLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_itemPrefab = obj.Result.GetComponent<PK_Item>();
        }

        m_loadMask |= (int)PivkingLoadable.item;
    }

    private void OnBasketLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_basketPrefab = obj.Result.GetComponent<PK_Basket>();
        }

        m_loadMask |= (int)PivkingLoadable.basket;
    }
    #endregion
}
