using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DS_DrawShape : MiniGameTemplate<DS_Logic, DS_DrawShape.DrawShapeData, DS_DrawShape.DrawShapeBotData, DrawShapeHud, DS_Balancing>
{
    public enum ModelShowState { visible, fade, hide }

    [System.Serializable]
    public class DrawShapeData : MiniGameBalancingData
    {
        [lwMinMaxVector(3, 10, true)]
        public Vector2 pointsCount = new Vector2(4, 4);
        public float fShapeWidth = 10f;
        public float fModelFadeOutTime = 5f;
        public int nGridColumnCount = 3;
        public int nGridRowCount = 4;
    }

    [System.Serializable]
    public class DrawShapeBotData : MiniGameBotData
    {
        public AnimationCurve[] pointsEvolution;
        [lwMinMaxVector(1000f, 50000f)]
        public Vector2 finalScore = new Vector2(5000f, 15000f);
    }

    public class PlayerInfo
    {
        public DS_PlayerCanvas canvas;
        public int currentShape = 0;
    }

    [Header("Game Config")]
    [SerializeField]
    private int m_nBadDrawPointsLost = 15;
    [SerializeField]
    public bool m_bFadeModel = false;
    [Tooltip("les couturiere disent la finesse de ton pas")]
    [SerializeField]
    public float m_fMeshHeight = 10f;
    [SerializeField]
    public float m_fMaterialUvoffset = 0.5f;
    [SerializeField]
    private Color[] m_drawColors;
    [SerializeField]
    public float m_fNeedleLatencyAnimTime = 0.3f;
    [SerializeField]
    private float m_fDrawingAreaRange = 1390f;

    [SerializeField]
    private float m_fTimeBeforeNextButtonAppear = 3f;



    [Header("Prefabs")]
    [SerializeField]
    private DS_PlayerCanvas m_playerCanvasPrefab;
    [SerializeField]
    public Transform m_modelPlaneRect;

    [SerializeField]
    public string m_sPickingSound;

    private int[] m_drawShapeStats;
    
    private lwLinesMesh m_modelShape;
    private lwLinesMesh m_playerShape;
    private Vector3 m_vLastInputPos;
    ModelShowState m_modelShowState = ModelShowState.visible;
    private int m_nPlayerInMeshPoint;
    private float m_fNeedleLastAnimTime = -1f;
    private float m_fAreaRangeSqrt;
    private float m_fStartDrawTimer = -1f;
    private bool m_bIsDrawStart = false;
    private Coroutine m_modelFadeOutRoutine;

    private string m_pickingSound;
    //private FMOD.Studio.ParameterInstance m_pickingSoundGood;

    private Vector3 m_vAreaCenter = new Vector3(0f, 857f, 10f);
    private PlayerInfo[] m_playerInfos;

    /// <summary>
    /// Boot datas
    /// </summary>
    private float m_fBootNextActionTimer = -1f;
    private float m_fBootFinalScore;
    private int m_nBootAnimCurveSelected;
    
    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.DrawShape);
        InitPlayerInfos();

        m_gameLogic = new DS_Logic();
        Rect rect = new Rect(m_modelPlaneRect.transform.position.x, m_modelPlaneRect.transform.position.y, m_modelPlaneRect.transform.localScale.x, m_modelPlaneRect.transform.localScale.y);
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected, rect);
        SetupLogicDelegate();

        //NetworkServer.Spawn(m_networkConfig);
        m_drawShapeStats = new int[2] { 0, 0 };

        /*m_pickingSound = FMODUnity.RuntimeManager.CreateInstance(m_sPickingSound);
        float fValue = 0f;
        if(m_pickingSound.getParameterByName( "bGood", out fValue ) != FMOD.RESULT.OK )
        {
            Debug.LogError("bGood parameter not found in drawShapeSound");
        }*/
    }

    public override void Init()
    {
        base.Init();
       /* m_hud = HudManager.instance.GetHud<DrawShapeHud>(HudManager.GameHudType.miniGame);
        m_hud.SetButtonInteractive(false);
        m_hud.onNextShapeAction = OnNextShape;
        m_hud.UpdateTime(-1);*/
    }

    protected override bool UpdateInit()
    {
        bool isInit = base.UpdateInit();
        if (isInit)
        {
            m_gameLogic.GenerateFirstShape();

            for( int i=0; i<m_playerInfos.Length;i++ )
            {
                m_playerInfos[i].canvas.GenerateModelShape(m_gameLogic.GetShape(0));
            }


        }
        return isInit;
    }

    public override void Clean()
    {
        /*if (m_pickingSound.isValid())
        {
            m_pickingSound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            m_pickingSound.release();
        }*/
        for( int i=0; i<m_playerInfos.Length; i++ )
        {
            m_playerInfos[i].canvas.Clean();
            GameObject.Destroy(m_playerInfos[i].canvas.gameObject);
        }
        HudManager.sSPLITHUD_COUNT = 1;

        base.Clean();
    }

   

    public override Dictionary<int, int> ComputeStatsDic()
    {
        Dictionary<int, int> dic = new Dictionary<int, int>();
        dic.Add(0, m_drawShapeStats[0]);
        dic.Add(1, m_drawShapeStats[1]);
        return dic;
    }


    protected override void UpdateWarmUp()
    {
       
    }

    protected override bool UpdateGamePlay()
    {
        //UpdateInput();

        if( m_bIsDrawStart && m_fStartDrawTimer > 0 )
        {
            if( Time.realtimeSinceStartup > m_fStartDrawTimer + m_fTimeBeforeNextButtonAppear )
            {
                m_fStartDrawTimer = -1f;
                //m_hud.SetButtonInteractive(true);
            }
        }

        return CheckAndUpdateTime();
    }

    protected override void StopPlaying()
    {
        for( int i=0; i<m_playerInfos.Length; i++ )
        {
            m_playerInfos[i].canvas.StopPlaying();
        }
    }

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        DS_PlayerCanvas playerCanvas = m_playerInfos[playerId].canvas;
        playerCanvas.ManageFireInput(v, buttonPhase);
    }

    protected override bool MiniGameActionInput(int playerId, RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch( inputActionType )
        {
            case RRInputManager.InputActionType.ButtonRight:
                {
                    DS_PlayerCanvas playerCanvas = m_playerInfos[playerId].canvas;
                    playerCanvas.OnNextShape();
                    m_playerInfos[playerId].currentShape++;
                    playerCanvas.GenerateModelShape(m_gameLogic.GetShape(m_playerInfos[playerId].currentShape));
                }
                break;
        }
        return true;
    }

    private void AddScore( bool bGood, int playerId, Vector3 vPos)
    {
        if (bGood)
        {
            BattleContext.instance.AddPoint(m_nGoodPointsWin, playerId);
            HudManager.instance.SpawnWinScore(vPos, m_nGoodPointsWin, playerId);
        }
        else
        {
            BattleContext.instance.AddPoint(-m_nBadDrawPointsLost, playerId);
            HudManager.instance.SpawnLoseScore(vPos, -m_nBadDrawPointsLost, playerId);
        }
    }

    protected override void SetupLogicDelegate()
    { 
    }



    /*
    private void OnNextShape()
    {
        //Debug.Log("OnNextShape");
        if (m_modelFadeOutRoutine != null)
        {
            StopCoroutine(m_modelFadeOutRoutine);
        }

       // m_hud.SetButtonInteractive(false);
        
        Invoke("GenerateNextShape", 0.1f);
 
    }

    public void GenerateNextShape()
    {
        Vector2[] array = m_gameLogic.GetNextShape();
        if (array != null)
        {
            GenerateModelShape(array);
        }
    }*/

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new PlayerInfo[playerCount];
        for( int i=0; i<playerCount; i++ )
        {
            m_playerInfos[i] = new PlayerInfo();
            m_playerInfos[i].canvas = GameObject.Instantiate<DS_PlayerCanvas>(m_playerCanvasPrefab);
            m_playerInfos[i].canvas.transform.position = new Vector3(4000f * (i + 1), 0f, 0f);
            m_playerInfos[i].canvas.Init(i, m_fDrawingAreaRange, this );
            m_playerInfos[i].canvas.SetCameraRegion(playerCount);
            m_playerInfos[i].canvas.m_addScore = AddScore;
        }
        HudManager.sSPLITHUD_COUNT = playerCount;
    }

    /*public override void UpdatePlayerBoot(GK_NetworkPlayer player)
    {
        if (m_miniGameState == MiniGameState.playing)
        {
            float fTime = Time.time;
            if (fTime > m_fBootNextActionTimer)
            {
                float gameTime = (fTime - m_fStartTimer) / m_drawShapeData.gameTime;

                // Make Action
                if (m_fBootNextActionTimer != -1)
                {
                    float fScore = (m_currentBoot.pointsEvolution[m_nBootAnimCurveSelected].Evaluate(gameTime) * m_fBootFinalScore); // * (float)m_currentBoot.m_nGoodActionPointWin;
                    player.SetLocalCurrentScore((int)fScore);
                }

                // compute next time
                m_fBootNextActionTimer = fTime + 0.2f;
            }
        }
    }*/
}