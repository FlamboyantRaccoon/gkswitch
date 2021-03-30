using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_Belt : MonoBehaviour
{
    public bool m_bVertical = true;
    public bool m_bIncrease = false;
    public Vector2 m_vStartDelta = new Vector2(0f, 0f);
    public Vector2 m_vSizeDelta = new Vector2(0f, 0f);

    private Material m_mainMaterial;
    public float m_fSpeed;

    public Vector3 ComputeStartPos()
    {
        float startDelta = UnityEngine.Random.Range(m_vStartDelta.x, m_vStartDelta.y);
        float startConst = m_vSizeDelta.x;

        if( m_bVertical )
        {
            return new Vector3(startDelta, startConst, 0f);
        }
        return new Vector3(startConst, startDelta, 0f);
    }

    public void Awake()
    {
        m_mainMaterial = GetComponent<MeshRenderer>().material;
    }

    public void SetSpeed(float fSpeed)
    {
        if (m_mainMaterial == null)
        {
            m_mainMaterial = GetComponent<MeshRenderer>().material;
        }
        m_fSpeed = fSpeed / (m_mainMaterial.mainTexture.height);
    }

    private void Update()
    {
        if (m_fSpeed != 0f)
        {
            Vector2 vOffset = m_mainMaterial.mainTextureOffset;
            vOffset.y -= m_fSpeed * Time.deltaTime;
            m_mainMaterial.mainTextureOffset = vOffset;
        }
    }

    internal bool IsMealOnBelt(Vector3 vPos)
    {
        float startSize = m_bIncrease ? m_vSizeDelta.x : m_vSizeDelta.y;
        float endSize = m_bIncrease ? m_vSizeDelta.y : m_vSizeDelta.x;


        if ( m_bVertical )
        {
            return vPos.x >= m_vStartDelta.x && vPos.x <= m_vStartDelta.x &&
                vPos.y >= startSize && vPos.y <= endSize;
        }
        else
        {
            return vPos.y >= m_vStartDelta.x && vPos.y <= m_vStartDelta.y &&
                vPos.x >= startSize && vPos.x <= endSize;

        }
    }
}
