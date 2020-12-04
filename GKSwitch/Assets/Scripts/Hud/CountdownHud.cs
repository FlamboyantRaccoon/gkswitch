using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CountdownHud : MonoBehaviour
{
    //    public System.Action onEndCountdownAnimation { set { m_OnEndCountdownAnimation = value; } }
    public System.Action onEndIntroAnimation { set { m_OnEndIntroAnimation = value; } }


    System.Action m_OnEndIntroAnimation;
    System.Action m_OnEndCountdownAnimation;
    System.Action m_OnEndOutroAnimation;

    private Animator m_animator;


    private void Awake()
    {
        m_animator = GetComponent<Animator>();
    }

    public void PlayIntro()
    {
        m_animator.SetTrigger("Start");
    }

    public void StartCountdown( System.Action action )
    {
        m_OnEndCountdownAnimation = action;
        m_animator.SetTrigger("Ready");
    }

    public void StartFinalAnimation(System.Action action)
    {
        m_OnEndOutroAnimation = action;
        m_animator.SetTrigger("End");

    }

    public void OnEndIntro()
    {
        if (m_OnEndIntroAnimation != null)
        {
            m_OnEndIntroAnimation();
        }
    }

    public void OnEndCountdown()
    {
        if( m_OnEndCountdownAnimation!=null )
        {
            m_OnEndCountdownAnimation();
        }
    }

    public void OnEndOutro()
    {
        if (m_OnEndOutroAnimation != null)
        {
            m_OnEndOutroAnimation();
        }
        gameObject.SetActive(false);
    }
}
