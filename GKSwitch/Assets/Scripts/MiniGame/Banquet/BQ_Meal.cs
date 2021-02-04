using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_Meal : MonoBehaviour
{
    private const int nDRAGMOVEPOSITION_COUNT = 5;


    [SerializeField]
    Animator m_animator;
    [SerializeField]
    Transform m_mealEltTransform;
    [SerializeField]
    Sprite[] m_ColorSprites;
    [SerializeField]
    SpriteRenderer m_ColorSpriteRenderer;

    public int itemId { get { return m_nItemId; } }
    public int colorId { get { return m_nColorId; } }

    System.Func<float> m_getSpeedMul;
    System.Action<BQ_Meal> m_deleteAction;
    System.Func<bool> m_canMoveMeal;
    System.Func<Vector3, bool> m_isMealOnTheBelt;


    private int m_nItemId;
    private int m_nColorId;

    private Vector3 _startPosition;
    private Vector3 _offsetToMouse;
    private Vector3 m_vLaunchedSpeed = Vector3.zero;
    private BQ_OrderView m_overOrderView;

    private List<Vector3> m_vDragPosition;

    private bool m_bIsDragged = false;
    private BQ_MealElt m_mealElt;

    // Use this for initialization
    void Start()
    {
        m_vDragPosition = new List<Vector3>();
    }

    public void Setup(int nItemId, int nColorId,
        System.Func<float> getSpeedMul,
        System.Action<BQ_Meal> deleteAction,
        System.Func<bool> canMoveMeal,
        System.Func<Vector3, bool> isMealOnTheBelt,
        bool bUseColor,
        int nItemShownId,
        BQ_MealElt eltPrefab
        )
    {
        m_nItemId = nItemId;
        m_nColorId = nColorId;
        m_getSpeedMul = getSpeedMul;
        m_canMoveMeal = canMoveMeal;
        m_deleteAction = deleteAction;
        m_isMealOnTheBelt = isMealOnTheBelt;
        //m_animator.Play("Balloon_on");

        m_mealElt = GameObject.Instantiate<BQ_MealElt>(eltPrefab, m_mealEltTransform);
        m_mealElt.m_meal = this;
 /*       m_mealElt.onBeginDragDlg = OnBeginDrag;
        m_mealElt.onDragDlg = OnDrag;
        m_mealElt.onEndDragDlg = OnEndDrag;*/

        m_ColorSpriteRenderer.gameObject.SetActive(bUseColor);
        m_ColorSpriteRenderer.sprite = m_ColorSprites[m_nColorId];
    }

    /*public Sprite GetEltSprite(int nItemId)
    {
        BQ_MealElt elt = m_mealEltPrefabs[nItemId];
        SpriteRenderer render = elt.gameObject.GetComponent<SpriteRenderer>();
        return render.sprite;
    }*/

    public Sprite GetColorSprite(int nColorId)
    {
        return m_ColorSprites[nColorId];
    }

    public void Update()
    {
        if (!m_bIsDragged)
        {
            if (m_vLaunchedSpeed != Vector3.zero)
            {
                Vector3 vMove = m_vLaunchedSpeed * Time.deltaTime;
                transform.Translate(vMove);
                m_vLaunchedSpeed.x /= 1.08f;

                if (Mathf.Abs(m_vLaunchedSpeed.x) < 30f)
                {
                    m_vLaunchedSpeed = Vector3.zero;
                    // End thrown, test if still on the belt or not
                    if (m_isMealOnTheBelt(transform.position))
                    {
                        m_animator.SetBool("Hold", false);
                    }
                    else
                    {
                        m_animator.SetTrigger("GetOut");
                    }
                }
            }
            else if (m_isMealOnTheBelt(transform.position))
            {
                float fSpeed = (m_getSpeedMul != null ? m_getSpeedMul() : 0f);
                Vector3 vMove = Vector3.down * Time.deltaTime * fSpeed;
                transform.Translate(vMove);
            }

        }

        if (transform.position.y < BQ_Banquet.fMEAL_END_Y)
        {
            DeleteMeal();
        }
    }

    public void DeleteMeal()
    {
        if (m_mealElt != null)
        {
            GameObject.Destroy(m_mealElt.gameObject);
        }

        if (m_deleteAction != null)
        {
            m_deleteAction(this);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void OnPlayerInput(int playerId, Vector2 vScreenPos, RRPlayerInput.ButtonPhase buttonPhase)
    {
        switch( buttonPhase )
        {
            case RRPlayerInput.ButtonPhase.press:
                {
                    OnPlayerDragEnter(playerId, vScreenPos);
                }
                break;
            case RRPlayerInput.ButtonPhase.on:
                {
                    OnPlayerDrag(playerId, vScreenPos);
                }
                break;
            case RRPlayerInput.ButtonPhase.release:
                {
                    OnEndDrag(playerId, vScreenPos);
                }
                break;
        }
    }

    void OnPlayerDragEnter( int playerId, Vector2 vScreenPos )
    {
        if (m_canMoveMeal != null && m_canMoveMeal())
        {
            //FMODUnity.RuntimeManager.PlayOneShot("event:/Banquet/GrabPlate"); // "event:/GoodMove");
            _startPosition = transform.position;
            _offsetToMouse = _startPosition - Camera.main.ViewportToWorldPoint(
                new Vector3(vScreenPos.x, vScreenPos.y, 100f)
            );
            _offsetToMouse.z = 0;
            m_bIsDragged = true;
            m_vDragPosition.Clear();
            //        Debug.Log("OnBeginDrag");
            m_animator.SetBool("Hold", true);
            m_mealElt.DisableCollider();
        }
    }

    void OnPlayerDrag(int playerId, Vector2 vScreenPos)
    {
        if (!m_bIsDragged)
        {
            OnPlayerDragEnter(playerId, vScreenPos);
            return;
        }

        if (!m_canMoveMeal())
        {
            OnEndDrag( playerId, vScreenPos);
        }

/*        List<GameObject> hoverObject = eventData.hovered;
        bool bFound = false;
        int nHoverObjIterator = 0;
        CB_OrderView overOrder = null;

        while (nHoverObjIterator < hoverObject.Count && !bFound)
        {
            overOrder = hoverObject[nHoverObjIterator].GetComponent<CB_OrderView>();
            if (overOrder != null && overOrder.bIsEmpty)
            {
                bFound = true;
            }
            else
            {
                nHoverObjIterator++;
            }
        }

        if (m_overOrderView != overOrder)
        {
            if (m_overOrderView != null)
            {
                m_overOrderView.SetHoover(false);
            }
            m_overOrderView = overOrder;
            if (m_overOrderView != null)
            {
                m_overOrderView.SetHoover(true);
            }
        }
        */
        if (m_bIsDragged)
        {
                    Debug.Log("OnDrag");
            transform.position = Camera.main.ViewportToWorldPoint(
                new Vector3(vScreenPos.x, vScreenPos.y, 4f)
                ) + _offsetToMouse;
            m_vDragPosition.Add(transform.position);
            if (m_vDragPosition.Count > nDRAGMOVEPOSITION_COUNT)
            {
                m_vDragPosition.RemoveAt(0);
            }
        }
    }

    void OnEndDrag(int playerId, Vector2 vScreenPos)
    {
        if (!m_canMoveMeal())
        {
            DeleteMeal();
            return;
        }

        if (m_bIsDragged)
        {
            _offsetToMouse = Vector3.zero;
            m_bIsDragged = false;

            if (m_overOrderView != null)
            {
                m_animator.SetBool("Hold", false);
                //m_overOrderView.PlaceMeal(this);
            }
            else
            {
                //        Debug.Log("OnEndDrag");
                Vector3 v = transform.position;
                v.z += 100f;
                transform.position = v;
                m_bIsDragged = false;
                m_vLaunchedSpeed = (m_vDragPosition[m_vDragPosition.Count - 1] - m_vDragPosition[0]) * 5f;
                m_vLaunchedSpeed.y = 0f;
                m_vLaunchedSpeed.z = 0f;
                m_mealElt.EnableCollider();
                //FMODUnity.RuntimeManager.PlayOneShot("event:/Banquet/ReleaseTable");
            }
        }
    }
}
