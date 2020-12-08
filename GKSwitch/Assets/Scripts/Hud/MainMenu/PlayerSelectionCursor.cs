using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionCursor : MonoBehaviour
{
    [SerializeField]
    private Image m_image;

    private RectTransform m_rt;


    // Start is called before the first frame update
    void Awake()
    {
        m_rt = GetComponent<RectTransform>();
    }

    public void Setup( Color c )
    {
        m_image.color = c;
    }

    internal void SetPosition(float v1, float v2)
    {
        m_rt.anchoredPosition = new Vector2(v1, v2);
    }
}
