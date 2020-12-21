using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGamePlayerCanvas : MonoBehaviour
{
    [SerializeField]
    protected Camera m_camera;

    protected int m_playerId;


    public virtual void Init(int playerId)
    {
        m_playerId = playerId;
    }

    public virtual void Clean()
    {
    }

    public void SetCameraRegion(int playerCount)
    {
        if (playerCount == 1)
        {
            return;
        }
        float fStartX = (m_playerId % 2) * 0.5f;
        float fWidth = 0.5f;
        float fStartY = 0;
        float fHeight = 1f;

        if (playerCount > 2)
        {
            fHeight = 0.5f;
            fStartY = (1 - ((int)(m_playerId / 2))) * 0.5f;
        }
        m_camera.rect = new Rect(fStartX, fStartY, fWidth, fHeight);
    }
}
