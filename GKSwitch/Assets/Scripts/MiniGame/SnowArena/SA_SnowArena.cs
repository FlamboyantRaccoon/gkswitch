using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SA_SnowArena : MiniGameTemplate<SA_Logic, SA_SnowArena.snowData, SA_SnowArena.snowBotData, MiniGameBasicHud, SA_Balancing>
{
    public enum ObstacleType { nomobile, mobile }
    public enum SnowLoadable { bkg = 1, ball = 2, obstacle = 4, target=8 }

    [System.Serializable] public class ObstaclePrefab : lwEnumArray<ObstacleType, SA_Obstacle> { }; // dummy definition to use Unity serialization
    [System.Serializable] public class ObstacleReferences : lwEnumArray<ObstacleType, AssetReference> { }; // dummy definition to use Unity serialization

    //    event:/Picking/PickingCherryGoodPerfect
    [System.Serializable]
    public class snowData : MiniGameBalancingData
    {
        public SA_Grid grid;
        [lwMinMaxVector(0, 10, true)]
        public Vector2 nomobileObstacleRange;
        [lwMinMaxVector(0, 10, true)]
        public Vector2 mobileObstacleRange;
        [lwMinMaxVector(10f, 1000f)]
        public Vector2 mobileSpeed;
        public int nTargetMax = 5;
        public SnowArenaDataSequence[] sequencesArray;

        public SnowArenaDataSequence GetSequence(float fTime)
        {
            int nSequenceId = 0;
            while (nSequenceId < sequencesArray.Length && sequencesArray[nSequenceId].startTime > fTime)
            {
                nSequenceId++;
            }
            return sequencesArray[nSequenceId];
        }
    }

    [System.Serializable]
    public class SnowArenaDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float startTime;
        [Header("Target")]
        public float itemsBySecond = 1f;
        public float goldTargetChance = 0.5f;
    }

    [System.Serializable]
    public class snowBotData : MiniGameBotData
    {
        public AnimationCurve targetBySecond;
        [Tooltip("to add noise on next target, one half more, one half less")]
        public AnimationCurve targetBySecondRandomDelta;
        [Tooltip("goldPercent would be multiply by sequence gold percent")]
        public AnimationCurve goldTargetPercent;
    }

    [Header("GameConfig")]
    [SerializeField]
    private float m_fGoldTargetTime = 3f;
    [SerializeField]
    private float m_playerScale = 1f;
    [SerializeField]
    private float m_obstacleScale = 1f;
    [SerializeField]
    private float m_mobileObstacleScale = 1f;
    [SerializeField]
    private float m_targetScale = 1f;
    [Header("SnowBall Speed")]
    [SerializeField]
    private float m_snowBallSpeedMultiplier = 2f;
    [SerializeField]
    private float m_snowBallMinSpeed = 200f;

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;
    [SerializeField]
    private AssetReference m_snowballReference;
    [SerializeField]
    private AssetReference m_targetReference;
    [SerializeField]
    private ObstacleReferences m_obstacleReference;


    private SA_Snowball m_snowballPrefab;
    private SA_Target m_targetPrefab;
    private ObstaclePrefab m_obstaclePrefab;

    private lwObjectPool<SA_Snowball> m_snowballPool;
    private lwObjectPool<SA_Obstacle>[] m_obstaclePool;
    private lwObjectPool<SA_Target> m_targetPool;
    private GameObject m_obstacleRoot;
    private GameObject m_ballsRoot;
    private GameObject m_targetRoot;

    private SA_PlayerInfos[] m_playerInfos;

    List<int> m_nAvailableTargetCells;
    List<int> m_nUsedTargetCells;

    public static Rect s_LaunchArea;
    public static Rect s_gameArea;
    public static float s_snowBallMinSpeed;

    private SA_MainObject m_bkg;
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(SnowLoadable)).Length)) - 1;

    public static bool IsPointOutGameArea(Vector3 vPoint, float fColRadius, ref Vector3 vDir, ref Vector3 vColisionPoint)
    {
        Vector2 vInter = Vector2.zero;
        Vector2 vA = (Vector2)(vPoint);
        Vector2 vB = (Vector2)(vPoint + vDir) + (Vector2)(vDir.normalized * fColRadius);

        // test left
        if (vB.x < s_gameArea.x && vDir.x < 0)
        {
            vColisionPoint = GK_Tools.ComputeVectorWithSegmentAndAbcisse(vA, vB, s_gameArea.x) - (vDir.normalized * fColRadius);
            vDir.x = -vDir.x;
            return true;
        }

        // test right
        if (vB.x > s_gameArea.x + s_gameArea.width && vDir.x > 0)
        {
            vColisionPoint = GK_Tools.ComputeVectorWithSegmentAndAbcisse(vA, vB, s_gameArea.x + s_gameArea.width) - (vDir.normalized * fColRadius);
            vDir.x = -vDir.x;
            return true;
        }
        
        return false;
    }


    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.SnowArena);
        InitPlayerInfos();
    }

    protected override bool UpdateInit()
    {
        if ((m_loadMask & (int)SnowLoadable.obstacle) == 0)
        {
            bool complete = true;
            for (int i = 0; i < m_obstaclePrefab.nLength; i++)
            {
                if (m_obstaclePrefab[i] == null)
                {
                    complete = false;
                }
            }

            if (complete)
            {
                m_loadMask |= (int)SnowLoadable.obstacle;
            }
        }

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

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if (m_playerInfos != null)
        {
            SA_PlayerInfos player = m_playerInfos[playerId];
            player.ManageFireInput(v, buttonPhase);
        }
    }

    public override void Clean()
    {
        for (int i = 0; i < m_obstaclePool.Length; i++)
        {
            m_obstaclePool[i].Destroy();
        }

        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            m_playerInfos[i].Clean();
        }

        m_snowballPool.Destroy();
        m_targetPool.Destroy();
        GameObject.Destroy(m_ballsRoot);
        GameObject.Destroy(m_obstacleRoot);
        GameObject.Destroy(m_targetRoot);

        AimingHud aimingHud = HudManager.instance.GetForeHud<AimingHud>(HudManager.ForeHudType.aimingHud);
        aimingHud.ResetSpecificZone();

        GameObject.Destroy(m_bkg.gameObject);
        base.Clean();
    }

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new SA_PlayerInfos[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            m_playerInfos[i] = new SA_PlayerInfos();
            m_playerInfos[i].Setup(i);
        }
    }

    void InitAfterLoad()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.SnowArena);

        float fWidth = m_bkg.m_modelPlaneRect.localScale.x;
        float fHeight = m_bkg.m_modelPlaneRect.localScale.y;
        float fStartX = m_bkg.m_modelPlaneRect.position.x - fWidth / 2;
        float fStartY = m_bkg.m_modelPlaneRect.position.y - fHeight / 2;
        s_LaunchArea = new Rect(fStartX, fStartY, fWidth, fHeight);

        fWidth = m_bkg.m_gamePlaneRect.localScale.x;
        fHeight = m_bkg.m_gamePlaneRect.localScale.y;
        fStartX = m_bkg.m_gamePlaneRect.position.x - fWidth / 2;
        fStartY = m_bkg.m_gamePlaneRect.position.y + fHeight / 2;
        s_gameArea = new Rect(fStartX, fStartY, fWidth, fHeight);
        //m_fBootNextActionTimer = -1f;
        s_snowBallMinSpeed = m_snowBallMinSpeed;

        m_ballsRoot = new GameObject("BallsRoot");
        m_snowballPool = new lwObjectPool<SA_Snowball>();
        m_snowballPool.Init(m_snowballPrefab, 10, m_ballsRoot.transform);

        m_targetRoot = new GameObject("TargetRoot");
        m_targetPool = new lwObjectPool<SA_Target>();
        m_targetPool.Init(m_targetPrefab, 10, m_targetRoot.transform);

        int nObstacleTypeCount = System.Enum.GetNames(typeof(ObstacleType)).Length;
        m_obstaclePool = new lwObjectPool<SA_Obstacle>[nObstacleTypeCount];
        m_obstacleRoot = new GameObject("ObstacleRoot");

        m_nAvailableTargetCells = new List<int>();
        m_nUsedTargetCells = new List<int>();

        for (int i = 0; i < nObstacleTypeCount; i++)
        {
            m_obstaclePool[i] = new lwObjectPool<SA_Obstacle>();
            m_obstaclePool[i].Init(m_obstaclePrefab[i], 10, m_obstacleRoot.transform);
        }

        m_gameLogic = new SA_Logic();
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected);

        float startX = s_LaunchArea.x;
        float width = s_LaunchArea.width / m_playerInfos.Length;

        AimingHud aimingHud = HudManager.instance.GetForeHud<AimingHud>(HudManager.ForeHudType.aimingHud);
        aimingHud.InitSpecificZone(m_playerInfos.Length);

        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            m_playerInfos[i].m_zoneRect = new Rect(startX + i * width, s_LaunchArea.y, width, s_LaunchArea.height);
            aimingHud.SetSpecificZone(i, m_playerInfos[i].m_zoneRect);
            SpawnSnowBall(i);
        }


        m_nAvailableTargetCells = m_gameLogic.GetGridCellsWithValue(0);
        GenerateObstacles();
    }

    protected override void UpdateWarmUp()
    {
        CheckTargetSpawn();
    }

    protected override bool UpdateGamePlay()
    {
        CheckTargetSpawn();
        return CheckAndUpdateTime();
    }

    private bool CanMove()
    {
        return m_miniGameState == MiniGameState.playing;
    }

    private SA_Snowball SpawnSnowBall( int playerId )
    {
        SA_Snowball ball = m_snowballPool.GetInstance(m_ballsRoot.transform);
        float fX = (s_LaunchArea.xMin + s_LaunchArea.xMax) / 2f;
        float fY = (s_LaunchArea.yMin + s_LaunchArea.yMax) / 2f;

        ball.transform.position = new Vector3(fX, fY, 0f);
        ball.transform.localScale = new Vector3(m_playerScale, m_playerScale, m_playerScale);
        ball.Setup(playerId, CanMove, IsInLaunchArea, OnSnowBallThrown, OnBallDelete, m_snowBallSpeedMultiplier);

        m_playerInfos[playerId].SetBall(ball);

        return ball;
    }

    private SA_Target SpawnTarget(int nCellId, bool bGold)
    {
        if (m_nUsedTargetCells.Contains(nCellId))
        {
            if (m_nAvailableTargetCells.Count == 0)
            {
                return null;
            }
            nCellId = m_nAvailableTargetCells[Random.Range(0, m_nAvailableTargetCells.Count)];
        }
        m_nAvailableTargetCells.Remove(nCellId);
        m_nUsedTargetCells.Add(nCellId);


        int nGridColumnCount = m_gameData.grid.nCols;
        int nGridRowCount = m_gameData.grid.nRows;
        float fColumnSize = SA_SnowArena.s_gameArea.width / (float)nGridColumnCount;
        float fRowSize = SA_SnowArena.s_gameArea.height / (float)nGridRowCount;

        int nX = nCellId % nGridColumnCount;
        int nY = (int)((nCellId) / nGridColumnCount);
        float fX = (nX + 0.5f) * fColumnSize + SA_SnowArena.s_gameArea.x;
        float fY = SA_SnowArena.s_gameArea.y - ((nY + 0.5f) * fRowSize);

        SA_Target target = m_targetPool.GetInstance(m_targetRoot.transform);
        float fZ = -(s_gameArea.y - fY) * 10f / s_gameArea.height;
        target.transform.position = new Vector3(fX, fY, fZ);
        target.transform.localScale = new Vector3(m_targetScale, m_targetScale, m_targetScale);
        target.Setup(OnTargetEndTime, nCellId, bGold, bGold ? m_fGoldTargetTime : -1f, OnTargetHit, OnTargetDelete);
        return target;
    }

    private void OnTargetDelete(SA_Target target)
    {
        m_nUsedTargetCells.Remove(target.m_nCellId);
        m_nAvailableTargetCells.Add(target.m_nCellId);
        m_targetPool.PoolObject(target);
    }

    private void OnBallDelete(SA_Snowball snowball)
    {
        m_snowballPool.PoolObject(snowball);
    }

    private void OnTargetHit(SA_Target target, SA_Snowball ball)
    {
        if (CanMove() && target != null)
        {
            int nPoints = (int)(m_nGoodPointsWin);
            if (target.m_bGold)
            {
                nPoints *= 2;
                if (ball != null)
                {
                    ball.AddTargetTouch();
                }
                SoundManager.instance.PlaySound("gk_game_expression_good_05");
                //FMODUnity.RuntimeManager.PlayOneShot("event:/Frozen/GoldTarget"); // "event:/GoodMove");
            }
            else
            {
                int nTouch = ball != null ? ball.AddTargetTouch() : 0;
                string sSound = "Target" + (Mathf.Min(3, nTouch - 1)).ToString();
                //                Debug.Log("sSound :  " + sSound);
                SoundManager.instance.PlaySound("gk_game_expression_good_04");
                //FMODUnity.RuntimeManager.PlayOneShot("event:/Frozen/" + sSound); // "event:/GoodMove");
            }
            Vector3 vPos = target.transform.position;

            if (target != null)
            {
                target.Hit();
            }
            AddPoint(true, ball.playerId, vPos, nPoints);
        }
    }

    private void OnSnowBallThrown( int playerId )
    {
        SpawnSnowBall( playerId );
    }

    private bool IsInLaunchArea(Vector3 vPos)
    {
        return vPos.x >= s_LaunchArea.xMin &&
            vPos.x <= s_LaunchArea.xMax &&
            vPos.y >= s_LaunchArea.yMin &&
            vPos.y <= s_LaunchArea.yMax;
    }

    private void GenerateObstacles()
    {
        List<SA_Logic.ObstacleSpawnInfo> noMobileSpawnInfos = m_gameLogic.noMobileSpawns;
        if (noMobileSpawnInfos != null)
        {
            for (int i = 0; i < noMobileSpawnInfos.Count; i++)
            {
                SA_Logic.ObstacleSpawnInfo info = noMobileSpawnInfos[i];
                SA_Obstacle obstacle = m_obstaclePool[(int)ObstacleType.nomobile].GetInstance(m_obstacleRoot.transform);
                float fZ = -(s_gameArea.y - info.fY) * 10f / s_gameArea.height;
                obstacle.transform.position = new Vector3(info.fX, info.fY, fZ);
                obstacle.transform.localScale = new Vector3(m_obstacleScale, m_obstacleScale, m_obstacleScale);
                obstacle.Setup(info.obstacleType, CanMove, OnObstacleHit);
            }
        }

        List<SA_Logic.MobileObstacleSpawnInfo> mobileObstacles = m_gameLogic.mobileSpawns;
        if (mobileObstacles != null)
        {
            for (int i = 0; i < mobileObstacles.Count; i++)
            {
                SA_Logic.MobileObstacleSpawnInfo info = mobileObstacles[i];
                SA_MobileObstacle mobileObstacle = (SA_MobileObstacle)m_obstaclePool[(int)ObstacleType.mobile].GetInstance(m_obstacleRoot.transform);
                Vector2[] path = m_gameLogic.ComputePathFromCells(info.path);
                float fZ = -(s_gameArea.y - path[info.nStartCell].y) * 10f / s_gameArea.height;
                mobileObstacle.transform.position = new Vector3(path[info.nStartCell].x, path[info.nStartCell].y, fZ);
                mobileObstacle.transform.localScale = new Vector3(m_mobileObstacleScale, m_mobileObstacleScale, m_mobileObstacleScale);
                mobileObstacle.Setup(ObstacleType.mobile, CanMove, OnObstacleHit, path, info.nStartCell, info.bReverse, info.fSpeed);
            }
        }


    }

    private void OnObstacleHit(SA_Obstacle obstacle, SA_Snowball ball)
    {
        if (obstacle.obstacleType == ObstacleType.mobile)
        {
            //FMODUnity.RuntimeManager.PlayOneShot("event:/Frozen/HitSnowman");
            SoundManager.instance.PlaySound("gk_game_trickortreat_pumpkinhit");
            m_gameStats[1]++;
        }

        if (ball != null)
        {
            ball.Hit(false);
        }
    }

    private void CheckTargetSpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            SA_Logic.TargetSpawnInfo spawn = m_gameLogic.CheckIfTargetToSpawn();
            if (spawn != null)
            {
                if (ComputeTargetCount() < m_gameData.nTargetMax)
                {
                    SpawnTarget(spawn.nCellId, spawn.bGold);
                    //SpawnMeal(spawn.fSpawnerRatioPos, spawn.nItemId, spawn.nColorId);
                }
            }
            else
            {
                bContinue = false;
            }

        }
    }

    private void OnTargetEndTime(SA_Target target)
    {
        target.GetOut();
    }

    private int ComputeTargetCount()
    {
        return m_targetPool.GetUsedInstanceCount();
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


    #region loading
    private void StartLoading()
    {
        // bkg
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_bkgReference, OnBkgLoad);

        // Obstacles
        m_obstaclePrefab = new ObstaclePrefab();
        for (int i = 0; i < m_obstacleReference.nLength; i++)
        {
            ObstacleType obstacleType = (ObstacleType)i;
            RR_AdressableAsset.instance.LoadAsset<GameObject>(m_obstacleReference[obstacleType], (AsyncOperationHandle<GameObject> obj) => {
                m_obstaclePrefab[obstacleType] = obj.Result.GetComponent<SA_Obstacle>();
            });
        }

        // Ball
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_snowballReference, OnBallLoad);

        // Target
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_targetReference, OnTargetLoad);
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<SA_MainObject>();
            m_loadMask |= (int)SnowLoadable.bkg;
        }

    }

    private void OnBallLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_snowballPrefab = obj.Result.GetComponent<SA_Snowball>();
        }

        m_loadMask |= (int)SnowLoadable.ball;
    }

    private void OnTargetLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_targetPrefab = obj.Result.GetComponent<SA_Target>();
        }

        m_loadMask |= (int)SnowLoadable.target;
    }

    #endregion
}
