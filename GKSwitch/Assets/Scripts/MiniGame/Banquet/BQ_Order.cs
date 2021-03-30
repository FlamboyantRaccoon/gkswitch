using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_Order
{
    public int colorId { get { return m_nColorId; } }
    public int eltId { get { return m_nEltId; } }
    public int shownEltId { get { return m_nShownEltId; } }
    public bool isElt { get { return m_bIsElement; } }

    private int m_nEltId;
    private int m_nShownEltId;
    private int m_nColorId;
    private bool m_bIsElement;

    private BQ_OrderView m_orderView;
    private BQ_OrderBubble m_orderBubble;

    public BQ_Order(int nEltId, int nColorId, bool bIsElement, int nShownEltId)
    {
        m_nEltId = nEltId;
        m_nColorId = nColorId;
        m_bIsElement = bIsElement;
        m_nShownEltId = nShownEltId;
    }

    /// <summary>
    /// Check if the meal complete order condition
    /// </summary>
    /// <param name="nItemId"></param>
    /// <param name="nColorId"></param>
    /// <returns></returns>
    public bool IsMealValid(int nItemId, int nColorId)
    {
        // TODO change this part, and use OrderSpawn one
        bool bElt = m_bIsElement ? m_nEltId == nItemId : m_nEltId != nItemId;
        bool bColor = m_nColorId == -1 || nColorId == m_nColorId;

        return bElt && bColor;
    }
}
