//#define DEBUG_TEXT

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class HudManager : lwSingletonMonoBehaviour<HudManager>
{
    public enum GameHudType { logoScreen, splashScreen, mainMenu, miniGame, countdown }
    public enum HudRootType { hud, popup, foreground }
    public enum PopupType { }
    public enum ForeHudType { aimingHud }


    [System.Serializable] private class GameHudPrefab : lwEnumArray<GameHudType, GameObject> { }; // dummy definition to use Unity serialization
    [System.Serializable] private class PopupHudPrefab : lwEnumArray<PopupType, GameObject> { }; // dummy definition to use Unity serialization
    [System.Serializable] private class ForeHudPrefab : lwEnumArray<ForeHudType, GameObject> { }; // dummy definition to use Unity serialization
    [System.Serializable] private class HudRoot : lwEnumArray<HudRootType, RectTransform> { }; // dummy definition to use Unity serialization

    [SerializeField]
    private HudRoot m_hudRoot = null;
    [SerializeField]
    private GameHudPrefab m_hudPrefab = null;
    [SerializeField]
    private PopupHudPrefab m_popupPrefab = null;
    [SerializeField]
    private ForeHudPrefab m_forePrefab = null;
    [SerializeField]
    public Image m_popupMaskPrefab = null;
    [SerializeField]
    HudScorePop m_scorePopWinPrefab;
    [SerializeField]
    HudScorePop m_scorePopLosePrefab;

    public System.Action onPopupHideDlg { set { m_onPopupHideDlg = value; } }

    private lwObjectPool<HudScorePop> m_scorePopWin;
    private lwObjectPool<HudScorePop> m_scorePopLoose;

    private Image m_popupMask;
    private GameObject[] m_hudArray;
    private GameObject[] m_popupArray;
    private GameObject[] m_foreHudArray;
    private RectTransform m_rootRect;
    private System.Action m_onPopupHideDlg;

    private GameObject m_placeRoot;

    private void Awake()
    {
        if (m_placeRoot == null)
        {
            m_placeRoot = new GameObject("PlaceRoot");
            m_placeRoot.transform.SetParent(transform);
        }

        m_rootRect = GetComponent<RectTransform>();

        m_hudArray = new GameObject[System.Enum.GetNames(typeof(GameHudType)).Length];
        m_popupArray = new GameObject[System.Enum.GetNames(typeof(PopupType)).Length];
        m_foreHudArray = new GameObject[System.Enum.GetNames(typeof(ForeHudType)).Length];

        m_scorePopWin = new lwObjectPool<HudScorePop>();
        m_scorePopWin.Init(m_scorePopWinPrefab, 10, m_hudRoot[HudRootType.hud]);
        m_scorePopLoose = new lwObjectPool<HudScorePop>();
        m_scorePopLoose.Init(m_scorePopLosePrefab, 10, m_hudRoot[HudRootType.hud]);
        //ShowHud(GameHudType.splashScreen);

    }

    public Vector2 GetUIPosition( Vector2 vScreenPos )
    {
        Vector3 vPos = Camera.main.ScreenToViewportPoint(vScreenPos);
        float fX = vPos.x * m_rootRect.rect.width - (m_rootRect.rect.width / 2f);
        float fY = vPos.y * m_rootRect.rect.height - (m_rootRect.rect.height / 2f);

        return new Vector2(fX, fY);
    }

    public Rect GetRect()
    {
        return m_rootRect.rect;
    }

    public void ShowHud( GameHudType hudType )
    {
        int nHudId = (int)hudType;
        if( m_hudArray[nHudId]==null )
        {
            m_hudArray[nHudId] = GameObject.Instantiate(m_hudPrefab[nHudId], m_hudRoot[HudRootType.hud] );
        }
        else
        {
            if(!m_hudArray[nHudId].activeSelf )
            {
                m_hudArray[nHudId].SetActive(true);
            }
        }
    }

    public void HideHud(GameHudType hudType)
    {
        int nHudId = (int)hudType;
        if (m_hudArray[nHudId] != null)
        {
            m_hudArray[nHudId].SetActive(false);
        }
    }

    public T GetHud<T>(GameHudType hudType) where T : MonoBehaviour
    {
        int nHudId = (int)hudType;
        if (m_hudArray[nHudId] != null)
        {
            T tmp = m_hudArray[nHudId].GetComponent<T>();
            return tmp;
        }
        return null;
    }

    public void ShowForeHud(ForeHudType hudType)
    {
        int nHudId = (int)hudType;
        if (m_foreHudArray[nHudId] == null)
        {
            m_foreHudArray[nHudId] = GameObject.Instantiate(m_forePrefab[nHudId], m_hudRoot[HudRootType.foreground]);
        }
        else
        {
            if (!m_foreHudArray[nHudId].activeSelf)
            {
                m_foreHudArray[nHudId].SetActive(true);
            }
        }
    }

    public void HideForeHud(ForeHudType hudType)
    {
        int nHudId = (int)hudType;
        if (m_foreHudArray[nHudId] != null)
        {
            m_foreHudArray[nHudId].SetActive(false);
        }
    }

    public T GetForeHud<T>(ForeHudType hudType) where T : MonoBehaviour
    {
        int nHudId = (int)hudType;
        if (m_foreHudArray[nHudId] != null)
        {
            T tmp = m_foreHudArray[nHudId].GetComponent<T>();
            return tmp;
        }
        return null;
    }

    public void SetHudInFront(GameHudType hudType )
    {
        int nId = (int)hudType;
        if( m_hudArray[nId]!=null )
        {
            m_hudArray[nId].transform.SetAsLastSibling();
        }
    }

    public void CreateMiniGameHud(GameObject prefab)
    {
        int nHudId = (int)GameHudType.miniGame;
        if (m_hudArray[nHudId] != null)
        {
            GameObject.Destroy(m_hudArray[nHudId]);
        }
        m_hudArray[nHudId] = GameObject.Instantiate(prefab, m_hudRoot[HudRootType.hud]);
    }

    public void ClosePopup(PopupType popupType, bool bUseAnimator = true)
    {
        if( bUseAnimator )
        {
            
        }
        else
        {
            HidePopup(popupType);
        }
    }

 

    private void HidePopup(PopupType popupType )
    {
        int nPopupId = (int)popupType;
        if (m_popupArray[nPopupId] != null)
        {
            m_popupArray[nPopupId].SetActive(false);
        }
        m_popupMask.gameObject.SetActive(false);
        //m_popupRoot.gameObject.SetActive(false);

        m_onPopupHideDlg?.Invoke();
    }

    

    public T ShowPopup<T>(PopupType popupType, bool bUseAnimator=true ) where T : Component
    {
        int nPopupId = (int)popupType;
        Transform parent = m_hudRoot[HudRootType.popup];

        if (m_popupPrefab[nPopupId] == null)
        {
            return null;
        }

        if (m_popupArray[nPopupId] == null)
        {
            m_popupArray[nPopupId] = GameObject.Instantiate(m_popupPrefab[nPopupId], parent, false );
        }
        else
        {
            m_popupArray[nPopupId].SetActive(true);
        }

        m_popupMask.gameObject.SetActive(true);
        m_popupMask.transform.SetAsLastSibling();

        if( bUseAnimator )
        {
           /* m_popupArray[nPopupId].transform.SetParent( m_popupRoot.container.transform );
            RectTransform rt = m_popupArray[nPopupId].GetComponent<RectTransform>();
            rt.anchoredPosition = Vector2.zero;
            m_popupArray[nPopupId].transform.SetAsLastSibling();
            m_popupRoot.transform.SetAsLastSibling();
            m_popupRoot.Show( popupType );*/
 //           m_popupRoot.gameObject.SetActive(true);
        }
        else
        {
            m_popupArray[nPopupId].transform.SetAsLastSibling();
        }

        T tmp = m_popupArray[nPopupId].GetComponent<T>();
        return tmp;
    }

    public Vector2 ComputeHudPosFromWorldPosition(Vector3 vWorldPos)
    {
        RectTransform CanvasRect = GetComponent<RectTransform>();
        Vector2 ViewportPosition = Camera.main.WorldToViewportPoint(vWorldPos);
        Vector2 WorldObject_ScreenPosition = new Vector2(
        ((ViewportPosition.x * CanvasRect.sizeDelta.x) - (CanvasRect.sizeDelta.x * 0.5f)),
        ((ViewportPosition.y * CanvasRect.sizeDelta.y) - (CanvasRect.sizeDelta.y * 0.5f)));
        return WorldObject_ScreenPosition;
    }

    public void SpawnWinScore(Vector3 vWorldPos, int nScore, string sPrefix = "", string sSuffix = "")
    {
        HudScorePop pop = m_scorePopWin.GetInstance(transform);
        pop.Setup(nScore, (HudScorePop elt) => { m_scorePopWin.PoolObject(elt); }, sPrefix, sSuffix);
        Vector2 vCanvasPos = ComputeHudPosFromWorldPosition(vWorldPos);
        pop.transform.localPosition = vCanvasPos;
        /*        RectTransform popRect = pop.gameObject.GetComponent<RectTransform>();
                popRect.anchoredPosition = vCanvasPos;*/
    }

    public void SpawnLoseScore(Vector3 vWorldPos, int nScore)
    {
        HudScorePop pop = m_scorePopLoose.GetInstance(transform);
        pop.Setup(nScore, (HudScorePop elt) => { m_scorePopLoose.PoolObject(elt); });
        Vector2 vCanvasPos = ComputeHudPosFromWorldPosition(vWorldPos);
        pop.transform.position = vCanvasPos;
        RectTransform popRect = pop.gameObject.GetComponent<RectTransform>();
        popRect.anchoredPosition = vCanvasPos;
    }
}
