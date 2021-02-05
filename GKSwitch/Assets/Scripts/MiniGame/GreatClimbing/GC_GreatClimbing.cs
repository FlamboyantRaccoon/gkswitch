using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC_GreatClimbing : MiniGameTemplate<GC_Logic, GC_GreatClimbing.GreatClimbingData, GC_GreatClimbing.GreatClimbingBotData, MiniGameBasicHud, GC_Balancing>
{
    [SerializeField]
    private static int m_nFoliageProbability = 30;

    public class GripData
    {
        public readonly int nGripId;
        public readonly Vector2 vPos;
        public readonly int nObstacleLife;
        public readonly int nBlocX;
        public readonly int nBlocY;
        public int nHit;
        public bool bAlreadyGenerateNext;

        public GripData(int _nGripId, float _fX, float _fY, int _nObstacleLife, int _nHits, int _nBlocX, int _nBlocY)
        {
            nGripId = _nGripId;
            vPos = new Vector2(_fX, _fY);
            nObstacleLife = _nObstacleLife;
            nHit = _nHits;
            nBlocX = _nBlocX;
            nBlocY = _nBlocY;
            bAlreadyGenerateNext = false;
        }

        public GripData(byte dataMask, float _fX, float _fY, int _nHits, int _nBlocX, int _nBlocY)
        {
            nGripId = dataMask >> 4;
            vPos = new Vector2(_fX, _fY);
            nObstacleLife = dataMask & ((1 << 4) - 1);
            nHit = _nHits;
            nBlocX = _nBlocX;
            nBlocY = _nBlocY;
            bAlreadyGenerateNext = false;
        }

        public GripData(float fGlobalPosX, float fGlobalPosY, byte dataMask)
        {
            nGripId = dataMask >> 4;
            nBlocX = (int)fGlobalPosX;
            nBlocY = (int)fGlobalPosX;
            vPos = new Vector2(fGlobalPosX - nBlocX, fGlobalPosX - nBlocY);
            nObstacleLife = dataMask & ((1 << 4) - 1);
            nHit = 0;
            bAlreadyGenerateNext = false;
        }

        public void Compact(out float fGlobalPosX, out float fGlobalPosY, out byte dataMask)
        {
            fGlobalPosX = nBlocX + vPos.x;
            fGlobalPosY = nBlocY + vPos.y;
            dataMask = (byte)((nGripId << 4) | nObstacleLife);
        }

        public byte GetCompactMask()
        {
            return (byte)((nGripId << 4) | nObstacleLife);
        }
    }

    public class WallEltData
    {
        public readonly int nX;
        public readonly int nY;
        public readonly int wallEltId;

        public readonly bool bHaveFoliage;
        public readonly int foliageRotation;

        public GripData[] gripsArray;


        public WallEltData(int _nX, int _nY, byte[] gripDataMask, float[] gripsX, float[] gripsY)
        {
            nX = _nX;
            nY = _nY;
            wallEltId = (((nX + nY) % 3) == 0 ? 1 : 0);

            gripsArray = new GripData[gripDataMask.Length];
            for (int nGripId = 0; nGripId < gripDataMask.Length; nGripId++)
            {
                gripsArray[nGripId] = new GripData(gripDataMask[nGripId], gripsX[nGripId], gripsY[nGripId], 0, nX, nY);
            }

            int nRnd = Random.Range(0, 100);
            bHaveFoliage = nRnd < GC_GreatClimbing.m_nFoliageProbability;
            foliageRotation = Random.Range(0, 360);
        }
    }

    public class WallTouch
    {
        public GC_Grip m_grip;
        public int m_nFingerId;
        public Vector3 m_vPosition;

        public WallTouch(GC_Grip grip, int nFingerId, Vector3 vPosition)
        {
            m_grip = grip;
            m_nFingerId = nFingerId;
            m_vPosition = vPosition;
        }
    }

    [System.Serializable]
    public class GreatClimbingData : MiniGameBalancingData
    {
        public GreatClimbingDataSequence[] sequencesArray;
    }

    public class PlayerInfo
    {
        public GC_PlayerCanvas canvas;
        public int currentShape = 0;
    }

    [System.Serializable]
    public class GreatClimbingDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float altitudeStart;
        public float distMin;
        public float distMax;
        /// <summary>
        /// 2 method to set angle, linear random with fAngle, with curve with dispersion curve, set fAngle to -1 to use curve
        /// </summary>
        [Tooltip("2 method to set angle, linear random with fAngle, with curve with dispersion curve, set fAngle to -1 to use curve")]
        public float fAngle = 120f;
        /// <summary>
        /// 2 method to set angle, linear random with fAngle, with curve with dispersion curve, set fAngle to -1 to use curve
        /// </summary>
        [Tooltip("2 method to set angle, linear random with fAngle, with curve with dispersion curve, set fAngle to -1 to use curve")]
        public AnimationCurve angleDispersionCurve;
        public bool bAllowNeighborgForChildren = false;
        [lwMinMaxVector(1, 10)]
        public Vector2 nextGripCount = new Vector2(2, 2);
        public AnimationCurve gripSizeProbability;
        public AnimationCurve gripObstacleLifeProbability;
    }

    [System.Serializable]
    public class GreatClimbingBotData : MiniGameBotData
    {
        public AnimationCurve[] altEvolution;
        [lwMinMaxVector(10f, 100f)]
        public Vector2 finalAltitude = new Vector2(15f, 20f);
    }

    private float m_fWallEltSize;
    [Header("GameConfig")]
    [SerializeField]
    internal float m_fFallSpeed = 100f;
    [SerializeField]
    internal float m_fTimeBeforeFall = 100f;
    [SerializeField]
    internal float m_fTimeToReachFallSpeed = 100f;
    internal int m_nMeterPointsWin = 100;
    [SerializeField]
    internal int m_nMeterUnits = 1000;
    [SerializeField]
    private int m_nGeneratedWallOffsetInY = 5;
    [SerializeField]
    private int m_nGeneratedWallOffsetInX = 3;

    [Header("Prefabs")]
    [SerializeField]
    private GC_PlayerCanvas m_playerCanvasPrefab;
    [SerializeField]
    private GC_WallElement m_wallPrefab;
    [SerializeField]
    private GC_Grip m_gripPrefab;
    [SerializeField]
    private GameObject m_wallRoot;
    [SerializeField]
    private GameObject m_wallBkg;

    private lwObjectPool<GC_WallElement> m_wallPool;
    private lwObjectPool<GC_Grip> m_gripPool;
    private Dictionary<string, GC_WallElement> m_wallElementDico;
    private PlayerInfo[] m_playerInfos;

    // Use this for initialization
    void Awake()
    {
        m_wallPool = new lwObjectPool<GC_WallElement>();
        m_wallPool.Init(m_wallPrefab, 100, m_wallRoot.transform);
        m_gripPool = new lwObjectPool<GC_Grip>();
        m_gripPool.Init(m_gripPrefab, 100, m_wallRoot.transform);

        InitPlayerInfos();
        InitGameDataAndBot(MiniGameManager.MiniGames.GreatClimbing);

        m_fWallEltSize = m_wallPrefab.GetEltSize();
        m_wallElementDico = new Dictionary<string, GC_WallElement>();
        
        m_gameLogic = new GC_Logic();
        m_gameLogic.computeAltFunc = ComputeAltFunc;
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected, m_fWallEltSize);
       

        List<WallEltData> dataList = m_gameLogic.GetWallElt(-m_nGeneratedWallOffsetInX, m_nGeneratedWallOffsetInX, 0, m_nGeneratedWallOffsetInY);
        GenerateWall(dataList);

    }

    #region Logic delegate
    public float ComputeAltFunc(int nX, int nY)
    {
        float fX = (nX * m_fWallEltSize) / ((float)m_nMeterUnits);
        float fY = (nY * m_fWallEltSize) / ((float)m_nMeterUnits);

        return Mathf.Max(0f, fY - Mathf.Abs(fX));
    }
    #endregion

    #region override
    public override void Init()
    {
        base.Init();
        /*m_hud = HudManager.instance.GetHud<GreatClimbingHud>(HudManager.GameHudType.miniGame);
        m_hud.UpdateTime(-1);
        m_hud.UpdateAltitude(0);*/
    }

    public override void Clean()
    {
        m_wallBkg.transform.parent = transform;

        if (m_wallElementDico != null)
        {
            foreach (KeyValuePair<string, GC_WallElement> pair in m_wallElementDico)
            {
                GameObject.Destroy(pair.Value.gameObject);
            }
            m_wallElementDico.Clear();
        }

        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            m_playerInfos[i].canvas.Clean();
            GameObject.Destroy(m_playerInfos[i].canvas.gameObject);
        }
        HudManager.sSPLITHUD_COUNT = 1;
        base.Clean();
    }

    protected override bool UpdateGamePlay()
    {
        ManageScroll();

        return CheckAndUpdateTime();
    }

    public override Dictionary<int, int> ComputeStatsDic()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        return dic;
    }

    #endregion

    #region wallGeneration
    private void GenerateWall(List<WallEltData> dataList)
    {
        Vector3 vPos = new Vector3(0f, 0f, 0f);

        for (int nDataIterator = 0; nDataIterator < dataList.Count; nDataIterator++)
        {
            // first check we don't already have the element
            string sKey = dataList[nDataIterator].nX.ToString() + "," + dataList[nDataIterator].nY.ToString();
            if (!m_wallElementDico.ContainsKey(sKey))
            {
                vPos.x = dataList[nDataIterator].nX * m_fWallEltSize;
                vPos.y = (dataList[nDataIterator].nY + 0.5f) * m_fWallEltSize;
                GC_WallElement wallElement = m_wallPool.GetInstance(m_wallRoot.transform);
                wallElement.transform.localPosition = vPos;
                wallElement.Setup(dataList[nDataIterator], m_gripPool, OnGripDlg, OnFlowerHit);
                m_wallElementDico.Add(sKey, wallElement);
            }
        }
    }
    #endregion

    #region inputs

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        GC_PlayerCanvas playerCanvas = m_playerInfos[playerId].canvas;
        playerCanvas.ManageFireInput(v, buttonPhase);
    }


    private void OnGripDlg( int playerId, Vector2 v, GC_Grip grip)
    {
        GC_PlayerCanvas playerCanvas = m_playerInfos[playerId].canvas;
        playerCanvas.OnGrip(v, grip);
    }




    #endregion

    #region scroll and management
   
    private void ManageScroll()
    {
        for( int i=0; i<m_playerInfos.Length; i++ )
        {
            m_playerInfos[i].canvas.ManageScroll();
        }
    }

    internal Vector2 GetWallCoord(Vector3 vWorldPos)
    {
        int nX = (int)(vWorldPos.x > 0 ? vWorldPos.x / m_fWallEltSize : (vWorldPos.x / m_fWallEltSize) - 1);
        int nY = (int)(vWorldPos.y / m_fWallEltSize);
        return new Vector2(nX, nY);
    }

    private void OnFlowerHit( int playerId)
    {
        GC_PlayerCanvas playerCanvas = m_playerInfos[playerId].canvas;
        playerCanvas.OnFlowerHit();
    }

    internal void CheckAndGenerateNewWallElements(Vector2 vOld, Vector2 vNew)
    {
        List<int> coordList = new List<int>();

        if (vOld != vNew)
        {
            string[] oldKeys = new string[m_wallElementDico.Keys.Count];
            m_wallElementDico.Keys.CopyTo(oldKeys, 0);
            List<string> sOldKey = new List<string>(oldKeys);


            for (int nX = (int)(vNew.x - m_nGeneratedWallOffsetInX); nX <= (int)(vNew.x + m_nGeneratedWallOffsetInX); nX++)
            {
                int nStartY = Mathf.Max(0, (int)(vNew.y - m_nGeneratedWallOffsetInY));
                for (int nY = nStartY; nY <= (int)(vNew.y + m_nGeneratedWallOffsetInY); nY++)
                {
                    string sKey = nX.ToString() + "," + nY.ToString();
                    if (!m_wallElementDico.ContainsKey(sKey))
                    {
                        coordList.Add(nX);
                        coordList.Add(nY);
                    }
                    else
                    {
                        sOldKey.Remove(sKey);
                    }
                }
            }
            if (coordList.Count > 0)
            {
                List<WallEltData> dataList = m_gameLogic.GetWallElt(coordList.ToArray());
                GenerateWall(dataList);
            }

            // search WallElt to release
            /*if (sOldKey.Count > 0)
            {
                for (int nDeleteId = 0; nDeleteId < sOldKey.Count; nDeleteId++)
                {
                    GC_WallElement wallElement = null;
                    if (m_wallElementDico.TryGetValue(sOldKey[nDeleteId], out wallElement))
                    {
                        wallElement.Clean();
                        m_wallPool.PoolObject(wallElement);
                        m_wallElementDico.Remove(sOldKey[nDeleteId]);
                    }
                }
            }*/
        }
    }

    //List<WallEltData> dataList = m_networkConfig.GetWallElt(-m_nGeneratedWallOffset, m_nGeneratedWallOffset, 0, m_nGeneratedWallOffset);
    //        GenerateWall(dataList);
    #endregion

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new PlayerInfo[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            m_playerInfos[i] = new PlayerInfo();
            m_playerInfos[i].canvas = GameObject.Instantiate<GC_PlayerCanvas>(m_playerCanvasPrefab);
            m_playerInfos[i].canvas.Init(i, this);
            m_playerInfos[i].canvas.SetCameraRegion(playerCount);
        }
        HudManager.sSPLITHUD_COUNT = playerCount;
        HudManager.sSPLITHUD_TYPE = HudManager.SplitHudType.quarter;
    }
}