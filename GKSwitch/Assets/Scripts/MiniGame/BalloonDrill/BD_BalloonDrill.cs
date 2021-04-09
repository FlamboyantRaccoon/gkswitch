using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BD_BalloonDrill : MiniGameTemplate<BD_Logic, BD_BalloonDrill.BalloonDrillData, BD_BalloonDrill.BalloonDrillBotData, BalloonDrillHud, BD_Balancing>
{
    public enum DisruptiveElt { random, wind, count };

    [System.Serializable]
    public class BalloonDrillData : MiniGameBalancingData
    {
        public BalloonDrillDataSequence[] sequencesArray;
    }

    [System.Serializable]
    public class BalloonDrillDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float startTime;
        public int colorCount = 4;
        public int balloonBySecond;
        public int goodColorPercent;
        [lwMinMaxVector(1, 1000)]
        public Vector2 speed = new Vector2(10, 100);
        public DisruptiveElt[] disruptiveElements;
        public Vector2 disruptiveElementSpeed = new Vector2(10, 100);

        public int GetDisruptiveMask()
        {
            int xMask = 0;
            for (int i = 0; i < (int)disruptiveElements.Length; i++)
            {
                xMask |= 1 << (int)disruptiveElements[i];
            }
            return xMask;
        }
    }

    [System.Serializable]
    public class BalloonDrillBotData : MiniGameBotData
    {
        public AnimationCurve clickBySecond;
        public AnimationCurve goodClickPercent;
    }

    public class BalloonSpawn
    {
        public float fSpawnerRatioPos;
        public float fSpeed;
        public int nColorId;
        public float fSpawnTime;
        public float fDepth;

        public BalloonSpawn(float _fSpawnerRatioPos, float _fSpeed, int _nColorId, float _fSpawnTime, float _fDepth)
        {
            fSpawnerRatioPos = _fSpawnerRatioPos;
            fSpeed = _fSpeed;
            nColorId = _nColorId;
            fSpawnTime = _fSpawnTime;
            fDepth = _fDepth;
        }
    }

    [Header("Game Config")]
    [SerializeField]
    private int m_nBadBalloonPointsLost = 50;

    [Header("Prefabs")]
    [SerializeField]
    BD_Balloon m_balloonPrefab;
    [SerializeField]
    BD_BalloonSpawner[] m_balloonSpawner;

    [SerializeField]
    private Animator m_cloudAnimator;
    [SerializeField]
    private WindFx m_windFx;

    private lwObjectPool<BD_Balloon> m_balloonPool;
    private GameObject m_balloonRoot;
    private BD_BalloonSpawner m_selectedSpawner;
    private float m_fCloudTime = 0f;
    private Vector2 m_vMoveModificator = Vector2.zero;

    private int[] m_ballonsDrillsStats;

    protected void Awake()
    {
        m_balloonPool = new lwObjectPool<BD_Balloon>();
        m_balloonRoot = new GameObject("BalloonRoot");
        m_balloonPool.Init(m_balloonPrefab, 100, m_balloonRoot.transform);
        m_selectedSpawner = m_balloonSpawner[Random.Range(0, m_balloonSpawner.Length)];
        m_selectedSpawner.Init(GetSpeedMultiplier, GetMoveModificator, m_balloonPool, DrillBalloon, CanDestroyBalloon);

        InitGameDataAndBot(MiniGameManager.MiniGames.BalloonDrill);

        m_gameLogic = new BD_Logic();
        m_gameLogic.Init(m_balloonSpawner.Length, m_nMiniGameDataSelected, m_gameData);
        m_gameLogic.onObjectiveChangeDlg = SetObjective;
        m_gameLogic.onDisruptiveElementAppear = OnDisruptiveElementAppear;
        m_gameLogic.onDisruptiveElementDisAppear = OnDisruptiveElementDisappear;
        m_gameLogic.onDisruptiveElementAlterate = OnDisruptiveElementAlterate;

        m_ballonsDrillsStats = new int[2] { 0, 0 };
    }

    public override void Init()
    {
        base.Init();
        SetObjective(-1);
    }

    protected override bool UpdateInit()
    {
        bool isInit = base.UpdateInit();
        if (isInit )
        {
            m_gameLogic.SetGameRefill();
        }
        return isInit;
    }

    protected override void StartGame()
    {
        base.StartGame();
        m_gameLogic.SelectObjective();
    }

    public override void Clean()
    {
        m_balloonPool.Destroy();
        GameObject.Destroy(m_balloonRoot);
        base.Clean();
    }

    private float GetSpeedMultiplier()
    {
        return 1f;
    }

    private Vector2 GetMoveModificator()
    {
        return m_vMoveModificator;
    }

    private void SetObjective(int nObjectiveId)
    {
        m_hud.SetObjective(nObjectiveId >= 0 ? m_balloonPrefab.m_spritesArray[nObjectiveId] : null);
    }



    protected override void UpdateWarmUp()
    {
        CheckSpawn();
        CheckCloud();
    }

    protected override bool UpdateGamePlay()
    {
        CheckSpawn();
        CheckCloud();

        return CheckAndUpdateTime();
    }

    public override Dictionary<int, int> ComputeStatsDic()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        dic.Add(0, m_ballonsDrillsStats[0]);
        dic.Add(1, m_ballonsDrillsStats[1]);
        return dic;
    }

    private void CheckSpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            BalloonSpawn spawn = m_gameLogic.CheckIfBalloonToSpawn();
            if (spawn != null)
            {
                m_selectedSpawner.SpawnBalloon(spawn.fSpawnerRatioPos, spawn.fSpeed, spawn.nColorId, spawn.fDepth);
            }
            else
            {
                bContinue = false;
            }

        }
    }

    private bool DrillBalloon(int nColorId,int playerId, Vector3 vBalloonPos)
    {
        bool bGood = false;
        if (nColorId == (m_gameLogic.objectiveId))
        {
            BattleContext.instance.AddPoint(m_nGoodPointsWin, playerId);
            HudManager.instance.SpawnWinScore(vBalloonPos, m_nGoodPointsWin, playerId);
            bGood = true;
        }
        else
        {
            BattleContext.instance.AddPoint(-m_nBadBalloonPointsLost, playerId);
            HudManager.instance.SpawnLoseScore(vBalloonPos, -m_nBadBalloonPointsLost, playerId);
            bGood = false;
        }
        m_ballonsDrillsStats[bGood ? 0 : 1]++;
        return bGood;
    }

    private bool CanDestroyBalloon()
    {
        return m_miniGameState == MiniGameState.playing;
    }

    private void OnDisruptiveElementAppear(int xMask, Vector2 vSpeed)
    {
        for (int nEltId = 0; nEltId < (int)DisruptiveElt.count; nEltId++)
        {
            if ((xMask & (1 << nEltId)) != 0)
            {
                DisruptiveElt elt = (DisruptiveElt)nEltId;
                switch (elt)
                {
                    case DisruptiveElt.wind:
                        m_windFx.SetWind(vSpeed);
                        m_vMoveModificator = vSpeed;
                        break;
                }
            }
        }
    }

    private void OnDisruptiveElementAlterate(int xMask, Vector2 vSpeed)
    {
        for (int nEltId = 0; nEltId < (int)DisruptiveElt.count; nEltId++)
        {
            if ((xMask & (1 << nEltId)) != 0)
            {
                DisruptiveElt elt = (DisruptiveElt)nEltId;
                switch (elt)
                {
                    case DisruptiveElt.wind:
                        m_windFx.StopWind();
                        m_windFx.SetWind(vSpeed);
                        m_vMoveModificator = vSpeed;
                        break;
                }
            }
        }
    }

    private void OnDisruptiveElementDisappear(int xMask)
    {
        for (int nEltId = 0; nEltId < (int)DisruptiveElt.count; nEltId++)
        {
            if ((xMask & (1 << nEltId)) != 0)
            {
                DisruptiveElt elt = (DisruptiveElt)nEltId;
                switch (elt)
                {
                    case DisruptiveElt.wind:
                        m_windFx.StopWind();
                        m_vMoveModificator = Vector2.zero;
                        break;
                }
            }
        }
    }

    private void CheckCloud()
    {
        if (Time.time > m_fCloudTime)
        {
            m_fCloudTime += 5f;
            if (m_cloudAnimator != null)
            {
                int nRnd = Random.Range(1, 5);
                string sAnim = "Clouds" + nRnd;
                m_cloudAnimator.SetTrigger(sAnim);
            }
        }
    }

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if( buttonPhase!= RRPlayerInput.ButtonPhase.press )
        {
            return;
        }
        Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(v.x, v.y, 0));

        //GameObject spere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //spere.transform.position = rayOrigin;
        Ray ray = new Ray(rayOrigin, Vector3.forward);
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray); // Camera.main.ScreenPointToRay(v));

        if (rayHit.transform!=null)
        {
            BD_Balloon balloon = rayHit.transform.GetComponent<BD_Balloon>();
            if( balloon!=null )
            {
                balloon.OnShoot( playerId );
            }
        }
    }
}
