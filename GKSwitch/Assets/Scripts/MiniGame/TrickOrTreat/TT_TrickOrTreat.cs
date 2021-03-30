using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TT_TrickOrTreat : MiniGameTemplate<TT_Logic, TT_TrickOrTreat.TrickOrTreatData, TT_TrickOrTreat.TrickOrTreatBotData, MiniGameBasicHud, TT_Balancing>
{
    public enum CandyType { normal, flash, gold }
    public enum EnemyType { ghosty, pumpky, frankensty }
    public enum TrickOrTreatLoadable { bkg = 1, candy = 2, cursor = 4, ennemy = 8 }

    public const float TOASTY_Z = 25f;

    [System.Serializable]
    public class CandyData
    {
        public float fPointCoeff;
        public float fShowTime;
        public string sCandyFx;
    }

    [System.Serializable] public class CandyDatas : lwEnumArray<CandyType, CandyData> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class CandyFloat : lwEnumArray<CandyType, float> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class CandyPrefab : lwEnumArray<CandyType, TT_Candy> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class EnemyPrefab : lwEnumArray<EnemyType, TT_Enemy> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class CandyReferences : lwEnumArray<CandyType, AssetReference> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class EnemyReferences : lwEnumArray<EnemyType, AssetReference> { }; // dummy definition to use Unity serialization

    [System.Serializable]
    public class TrickOrTreatData : MiniGameBalancingData
    {
        public int nGridColumnCount = 3;
        public int nGridRowCount = 4;
        public int nPumpkyCount = 0;
        [lwMinMaxVector(10f, 1000f)]
        public Vector2 fPumpkySpeed;
        public int nFrankyCount = 0;
        public int nCandyMax = 5;
        [lwMinMaxVector(10f, 1000f)]
        public Vector2 fFrankySpeed;
        public TrickOrTreatDataSequence[] sequencesArray;
    }

    [System.Serializable]
    public class TrickOrTreatDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float startTime;
        [Header("Candy")]
        public float itemsBySecond = 1f;
        public CandyFloat candyWeight;
        [Header("Ghost")]
        public float ghostBySecond = 1f;
        [lwMinMaxVector(10f, 1000f)]
        public Vector2 fGhostSpeed;
    }

    [System.Serializable]
    public class TrickOrTreatBotData : MiniGameBotData
    {
        [Tooltip("determine chance to be hit by enemy")]
        public AnimationCurve percentChanceToBeHitByEnemy;
        [lwMinMaxVector(10f, 1000f)]
        public Vector2 minTimeBeforeEnemyHit;

        public AnimationCurve percentChanceToTakeFlashCandy;
        [lwMinMaxVector(10f, 1000f)]
        public Vector2 timeToTakeCandy;
    }

    [Header("Config")]
    [SerializeField]
    private int m_nBadPointsLost = 200;
    [SerializeField]
    private CandyDatas m_candyDatas;
    [SerializeField]
    private float m_fInvincibleTime = 2;

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;
    [SerializeField]
    CandyReferences m_candyReference;
    [SerializeField]
    AssetReference m_cursorReference;
    [SerializeField]
    EnemyReferences m_enemyReference;

    [SerializeField]
    private float m_playerScale = 1f;
    [SerializeField]
    private float m_enemyScale = 1f;
    [SerializeField]
    private float m_candyScale = 1f;

    private CandyPrefab m_candyPrefabs;
    private TT_Toasty m_toastyPrefab;
    private EnemyPrefab m_enemyPrefab;


    private lwObjectPool<TT_Candy>[] m_candysPool;
    private lwObjectPool<TT_Enemy>[] m_enemyPool;
    private GameObject m_candyRoot;
    private GameObject m_enemyRoot;
    public static Rect s_gameArea;

    private TT_PlayerInfos[] m_playerInfos;

    private TT_MainObject m_bkg;
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(TrickOrTreatLoadable)).Length)) - 1;


    /// <summary>
    /// Bot datas
    /// </summary>
    private float m_fBotNextEnemyTimer = -1;
    private List<TT_Logic.ItemBotInfo> m_botItemInfoList = null;

    public static bool IsPointOutGameArea( Vector3 vPoint, ref Vector3 vDir, ref Vector3 vColisionPoint )
    {
        Vector2 vInter = Vector2.zero;
        Vector2 vA = (Vector2)(vPoint);
        Vector2 vB = (Vector2)( vPoint + vDir );

        // test left
        Vector2 vC = new Vector2(s_gameArea.x, s_gameArea.y);
        Vector2 vD = new Vector2(s_gameArea.x, s_gameArea.y + s_gameArea.height );
        if( RRGeometry.ComputeSegmentIntersection( vA, vB, vC, vD, ref vInter ) )
        {
            vColisionPoint = (Vector3)vInter;
            vDir.x = -vDir.x;
            return true;
        }

        // test right
        vC = new Vector2(s_gameArea.x + s_gameArea.width, s_gameArea.y);
        vD = new Vector2(s_gameArea.x + s_gameArea.width, s_gameArea.y + s_gameArea.height);
        if (RRGeometry.ComputeSegmentIntersection(vA, vB, vC, vD, ref vInter))
        {
            vColisionPoint = (Vector3)vInter;
            vDir.x = -vDir.x;
            return true;
        }
        
        // test up
        vC = new Vector2(s_gameArea.x, s_gameArea.y + s_gameArea.height);
        vD = new Vector2(s_gameArea.x + s_gameArea.width, s_gameArea.y + s_gameArea.height);
        if (RRGeometry.ComputeSegmentIntersection(vA, vB, vC, vD, ref vInter))
        {
            vColisionPoint = (Vector3)vInter;
            vDir.y = -vDir.y;
            return true;
        }
        
        // test down
        vC = new Vector2(s_gameArea.x, s_gameArea.y);
        vD = new Vector2(s_gameArea.x + s_gameArea.width, s_gameArea.y );
        if (RRGeometry.ComputeSegmentIntersection(vA, vB, vC, vD, ref vInter))
        {
            vColisionPoint = (Vector3)vInter;
            vDir.y = -vDir.y;
            return true;
        }

        return false;
    }

    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.TrickOrTreat);
        AimingHud aimingHud = HudManager.instance.GetForeHud<AimingHud>(HudManager.ForeHudType.aimingHud);
        aimingHud.Hide();
    }

    protected override bool UpdateInit()
    {
        if ((m_loadMask & (int)TrickOrTreatLoadable.candy) == 0 )
        {
            bool complete = true;
            for (int i = 0; i < m_candyPrefabs.nLength; i++)
            {
                if (m_candyPrefabs[i] == null)
                {
                    complete = false;
                }
            }

            if (complete)
            {
                m_loadMask |= (int)TrickOrTreatLoadable.candy;
            }
        }

        if ((m_loadMask & (int)TrickOrTreatLoadable.ennemy) == 0)
        {
            bool complete = true;
            for (int i = 0; i < m_enemyPrefab.nLength; i++)
            {
                if (m_enemyPrefab[i] == null)
                {
                    complete = false;
                }
            }

            if (complete)
            {
                m_loadMask |= (int)TrickOrTreatLoadable.ennemy;
            }
        }


        if (m_loadMask != m_loadMaskComplete)
        {
            return false;
        }

        bool bReturn = base.UpdateInit();
        if( bReturn )
        {
            InitAfterLoad();
        }
        return bReturn;
    }

    public override void Init()
    {
        base.Init();
        InitPlayerInfos();
        StartLoading();
        m_gameStats = new int[2] { 0, 0 };
    }

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new TT_PlayerInfos[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            m_playerInfos[i] = new TT_PlayerInfos();
            m_playerInfos[i].Setup(i);
        }
    }

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if( m_playerInfos!=null )
        {
            TT_PlayerInfos player = m_playerInfos[playerId];
            player.ManageFireInput(v, buttonPhase);
        }
    }

    public override void Clean()
    {
        for( int i=0; i<m_candysPool.Length; i++ )
        {
            m_candysPool[i].Destroy();
        }

        for (int i = 0; i < m_enemyPool.Length; i++)
        {
            m_enemyPool[i].Destroy();
        }

        for( int i=0; i<m_playerInfos.Length; i++ )
        {
            m_playerInfos[i].Clean();
        }

        GameObject.Destroy(m_candyRoot);
        GameObject.Destroy(m_enemyRoot);

        GameObject.Destroy(m_bkg.gameObject);

        AimingHud aimingHud = HudManager.instance.GetForeHud<AimingHud>(HudManager.ForeHudType.aimingHud);
        aimingHud.Show();

        base.Clean();
    }

    protected override void UpdateWarmUp()
    {
        CheckCandySpawn();
    }

    protected override bool UpdateGamePlay()
    {
        CheckCandySpawn();
        CheckGhostSpawn();
        //CheckSpawn();
        return CheckAndUpdateTime();
    }

    private bool CanMove()
    {
        return m_miniGameState == MiniGameState.playing;
    }

    private void GeneratePermanentEnemy()
    {
        Transform[] players = new Transform[m_playerInfos.Length];
        for( int i=0; i<players.Length;i++ )
        {
            players[i] = m_playerInfos[i].m_toasty.transform;
        }


        List<TT_Logic.FrankySpawnInfo> frankySpawnInfos = m_gameLogic.frankyList;
        if (frankySpawnInfos != null)
        {
            for (int i = 0; i < frankySpawnInfos.Count; i++)
            {
                TT_Logic.FrankySpawnInfo info = frankySpawnInfos[i];
                TT_Enemy enemy = m_enemyPool[(int)EnemyType.frankensty].GetInstance(m_enemyRoot.transform);
                enemy.transform.position = new Vector3(info.fX, info.fY, 0f);
                enemy.transform.localScale = new Vector3(m_enemyScale, m_enemyScale, m_enemyScale);
                enemy.Setup(EnemyType.frankensty, players, CanMove, null, OnToastyHit, info.fSpeed );
            }
        }
        
        List<TT_Logic.PumpkySpawnInfo> pumpkySpawnInfos = m_gameLogic.pumpkyList;
        if( pumpkySpawnInfos!=null )
        {
            for( int i=0; i<pumpkySpawnInfos.Count; i++ )
            {
                TT_Logic.PumpkySpawnInfo info = pumpkySpawnInfos[i];
                TT_Enemy enemy = m_enemyPool[(int)EnemyType.pumpky].GetInstance(m_enemyRoot.transform);
                enemy.transform.position = new Vector3( info.fX, info.fY, 0f);
                enemy.transform.localScale = new Vector3(m_enemyScale, m_enemyScale, m_enemyScale);
                enemy.Setup(EnemyType.pumpky, new Vector3(info.fDirX, info.fDirY, 0f), CanMove, null, OnToastyHit, info.fSpeed);
            }
        }
    }

    private void CheckCandySpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            TT_Logic.CandySpawnInfo spawn = m_gameLogic.CheckIfCandyToSpawn();
            if (spawn != null)
            {
                if( ComputeCandyCount() < m_gameData.nCandyMax )
                {
                    SpawnCandy(spawn.candyType, spawn.fX, spawn.fY);
                    //SpawnMeal(spawn.fSpawnerRatioPos, spawn.nItemId, spawn.nColorId);
                }
            }
            else
            {
                bContinue = false;
            }

        }
    }

    private int ComputeCandyCount()
    {
        int nCount = 0;
        for( int i=0; i<m_candysPool.Length; i++ )
        {
            nCount += m_candysPool[i].GetUsedInstanceCount();
        }
        return nCount;
    }

    private void CheckGhostSpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            TT_Logic.GhostSpawnInfo spawn = m_gameLogic.CheckIfGhostToSpawn();
            if (spawn != null)
            {
                TT_Enemy enemy = m_enemyPool[(int)EnemyType.ghosty].GetInstance(m_enemyRoot.transform);
                float fX = spawn.bReverse ? s_gameArea.x + s_gameArea.width : s_gameArea.x;
                Vector3 vDir = new Vector3(spawn.bReverse ? -1 : 1, 0f, 0f);
                enemy.transform.position = new Vector3(fX, spawn.fY, 0f);
                enemy.transform.localScale = new Vector3(m_enemyScale * (spawn.bReverse ? -1f : 1f), m_enemyScale, m_enemyScale);
                enemy.Setup(EnemyType.ghosty, vDir, CanMove, OnEnemyDelete, OnToastyHit, spawn.fSpeed);
            }
            else
            {
                bContinue = false;
            }

        }
    }

    private TT_Candy SpawnCandy( CandyType candyType, float fX, float fY )
    {
        TT_Candy candy = m_candysPool[(int)candyType].GetInstance(m_candyRoot.transform);
        candy.transform.position = new Vector3(fX, fY, 0f);
        candy.transform.localScale = new Vector3(m_candyScale, m_candyScale, m_candyScale);
        candy.Setup(candyType, m_candyDatas[candyType].fShowTime, OnCandyEndTime, OnCandyPickup, OnCandyDelete);
        return candy;
    }

    private void OnCandyEndTime(TT_Candy candy)
    {
        candy.SetOut();
    }

    private void OnCandyDelete( TT_Candy candy )
    {
        int nId = (int)candy.candyType;
        m_candysPool[nId].PoolObject(candy);
    }

    private void OnEnemyDelete(TT_Enemy enemy )
    {
        int nId = (int)enemy.enemyType;
        m_enemyPool[nId].PoolObject(enemy);
    }

    private void OnToastyHit(TT_Enemy enemy, TT_Toasty toasty)
    {
        if (CanMove() && !toasty.IsInvincible())
        {
            int nPoints = m_nBadPointsLost;
            AddPoint(false, toasty.playerId, enemy.transform.position, nPoints);

            enemy.CatchPlayer();
            toasty.GetHit();
        }
    }

    private void OnCandyPickup(TT_Candy candy, TT_Toasty toasty)
    {
        if( CanMove())
        {
            CandyType candyType = candy.candyType;
            if (!string.IsNullOrEmpty(m_candyDatas[candyType].sCandyFx))
            {
                RRSoundManager.instance.PlaySound(m_candyDatas[candyType].sCandyFx); // "event:/GoodMove");
            }

            if (candyType == CandyType.flash)
            {
                KillGhosts();
            }

            int nPoints = (int)(m_candyDatas[candyType].fPointCoeff * m_nGoodPointsWin);
            AddPoint(true, toasty.playerId, candy.transform.position, nPoints);
            toasty.GetCandy();
        }
        candy.SetOut();
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



    private void KillGhosts()
    {
        List<TT_Enemy> ghostList = m_enemyPool[(int)EnemyType.ghosty].UsedInstanceArray;
        for( int i=ghostList.Count-1; i>=0; i-- )
        {
            ghostList[i].Kill();
        }
    }

    #region loading
    private void StartLoading()
    {
        // bkg
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_bkgReference, OnBkgLoad);

        // Candys
        m_candyPrefabs = new CandyPrefab();
        for( int i=0; i<m_candyReference.nLength; i++ )
        {
            CandyType candyType = (CandyType)i;
            RR_AdressableAsset.instance.LoadAsset<GameObject>(m_candyReference[candyType], (AsyncOperationHandle<GameObject> obj) => {
                m_candyPrefabs[candyType] = obj.Result.GetComponent<TT_Candy>();
            });
        }

        // Cursor
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_cursorReference, OnCursorLoad);

        // Ennemy
        m_enemyPrefab = new EnemyPrefab();
        for (int i = 0; i < m_enemyReference.nLength; i++)
        {
            EnemyType enemyType = (EnemyType)i;
            RR_AdressableAsset.instance.LoadAsset<GameObject>(m_enemyReference[enemyType], (AsyncOperationHandle<GameObject> obj) => {
                m_enemyPrefab[enemyType] = obj.Result.GetComponent<TT_Enemy>();
            });
        }
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            Debug.Log("Successfully loaded object.");
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<TT_MainObject>();
            m_loadMask |= (int)TrickOrTreatLoadable.bkg;
        }

    }

    private void OnCursorLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_toastyPrefab = obj.Result.GetComponent<TT_Toasty>();
        }

        m_loadMask |= (int)TrickOrTreatLoadable.cursor;
    }

    void InitAfterLoad()
    {
        float fWidth = m_bkg.m_modelPlaneRect.localScale.x;
        float fHeight = m_bkg.m_modelPlaneRect.localScale.y;
        float fStartX = m_bkg.m_modelPlaneRect.position.x - fWidth / 2;
        float fStartY = m_bkg.m_modelPlaneRect.position.y - fHeight / 2;
        s_gameArea = new Rect(fStartX, fStartY, fWidth, fHeight);

        m_gameLogic = new TT_Logic();
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected);

        GameSettings gameSettings = GameContext.instance.m_settings;
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;

        for ( int i=0; i<m_playerInfos.Length; i++ )
        {
            TT_Toasty toasty = GameObject.Instantiate<TT_Toasty>(m_toastyPrefab, transform);
            toasty.playerId = i;
            Vector3 vPos = toasty.transform.position;
            vPos.x = m_bkg.m_modelPlaneRect.position.x;
            vPos.y = m_bkg.m_modelPlaneRect.position.y;
            vPos.z = TOASTY_Z;
            toasty.transform.position = vPos;
            toasty.transform.localScale = new Vector3(m_playerScale, m_playerScale, m_playerScale);
            toasty.canMove = CanMove;
            toasty.fInvincibleTime = m_fInvincibleTime;

            GKPlayerData playerData = BattleContext.instance.GetPlayer(i);
            toasty.SetAvatar( toasties.GetToasty(playerData.sToastyId).avatar );

            m_playerInfos[i].SetToasty(toasty);
        }

        int nCandyTypeCount = System.Enum.GetNames(typeof(CandyType)).Length;
        m_candysPool = new lwObjectPool<TT_Candy>[nCandyTypeCount];
        m_candyRoot = new GameObject("ItemRoot");

        for (int i = 0; i < nCandyTypeCount; i++)
        {
            m_candysPool[i] = new lwObjectPool<TT_Candy>();
            m_candysPool[i].Init(m_candyPrefabs[i], 10, m_candyRoot.transform);
        }

        int nEnemyTypeCount = System.Enum.GetNames(typeof(EnemyType)).Length;
        m_enemyPool = new lwObjectPool<TT_Enemy>[nEnemyTypeCount];
        m_enemyRoot = new GameObject("EnemyRoot");

        for (int i = 0; i < nEnemyTypeCount; i++)
        {
            m_enemyPool[i] = new lwObjectPool<TT_Enemy>();
            m_enemyPool[i].Init(m_enemyPrefab[i], 10, m_enemyRoot.transform);
        }
        GeneratePermanentEnemy();
    }

    #endregion


    /*
    public override void UpdatePlayerBot(GK_NetworkPlayer player)
    {
        if (m_miniGameState == MiniGameState.playing && m_networkConfig != null)
        {
            if (m_botItemInfoList == null)
            {
                m_botItemInfoList = m_networkConfig.ComputeItemBotInfo( m_currentBoot.percentChanceToTakeFlashCandy, m_currentBoot.timeToTakeCandy, m_candyDatas[CandyType.gold].fShowTime );
            }

            float fTime = Time.time - m_networkConfig.fGameStartTime;
 
            int nItemId = 0;
            while (nItemId < m_botItemInfoList.Count && m_botItemInfoList[nItemId].fSpawnTime < fTime)
            {
                if (fTime > m_botItemInfoList[nItemId].fTakeTime)
                {
                    int nPoints = (int)(m_candyDatas[m_botItemInfoList[nItemId].candyType].fPointCoeff * m_currentBoot.m_nGoodActionPointWin);
                    player.AddLocalCurrentScore((int)(nPoints));

                    m_botItemInfoList.RemoveAt(nItemId);
                }
                else
                {
                    nItemId++;
                }
            }

            if(m_fBotNextEnemyTimer < Time.time )
            {
                if( m_fBotNextEnemyTimer!=-1 )
                {
                    float gameTime = (fTime) / m_gameData.gameTime;
                    float fPercent = m_currentBoot.percentChanceToBeHitByEnemy.Evaluate(gameTime);
                    float f = Random.Range(0f, 1f);
                    if( f <= fPercent )
                    {
                        player.AddLocalCurrentScore(-m_nBadPointsLost);
                    }
                }
                m_fBotNextEnemyTimer = Time.time + Random.Range(m_currentBoot.minTimeBeforeEnemyHit.x, m_currentBoot.minTimeBeforeEnemyHit.y);
            }

        }
    }
    */
}
