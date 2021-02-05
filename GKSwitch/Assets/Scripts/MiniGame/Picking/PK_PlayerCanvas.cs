using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PK_PlayerCanvas : MonoBehaviour
{
    [SerializeField]
    private Camera m_camera;

    public PK_Basket m_basket { private set; get; }

    private int m_playerId;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    internal void Setup(int i, float cameraSize, float cameraY )
    {
        m_camera.orthographicSize = cameraSize;
        Vector3 vLocal = m_camera.transform.localPosition;
        vLocal.y = cameraY;
        m_camera.transform.localPosition = vLocal;
        m_playerId = i;
    }

    internal void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        if (m_basket != null)
        {
            UpdateBasketPosition(v);
        }
    }

    internal void Clean()
    {
    }

    internal void SetBasket(PK_Basket basket)
    {
        m_basket = basket;
        UpdateBasketPosition(new Vector2(0.5f, 0f));

    }

    internal void UpdateBasketPosition( Vector2 v )
    {
        Vector3 vInputPos = m_camera.ViewportToWorldPoint(new Vector3(v.x, v.y, 0)); //Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.nearClipPlane));
        vInputPos.y = PK_Picking.fPICKZONEY;
        vInputPos.z = 0f;

        m_basket.transform.position = vInputPos;
    }

    internal void SetCameraRegion(int playerCount)
    {
        if (playerCount == 1)
        {
            return;
        }

        float fWidth = 1f / playerCount;
        float fStartX = m_playerId * fWidth;
        float fStartY = 0;
        float fHeight = 1f;

        m_camera.rect = new Rect(fStartX, fStartY, fWidth, fHeight);
    }
}
