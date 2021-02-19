using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EX_Expression : MiniGameTemplate<EX_Logic, EX_Expression.ExpressionData, EX_Expression.ExpressionBotData, ExpressionsHud, EX_Balancing>
{
    public enum ExpressionLoadable { bkg = 1, item=2 }
    public enum ApparitionCurveType { linear, rebound, jump, hesitation }

    [System.Serializable]
    public class ApparitionAnimationData
    {
        [SerializeField]
        public ApparitionCurveType m_curveType;
        [Tooltip("time in x, y=0 for start point, y=1 for end point")]
        [SerializeField]
        public AnimationCurve m_translationCurve;
    }

    //    event:/Picking/PickingCherryGoodPerfect
    [System.Serializable]
    public class ExpressionData : MiniGameBalancingData
    {
        public int nPlayerChoiceCount = 3;
        public ExpressionsDataSequence[] sequencesArray;
    }

    [System.Serializable]
    public class ExpressionsDataSequence
    {
        /// <summary>
        /// Time the sequence start, include countdown time
        /// </summary>
        public float startTime;
        [lwMinMaxVector(0.1f, 10f)]
        public Vector2 apparitionTime = new Vector2(1f, 1f);
        [lwMinMaxVector(0.1f, 10f)]
        public Vector2 idleTime = new Vector2(1f, 1f);
        [lwMinMaxVector(0.1f, 10f)]
        public Vector2 disappearTime = new Vector2(0.1f, 0.5f);
        [lwMinMaxVector(0.1f, 10f)]
        public Vector2 timebetweenSpawn = new Vector2(1f, 2f);
        [lwMinMaxVector(1, 10, true)]
        public Vector2 goodInRow = new Vector2(1f, 1f);
        [lwMinMaxVector(1, 10, true)]
        public Vector2 badInRow = new Vector2(1f, 1f);
        public int goodExpressionPercent;
        public int nEyeDifferentSpriteCount = 3;
        public int nMouthDifferentSpriteCount = 3;
    }

    [System.Serializable]
    public class ExpressionBotData : MiniGameBotData
    {
        [Tooltip("determine when (in the character animation) the bot react, 0 at the beginning, 1 at the end")]
        public AnimationCurve characterAnimationRatioReactionTime;
        public float fRandomGapForReactionTime;
        [Tooltip("fact that he can win point with seen good element")]
        public AnimationCurve goodReactionPercentWithGoodElement;
        [Tooltip("fact that he can loose point with seen bad element")]
        public AnimationCurve goodReactionPercentWithBadElement;
    }

    [Header("Game Config")]
    [SerializeField]
    private int m_nBadPointsLost = 1000;
    [SerializeField]
    private float m_fTimeToSeeCharacter = 10f;
    [SerializeField]
    public ApparitionAnimationData[] m_apparitionAnimationDataArray;

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;
    [SerializeField]
    private AssetReference m_itemReference;

    private lwObjectPool<EX_Character> m_itemPool;
    private GameObject m_itemRoot;
    private EX_Character m_itemPrefab;
    public EX_MainObject m_bkg { private set; get; }
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(ExpressionLoadable)).Length)) - 1;

    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.Expressions);
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

    private void SetupHud()
    {
            ushort[] nMaskArray = m_gameLogic.goodExpressionArray;
            Sprite[][] sprArray = new Sprite[nMaskArray.Length][];

            for (int i = 0; i < nMaskArray.Length; i++)
            {
                sprArray[i] = GenerateSpriteArrayFromMask(nMaskArray[i]);
            }

            m_hud.Setup(nMaskArray, sprArray, m_nGoodPointsWin);
    }


    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if (buttonPhase != RRPlayerInput.ButtonPhase.press)
        {
            return;
        }
        Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(v.x, v.y, 0));

        //GameObject spere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //spere.transform.position = rayOrigin;
        Ray ray = new Ray(rayOrigin, Vector3.forward);
        RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray); // Camera.main.ScreenPointToRay(v));

        if (rayHit.transform != null)
        {
            EX_Character character = rayHit.transform.GetComponent<EX_Character>();
            if (character != null)
            {
                character.OnShoot(playerId);
            }
        }
    }

    public override void Clean()
    {
        m_itemPool.Destroy();
        GameObject.Destroy(m_itemRoot);

        GameObject.Destroy(m_bkg.gameObject);
        Addressables.ClearResourceLocators();
        base.Clean();
    }

    void InitAfterLoad()
    {
        m_itemPool = new lwObjectPool<EX_Character>();
        m_itemRoot = new GameObject("ItemRoot");
        m_itemPool.Init(m_itemPrefab, 5, m_itemRoot.transform);

        m_gameLogic = new EX_Logic();
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected, this);

        SetupHud();
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

    private void CheckSpawn()
    {
        bool bContinue = true;
        while (bContinue)
        {
            EX_Logic.ItemSpawnInfo spawn = m_gameLogic.CheckIfItemToSpawn();
            if (spawn != null)
            {
                SpawnItem(spawn);
                //SpawnMeal(spawn.fSpawnerRatioPos, spawn.nItemId, spawn.nColorId);
            }
            else
            {
                bContinue = false;
            }

        }
    }

    private EX_Character SpawnItem(EX_Logic.ItemSpawnInfo spawn)
    {
        AnimationCurve apparitionCurve = m_apparitionAnimationDataArray[spawn.nComeInCurve].m_translationCurve;
        EX_MoveConfig moveConfig = m_bkg.m_hideSpotArray[spawn.nSpotId];
        EX_Character item = m_itemPool.GetInstance(m_itemRoot.transform);
        item.transform.localPosition = new Vector3(moveConfig.m_vStartPoint.x, moveConfig.m_vStartPoint.y, transform.position.z);
        Sprite[] sprArray = GenerateSpriteArrayFromMask(spawn.nExpressionMask);

        item.Setup(sprArray, spawn.nExpressionMask, apparitionCurve, moveConfig, spawn, DeleteItem, OnItemPress);
        return item;
        /* CB_Meal meal = m_mealPool.GetInstance(transform);
         float fZ = -100f;
         meal.transform.localPosition = new Vector3((fSpawnerRatioPos - 0.5f) * m_nBeltSize, fMEAL_START_Y, fZ);
         meal.Setup(nItemId, nColorId, GetBeltSpeed, DeleteMeal, CanMoveMeal, IsMealOnTheBelt);*/
    }

    private Sprite[] GenerateSpriteArrayFromMask(int nMask)
    {
        int nMouthId = (int)(nMask >> 8);
        int nEyeRightId = (int)((nMask >> 4) & 15);
        int nEyeLeftId = (int)(nMask & 15);
        Sprite[] sprArray = new Sprite[4];
        sprArray[0] = null;
        sprArray[1] = m_bkg.m_eyeSpritesArray[nEyeLeftId];
        sprArray[2] = m_bkg.m_eyeSpritesArray[nEyeRightId];
        sprArray[3] = m_bkg.m_mouthSpritesArray[nMouthId];
        return sprArray;
    }

    private void DeleteItem(EX_Character item)
    {
        //       Debug.Log("Real end time : " + m_networkConfig.gameTime );
        m_itemPool.PoolObject(item);
    }

    private bool OnItemPress(EX_Character item, int playerId )
    {
        if (m_miniGameState != MiniGameState.playing)
        {
            return false;
        }

        ushort nExpressionMask = item.nExpressionMask;
        ushort[] nMaskArray = m_gameLogic.goodExpressionArray;

        bool bGood = false;
        int nExpressionId = 0;

        while (!bGood && nExpressionId < nMaskArray.Length)
        {
            if (nMaskArray[nExpressionId] == nExpressionMask)
            {
                bGood = true;
            }
            else
            {
                nExpressionId++;
            }
        }

        if (bGood)
        {
            ComputeAndAddPointForItem(item, playerId);
            m_hud.PlaySpotGoodAnim(nExpressionMask);
        }
        else
        {
            AddPoint(false, playerId, item.transform.position, m_nBadPointsLost);
            item.BadMove();
        }
        return true;
    }

    private void ComputeAndAddPointForItem(EX_Character item, int playerId )
    {
        item.Found();

        float fTime = item.ComputeShowTime();
        float fPointCoeff = 1f - (Mathf.Min(fTime, m_fTimeToSeeCharacter) / m_fTimeToSeeCharacter);
        int nPoints = (int)(m_nGoodPointsWin * fPointCoeff);
        AddPoint(true, playerId, item.transform.position, nPoints);
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

        // Item
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_itemReference, OnItemLoad);
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<EX_MainObject>();
            m_loadMask |= (int)ExpressionLoadable.bkg;
        }

    }

    private void OnItemLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_itemPrefab = obj.Result.GetComponent<EX_Character>();
        }

        m_loadMask |= (int)ExpressionLoadable.item;
    }
    #endregion
}
