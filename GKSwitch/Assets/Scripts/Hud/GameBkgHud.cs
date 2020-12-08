using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameBkgHud : MonoBehaviour {

    [SerializeField]
    private Image m_bkgImg;
    [SerializeField]
    private Sprite[] m_bkgSprites;

    public void Start()
    {
        UpdateBkg();
    }

    public void UpdateBkg()
    {
        int nSpriteId = 0; // PartyManager.instance.IsParty() ? 1 : 0;
        m_bkgImg.sprite = m_bkgSprites[nSpriteId];
    }

}
