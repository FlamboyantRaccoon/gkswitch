using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PK_Item : MonoBehaviour
{
    [System.Serializable] public class itemTypeSprite : lwEnumArray<PK_Picking.itemType, Sprite> { }; // dummy definition to use Unity serialization

    System.Action<PK_Item> m_deleteAction;
    System.Action<PK_Item, int> m_onItemFallout;

    [SerializeField]
    private SpriteRenderer[] m_sprite;
    [SerializeField]
    private itemTypeSprite m_sprites;
    [SerializeField]
    Animator m_animator;
    [SerializeField]
    AnimationCurve m_flyingNormalCurve;
    [SerializeField]
    float m_flyingNormalMultiplier;

    public PK_Picking.itemType itemType { get { return m_itemType; } }
    public bool bMoving { set { m_bMoving = value; } }
    public bool bEnding { get { return m_bEnding; } }
    public int nReboundLineId { get { return m_nReboundLineId; } }
    public int nLaneId { get { return m_nLaneId; } }
    public float fSpeed { get { return m_fSpeed; } set { m_fSpeed = value; } }


    private PK_Picking.itemType m_itemType;
    private bool m_bMoving;
    private bool m_bEnding;
    private float m_fSpeed;
    private float m_fEndTouchZoneY;
    private int m_nLaneId;
    private int m_playerId;
    private int m_nReboundLineId;
    private System.Func<float> m_getSpeedMultiplier;
    private bool m_bAlreadyTake = false;
    private System.Action<PK_Item, PK_Basket> m_onPickAction;

    public void Setup(PK_Picking.itemType itemType, float fSpeed, float fEndTouchZoneY, int playerId,
        System.Action<PK_Item> deleteAction,
        System.Action<PK_Item, int> onItemFallout,
        int nLaneId,
        int nReboundLineId,
        System.Func<float> getSpeedMultiplier,
        System.Action<PK_Item, PK_Basket> onPickAction

        )
    {
        m_playerId = playerId;
        m_bAlreadyTake = false;
        m_itemType = itemType;
        m_deleteAction = deleteAction;
        m_fSpeed = fSpeed;
        m_fEndTouchZoneY = fEndTouchZoneY;
        m_nLaneId = nLaneId;
        m_nReboundLineId = nReboundLineId;
        m_getSpeedMultiplier = getSpeedMultiplier;
        m_onPickAction = onPickAction;

        m_sprite[0].sprite = m_sprites[itemType];
        if (itemType == PK_Picking.itemType.doubleTouch)
        {
            m_sprite[1].sprite = m_sprites[PK_Picking.itemType.doubleTouchOther];
            m_sprite[1].gameObject.SetActive(true);
        }
        else
        {
            m_sprite[1].gameObject.SetActive(false);
        }

        m_onItemFallout = onItemFallout;
        m_bMoving = true;
        m_bEnding = false;
    }

    public void Update()
    {
        if (m_bMoving)
        {
            Vector3 vMove = Vector3.down * Time.deltaTime * m_fSpeed;
            if (m_getSpeedMultiplier != null)
            {
                vMove *= m_getSpeedMultiplier();
            }

            transform.Translate(vMove);

            if (transform.position.y < m_fEndTouchZoneY && !m_bEnding)
            {
                if (m_onItemFallout != null)
                {
                    m_onItemFallout(this, m_playerId);
                }
                else
                {
                    PlayEndAnim(false);
                    //                    DeleteItem();
                }
            }
        }
    }

    public void DeleteItem()
    {
        if (m_deleteAction != null)
        {
            m_deleteAction(this);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void PlayEndAnim(bool bGood)
    {
        m_bEnding = true;
        if (m_animator != null)
        {
            if (bGood)
            {
                m_animator.SetTrigger("Catch");
            }
            else
            {
                m_animator.SetTrigger("Miss");
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (m_bAlreadyTake)
        {
            return;
        }

        PK_Basket basket = collision.gameObject.GetComponent<PK_Basket>();
        if (basket == null)
        {
            return;
        }

        m_bAlreadyTake = true;
        if (m_onPickAction != null)
        {
            m_onPickAction(this, basket);
        }
        else
        {
            GameObject.Destroy(this.gameObject);
        }
    }

    public void PlayRebound(float fNewX, float fFlyingSpeed)
    {
        StartCoroutine(PlayReboundRoutine(fNewX, fFlyingSpeed));
    }

    public void RemoveSecondSprite()
    {
        m_sprite[1].gameObject.SetActive(false);
    }

    private IEnumerator PlayReboundRoutine(float fNewX, float fFlyingSpeed)
    {
        float fCurrentY = transform.position.y;
        float fCurrentX = transform.position.x;
        float fTargetY = transform.position.y + PK_Picking.fSECONDTOUCH_JUMP;
        float fTargetX = fNewX;
        float fAnimTime = Mathf.Abs(fTargetY - fCurrentY) / fFlyingSpeed;

        Vector3 vPos = transform.position;
        float fDeltaX = fTargetX - fCurrentX;
        float fDeltaY = fTargetY - fCurrentY;
        Vector2 vNormal = new Vector2(fDeltaX > 0 ? -fDeltaY : fDeltaY, fDeltaX > 0 ? fDeltaX : -fDeltaX);
        vNormal.Normalize();

        m_animator.SetBool("Flying", true);

        float fElapsedTime = 0;
        float fStartTime = Time.time;
        while (fElapsedTime < fAnimTime)
        {
            fElapsedTime = Time.time - fStartTime;
            if (fElapsedTime < fAnimTime)
            {
                float fCoeff = fElapsedTime / fAnimTime;
                float fNormalMul = m_flyingNormalCurve.Evaluate(fCoeff) * m_flyingNormalMultiplier;

                vPos.x = fCurrentX + (fTargetX - fCurrentX) * fCoeff + vNormal.x * fNormalMul;
                vPos.y = fCurrentY + (fTargetY - fCurrentY) * fCoeff + vNormal.y * fNormalMul;
                transform.position = vPos;

                yield return null;
            }
        }


        m_animator.SetBool("Flying", false);

        vPos.x = fTargetX;
        vPos.y = fTargetY;
        transform.position = vPos;
        bMoving = true;
    }
}
