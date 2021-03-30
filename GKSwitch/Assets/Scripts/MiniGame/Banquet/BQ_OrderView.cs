using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_OrderView : MonoBehaviour
{
    private const float fHORIZONTAL_OFFSET = 500f;
    private const float fREJECT_VALUE = 3000f;
    private const float fREJECT_VALUE_X = 3000f;

    [SerializeField]
    Sprite[] m_plateSprites;
    [SerializeField]
    SpriteRenderer m_plateRenderer;
    [SerializeField]
    Animator m_animator;

    public BQ_OrderBubble bubble { set { m_bubble = value; } }
    public System.Action<int> onEndMealGiven { set { m_onEndMealGiven = value; } }
    public System.Action<bool, int> onMealResult { set { m_onMealResult = value; } }
    public bool bIsEmpty { get { return m_placedMeal == null; } }
    public int slotId { get { return m_nSlotId; } }


    private BQ_Order m_order;
    private int m_nSlotId;
    private BQ_Meal m_placedMeal;

    private BQ_OrderBubble m_bubble;
    private Vector3 m_vSlotPosition;
    private Vector3 m_vStartPosition;
    private Vector2 m_rejectRangeX;
    private float m_rejectY;

    private System.Action<int> m_onEndMealGiven;
    private System.Action<bool, int> m_onMealResult;
    private void Awake()
    {
        m_vSlotPosition = transform.position;
    }

    public void SetSlotId(int nSlotId, bool bUpSlot)
    {
        m_nSlotId = nSlotId;
        m_vStartPosition = m_vSlotPosition + Vector3.up * fHORIZONTAL_OFFSET * (bUpSlot ? 1 : -1);
        if( m_vStartPosition.x == 0 )
        {
            m_rejectRangeX = new Vector2(-fREJECT_VALUE_X, fREJECT_VALUE_X);
        }
        else if( m_vStartPosition.x < 0 )
        {
            m_rejectRangeX = new Vector2(0f, fREJECT_VALUE_X);
        }
        else
        {
            m_rejectRangeX = new Vector2(-fREJECT_VALUE_X, 0f);
        }
        m_rejectY = (bUpSlot ? -1 : 1) * fREJECT_VALUE;
    }


    public void SetHoover(bool bOn)
    {
        m_animator.SetBool("Over", bOn);
    }

    public void SetActive(bool bActive)
    {
        gameObject.SetActive(bActive);
        m_bubble.gameObject.SetActive(bActive);
    }

    public void SetOrder(BQ_Order order)
    {
        m_order = order;
        SetColor(m_order.colorId);
        m_bubble.SetElement(m_order.shownEltId, m_order.isElt);
        transform.position = m_vStartPosition;
        gameObject.SetActive(true);
        StartCoroutine(PlayIntroAnimation());
    }

    public BQ_Order GetOrder()
    {
        return m_order;
    }

    public void SetColor(int nColorId)
    {
        int nSprId = nColorId == -1 ? m_plateSprites.Length - 1 : nColorId;
        m_plateRenderer.sprite = m_plateSprites[nSprId];
    }

    public void PlaceMeal(BQ_Meal meal)
    {
        bool bGood = m_order.IsMealValid(meal.itemId, meal.colorId);
        m_bubble.SetResult(bGood);
        m_placedMeal = meal;
        meal.transform.parent = transform;
        Vector3 vPos = meal.transform.localPosition;
        vPos.x = 0f;
        vPos.y = 0f;
        meal.transform.localPosition = vPos;
        StartCoroutine(PlayOutroAnimation());
        //        m_animator.SetTrigger()
    }

    public Vector3 ComputeRejectLaunch()
    {
        float fX = Random.Range(m_rejectRangeX.x, m_rejectRangeX.y);
        Debug.Log("#### ComputeRejectLaunch fX " + fX);

        return new Vector3(fX, m_rejectY, 0f);
    }

    private IEnumerator PlayIntroAnimation()
    {
        float fElapsedTime = 0;
        float fStartTime = Time.time;
        float fAnimTime = 0.5f;
        while (fElapsedTime < fAnimTime)
        {
            fElapsedTime = Time.time - fStartTime;
            float fCoeff = Mathf.Min(fElapsedTime / fAnimTime, 1f);
            transform.position = Vector3.Lerp(m_vStartPosition, m_vSlotPosition, fCoeff);
            if (fElapsedTime < fAnimTime)
            {
                yield return null;
            }
        }
        m_bubble.gameObject.SetActive(true);
    }

    private IEnumerator PlayOutroAnimation()
    {
        if (m_onMealResult != null)
        {
            m_onMealResult(m_order.IsMealValid(m_placedMeal.itemId, m_placedMeal.colorId), m_nSlotId);
        }

        float fElapsedTime = 0;
        float fStartTime = Time.time;
        float fAnimTime = 0.5f;
        while (fElapsedTime < fAnimTime)
        {
            fElapsedTime = Time.time - fStartTime;
            if (fElapsedTime < fAnimTime)
            {
                yield return null;
            }
        }

        m_animator.SetTrigger("Outro");
        fElapsedTime = 0;
        fStartTime = Time.time;
        fAnimTime = 0.5f;
        while (fElapsedTime < fAnimTime)
        {
            fElapsedTime = Time.time - fStartTime;
            float fCoeff = Mathf.Min(fElapsedTime / fAnimTime, 1f);
            transform.position = Vector3.Lerp(m_vSlotPosition, m_vStartPosition, fCoeff);
            if (fElapsedTime < fAnimTime)
            {
                yield return null;
            }
        }

        fElapsedTime = 0;
        fStartTime = Time.time;
        fAnimTime = 0.5f;
        while (fElapsedTime < fAnimTime)
        {
            fElapsedTime = Time.time - fStartTime;
            if (fElapsedTime < fAnimTime)
            {
                yield return null;
            }
        }

        SetActive(false);

        if (m_placedMeal != null)
        {
            m_placedMeal.DeleteMeal();
            m_placedMeal = null;
        }

        if (m_onEndMealGiven != null)
        {
            m_onEndMealGiven(m_nSlotId);
        }
    }
}
