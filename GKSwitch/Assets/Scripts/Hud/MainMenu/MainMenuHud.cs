using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuHud : MonoBehaviour
{
    public enum MainMenuState {  none, mainSelection, ToastySelection, RulesSettings, FreeMiniGameSelection }

    [Serializable] private class MainMenuStateObjects : lwEnumArray<MainMenuState, MainMenuStateObject> { };

    [SerializeField]
    private MainMenuStateObjects mainMenuStateObjects;

    private MainMenuState m_currentState;

    public void Start()
    {
        for( int i=0; i<mainMenuStateObjects.nLength; i++ )
        {
            if(mainMenuStateObjects[i]!=null )
            {
                mainMenuStateObjects[i].m_changeMenuStateDlg = ChangeMenuState;
            }
        }
    }

    private void ChangeMenuState(MainMenuState state )
    {
        if( state==m_currentState )
        {
            return;
        }
        if(mainMenuStateObjects[m_currentState]!=null )
        {
            mainMenuStateObjects[m_currentState].Clean();
        }
        m_currentState = state;
        int nSelect = (int)state;
        for (int i = 0; i < mainMenuStateObjects.nLength; i++)
        {
            if(mainMenuStateObjects[i]==null )
            {
                continue;
            }
            bool bSelect = i == nSelect;
            mainMenuStateObjects[i].gameObject.SetActive( bSelect );
            if( bSelect )
            {
                mainMenuStateObjects[i].Setup();
            }
        }
    }

    public void OnEnable()
    {
        m_currentState = MainMenuState.none;
        ChangeMenuState(MainMenuState.mainSelection);
    }

    public void OnDisable()
    {
    }

   
}
