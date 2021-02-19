using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class IH_IceHockey : MiniGameTemplate<IH_Logic, IH_IceHockey.IceHockeyData, IH_IceHockey.IceHockeyBotData, MiniGameBasicHud, IH_Balancing>
{
    public enum IceHockeyLoadable { bkg = 1, playerPrefab = 2, ballPrefab = 4 }

    //    event:/Picking/PickingCherryGoodPerfect
    [System.Serializable]
    public class IceHockeyData : MiniGameBalancingData
    {
    }

    [System.Serializable]
    public class IceHockeyBotData : MiniGameBotData
    {
    }

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;
    [SerializeField]
    private AssetReference m_playerReference;
    [SerializeField]
    private AssetReference m_ballReference;

    private IH_Player m_playerPrefab;
    private IH_Ball m_ballPrefab;

    private IH_PlayerInfos[] m_playerInfos;
    private IH_Ball m_ball;

    private IH_MainObject m_bkg;
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(IceHockeyLoadable)).Length)) - 1;

    protected void Awake()
    {
        InitGameDataAndBot(MiniGameManager.MiniGames.IceHockey);
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

        InitPlayerInfos();
        StartLoading();
        m_gameStats = new int[2] { 0, 0 };
    }

    private void InitPlayerInfos()
    {
        int playerCount = BattleContext.instance.playerCount;
        m_playerInfos = new IH_PlayerInfos[playerCount];
        for (int i = 0; i < playerCount; i++)
        {
            m_playerInfos[i] = new IH_PlayerInfos();
            m_playerInfos[i].Setup(i);
        }
    }

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if (m_playerInfos != null)
        {
            IH_PlayerInfos player = m_playerInfos[playerId];
            player.ManageFireInput(v, buttonPhase);
        }
    }

    public override void Clean()
    {
        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            m_playerInfos[i].Clean();
        }

        GameObject.Destroy(m_bkg.gameObject);
        Addressables.ClearResourceLocators();
        base.Clean();
    }

    void InitAfterLoad()
    {
        GameSettings gameSettings = GameContext.instance.m_settings;
        ToastyCollection toasties = GameContext.instance.m_toastyCollection;

        for (int i = 0; i < m_playerInfos.Length; i++)
        {
            IH_Player player = GameObject.Instantiate<IH_Player>(m_playerPrefab, transform);
            GKPlayerData playerData = BattleContext.instance.GetPlayer(i);
            player.SetAvatar(toasties.GetToasty(playerData.sToastyId).avatar);

            m_playerInfos[i].SetPlayer(player, m_bkg.sideRect[i%m_bkg.sideRect.Length]);
        }

        m_ball = GameObject.Instantiate<IH_Ball>(m_ballPrefab, transform);

        m_gameLogic = new IH_Logic();
        m_gameLogic.Init(m_gameData, m_nMiniGameDataSelected);
    }

    protected override void UpdateWarmUp()
    {
    }

    protected override bool UpdateGamePlay()
    {
        return CheckAndUpdateTime();
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

        // Player
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_playerReference, OnPlayerLoad);

        // Ball
        RR_AdressableAsset.instance.LoadAsset<GameObject>(m_ballReference, OnBallLoad);
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<IH_MainObject>();
            m_loadMask |= (int)IceHockeyLoadable.bkg;
        }

    }

    private void OnPlayerLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_playerPrefab = obj.Result.GetComponent<IH_Player>();
        }

        m_loadMask |= (int)IceHockeyLoadable.playerPrefab;
    }

    private void OnBallLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            m_ballPrefab = obj.Result.GetComponent<IH_Ball>();
        }

        m_loadMask |= (int)IceHockeyLoadable.ballPrefab;
    }
    #endregion
}
