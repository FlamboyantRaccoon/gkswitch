using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GenericTransitionHud : MonoBehaviour
{
    private Animator m_animator;
    System.Action m_OnEndTransitionIn;
    System.Action m_OnEndTransitionOut;

    // Use this for initialization
    void Awake ()
    {
        m_animator = GetComponent<Animator>();
        Canvas canvas = GetComponent<Canvas>();
        canvas.overrideSorting = true;
        canvas.sortingOrder = 300;
	}

    public void Reset()
    {
        m_OnEndTransitionIn = null;
        m_OnEndTransitionOut = null;
        gameObject.SetActive(false);
        /*if( !m_animator.GetBool("TransitionActive"))
        {
            m_animator.SetBool("TransitionActive", true);
            m_animator.Play("Transition_idle");
        }*/
    }

    public void StartTransitionIn(System.Action action)
    {
        m_animator.SetBool("TransitionActive", true);
        m_OnEndTransitionIn = action;
    }

    public void StartTransitionOut(System.Action action)
    {
        m_OnEndTransitionOut = action;
        m_animator.SetBool("TransitionActive", false );

    }

    public void OnEndTransitionIn()
    {
        if (m_OnEndTransitionIn != null)
        {
            m_OnEndTransitionIn();
        }
    }

    public void OnEndTransitionOut()
    {
        if (m_OnEndTransitionOut != null)
        {
            m_OnEndTransitionOut();
        }

        gameObject.SetActive(false);
    }

}
