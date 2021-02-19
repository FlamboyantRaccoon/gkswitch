using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExpressionsHud : MiniGameBasicHud
{
    [SerializeField]
    private ExpressionSlot m_slotPrefab;
    [SerializeField]
    private Transform m_slotRoot;

    private ExpressionSlot[] m_slots;
    private bool m_bIsSet = false;

    public void Setup(ushort[] nExpressionMask, Sprite[][] spritesArray, int nPointsWin )
    {
        if (!m_bIsSet)
        {
            int nCount = nExpressionMask.Length;
            m_slots = new ExpressionSlot[nCount];
            for (int nSlotId = 0; nSlotId < nCount; nSlotId++)
            {
                m_slots[nSlotId] = GameObject.Instantiate<ExpressionSlot>(m_slotPrefab, m_slotRoot);
                m_slots[nSlotId].Setup(nExpressionMask[nSlotId], spritesArray[nSlotId], nPointsWin);
            }
            m_bIsSet = true;
        }

    }

    public void PlaySpotGoodAnim( int nExpressionMask)
    {
        int nSlotId = 0;
        bool bFound = false;
        while( !bFound && nSlotId<m_slots.Length )
        {
            if( m_slots[nSlotId].nExpressionMask==nExpressionMask )
            {
                bFound = true;
            }
            else
            {
                nSlotId++;
            }
        }

        Debug.Assert(bFound);
        if( bFound )
        {
            m_slots[nSlotId].PlayGoodAnim();
        }
    }

    public override void Exit()
    {
        base.Exit();

        if (m_slots != null)
        {
            for (int i = 0; i < m_slots.Length; i++)
            {
                GameObject.Destroy(m_slots[i].gameObject);
            }
            m_slots = null;
        }
    }
}
