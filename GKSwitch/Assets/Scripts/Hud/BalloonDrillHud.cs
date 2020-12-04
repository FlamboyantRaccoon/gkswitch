using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BalloonDrillHud : MiniGameBasicHud
{
    [SerializeField]
    private Image m_objectiveImage;

    public void SetObjective( Sprite objSprite )
    {
        Debug.Assert(m_objectiveImage != null);
        if( objSprite==null )
        {
            m_objectiveImage.gameObject.SetActive(false);
        }
        else
        {
            m_objectiveImage.gameObject.SetActive(true);
            m_objectiveImage.sprite = objSprite;
        }
    }
}
