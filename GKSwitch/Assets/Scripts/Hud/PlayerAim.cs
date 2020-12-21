using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerAim : MonoBehaviour
{
    [SerializeField]
    private Image m_image;

    private RectTransform m_rt;


    // Start is called before the first frame update
    void Awake()
    {
        m_rt = GetComponent<RectTransform>();
    }

    public void Setup( int playerId )
    {
        m_image.color = GameContext.instance.m_settings.playerSettings[playerId].color;
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
