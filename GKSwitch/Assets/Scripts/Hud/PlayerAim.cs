using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAim : MonoBehaviour
{
    private RectTransform m_rt;

    // Start is called before the first frame update
    void Awake()
    {
        m_rt = GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void SetPosition(Vector2 vector2)
    {
        m_rt.anchoredPosition = vector2;
    }
}
