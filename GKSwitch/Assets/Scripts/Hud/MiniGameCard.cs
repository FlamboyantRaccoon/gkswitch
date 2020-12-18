using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MiniGameCard : MiniGameCardBase
{

    public System.Action onEndSelectionAnim { set { m_onEndSelectionAnim = value; } }


    [SerializeField]
    Animator m_animator;
    [SerializeField]
    Image m_CardBase;
    [SerializeField]
    Image m_StarImg;
    [SerializeField]
    Sprite[] m_normalLockedSprite;

    private Canvas m_canvas;
    private RectTransform m_rectTransform;
    private System.Action m_onEndSelectionAnim;
    private System.Action m_onEndLevelShownAnim;
    private System.Action m_onEndLevelHideAnim;

    private int m_nEqualizedLevels = 0;

    // Use this for initialization
    void Awake ()
    {
        SetupAttributeMembers();
    }


    void SetupAttributeMembers()
    {
        if( m_canvas==null )
        {
            m_canvas = GetComponent<Canvas>();
            m_canvas.overrideSorting = true;
        }

        if( m_rectTransform==null )
        {
            m_rectTransform = GetComponent<RectTransform>();
        }
    }
    /*private void Start()
    {
        m_animator.SetBool("CardSelected", true);
    }*/

    public void Init( int nMiniGameId )
    {
        InitGame(nMiniGameId);
    }

    public void SetAvailable( bool bAvailable)
    {
        m_CardBase.sprite = m_normalLockedSprite[bAvailable ? 0 : 1];
        m_StarImg.gameObject.SetActive(bAvailable);
        m_miniGameLevel.gameObject.SetActive(bAvailable);
    }

    public void SetOrderInLayer( int nSortingOrder )
    {
        m_canvas.sortingOrder = nSortingOrder;
    }

    public void SetPositionAndOrderInLayer( Vector3 vPos, int nSortingOrder)
    {
        if (m_rectTransform == null)
        {
            m_rectTransform = GetComponent<RectTransform>();
        }

        m_rectTransform.localPosition = vPos;
        if( m_canvas!=null )
        {
            m_canvas.sortingOrder = nSortingOrder;
        }
    }

    public void SetAngle( float fAngle)
    {
        transform.rotation = Quaternion.Euler(0f, 0f, fAngle);
    }

    public void PlaySelectedAnim()
    {
        m_animator.SetTrigger("ForceSelect");
    }

    public void OnEndSelectionAnim()
    {
        if( m_onEndSelectionAnim != null )
        {
            m_onEndSelectionAnim();
        }
    }

    public void ShowLevels(System.Action onEndLevelShownAnim)
    {
        m_onEndLevelShownAnim = onEndLevelShownAnim;
        m_animator.SetBool("Level_visible", true );
    }

    public void HideLevels(System.Action onEndLevelHideAnim)
    {
        m_onEndLevelHideAnim = onEndLevelHideAnim;
        m_animator.SetBool("Level_visible", false);
    }

    public void OnEndLevelShownAnim()
    {
        if (m_onEndLevelShownAnim != null)
        {
            m_onEndLevelShownAnim();
        }
    }

    public void OnEndLevelHideAnim()
    {
        if (m_onEndLevelHideAnim != null)
        {
            m_onEndLevelHideAnim();
        }
    }

    public void RemoveCanvas()
    {
        if( m_canvas!=null )
        {
            GameObject.Destroy(m_canvas);
            m_canvas = null;
        }
    }

    public Sprite GetMiniGameSprite(int nMiniGame)
    {
        return m_miniGameSpriteArray[nMiniGame];
    }
}
