using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class TMP_Template : MiniGameTemplate<TMP_Logic, TMP_Template.TmpData, TMP_Template.TmpBotData, MiniGameBasicHud, TMP_Balancing>
{
    public enum TmpLoadable { bkg = 1 }

    //    event:/Picking/PickingCherryGoodPerfect
    [System.Serializable]
    public class TmpData : MiniGameBalancingData
    {
    }

    [System.Serializable]
    public class TmpBotData : MiniGameBotData
    {
    }

    [Header("Loading")]
    [SerializeField]
    private AssetReference m_bkgReference;

    private TMP_MainObject m_bkg;
    private int m_loadMask = 0;
    private int m_loadMaskComplete = (1 << (System.Enum.GetNames(typeof(TmpLoadable)).Length)) - 1;

    protected void Awake()
    {
        //InitGameDataAndBot(MiniGameManager.MiniGames.TrickOrTreat);
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

    protected override void MiniGameFireInput(int playerId, Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
    }

    public override void Clean()
    {
        GameObject.Destroy(m_bkg.gameObject);
        base.Clean();
    }

    void InitAfterLoad()
    {
        m_gameLogic = new TMP_Logic();
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
    }

    private void OnBkgLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            GameObject bkg = Instantiate(loadedObject);
            m_bkg = bkg.GetComponent<TMP_MainObject>();
            m_loadMask |= (int)TmpLoadable.bkg;
        }

    }
    #endregion
}
