using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_OrderBubble : MonoBehaviour
{
    [Tooltip("0 TopLeft, 1 TopRight, 2 BottomLeft, 3 BottomRight")]
    [SerializeField]
    SpriteRenderer[] m_bubbleTips;
    [SerializeField]
    Sprite[] m_ItemsSprite;
    [SerializeField]
    SpriteRenderer m_itemRenderer;
    [SerializeField]
    SpriteRenderer m_itemCross;

    private Animator m_animator;

    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void SetTips(bool bUp, bool bLeft)
    {
        int nShownId = (bUp ? 0 : 2) + (bLeft ? 0 : 1);
        for (int nTipId = 0; nTipId < m_bubbleTips.Length; nTipId++)
        {
            m_bubbleTips[nTipId].gameObject.SetActive(nTipId == nShownId);
        }
    }

    public void SetElement(int nEltId, bool bIsElt)
    {
        m_itemRenderer.sprite = m_ItemsSprite[nEltId];
        m_itemCross.gameObject.SetActive(!bIsElt);

        float fAngle = Random.Range(-45f, 45f);
        m_itemRenderer.transform.localRotation = Quaternion.Euler(0f, 0f, fAngle);
    }

    public void SetResult(bool bGood)
    {
        if (bGood)
        {
            m_animator.SetTrigger("Good");
        }
        else
        {
            m_animator.SetTrigger("Wrong");
        }
    }

}
