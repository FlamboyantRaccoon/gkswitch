using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerSelectionBoard : MonoBehaviour
{
    [SerializeField]
    private Image m_avatar;
    [SerializeField]
    private Image m_plate;
    [SerializeField]
    private GameObject m_tild;

    public void Setup( Color c )
    {
        m_plate.color = c;
        m_tild.SetActive(false);
    }

    public void SetAvatar( ToastyData toastyData )
    {
        m_avatar.sprite = toastyData.avatar;
    }

    internal void Validate()
    {
        m_tild.SetActive(true);
    }
}
