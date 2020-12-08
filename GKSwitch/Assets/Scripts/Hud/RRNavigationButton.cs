using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class RRNavigationButton : MonoBehaviour
{
    [System.Serializable] private class NavigationWayButton : lwEnumArray<RRInputManager.MoveDirection, RRNavigationButton> { }; // dummy definition to use Unity serialization

    [Header("Navigation")]
    [SerializeField]
    private NavigationWayButton m_navigationsWays;

    public Button m_actionButton = null;

    public static int SelectNextItemValid<T>(IList<T> list, int currentItem, int increment) where T : RRNavigationButton
    {
        if( list==null || list.Count==0 )
        {
            return -1;
        }

        if( currentItem>= list.Count )
        {
            currentItem = list.Count - 1;
        }

        if (increment == 0)
        {
            if ( list[currentItem].IsSelectable())
            {
                return currentItem;
            }
            increment = 1;
        }

        bool found = false;
        int tmp = currentItem + increment;
        while (!found && tmp < list.Count && tmp >= 0)
        {
            if (list[tmp].IsSelectable())
            {
                found = true;
            }
            else
            {
                tmp += increment;
            }
        }

        if (found)
        {
            return tmp;
        }
        if (list[currentItem].IsSelectable())
        {
            return currentItem;
        }
        return -1;
    }


    public virtual bool IsSelectable()
    {
        if(m_actionButton!=null )
        {
            return m_actionButton.interactable;
        }
        return true;
    }

    public virtual void Select()
    {
        UnityEngine.EventSystems.EventSystem.current.SetSelectedGameObject(gameObject);
    }

    public void SimulatePress()
    {
        var pointer = new PointerEventData(EventSystem.current); // pointer event for Execute
        ExecuteEvents.Execute(m_actionButton.gameObject, pointer, ExecuteEvents.submitHandler);
    }

    public RRNavigationButton SelectNext( RRInputManager.MoveDirection direction )
    {
        RRNavigationButton button = m_navigationsWays[direction];
        while( button !=null && !button.IsSelectable())
        {
            button = button.m_navigationsWays[direction];
        }
        return button;
    }
}
