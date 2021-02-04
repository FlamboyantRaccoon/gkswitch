using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_Belt : MonoBehaviour
{
    private Material m_mainMaterial;
    public float m_fSpeed;


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

}
