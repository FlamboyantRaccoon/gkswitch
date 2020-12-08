using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SelectableToasty : MonoBehaviour
{
    [SerializeField]
    private Image m_avatar;

    public ToastyData m_toastyData { private set; get; }

    internal void Setup(ToastyData toastyData)
    {
        m_toastyData = toastyData;
        m_avatar.sprite = toastyData.avatar;
    }
}
