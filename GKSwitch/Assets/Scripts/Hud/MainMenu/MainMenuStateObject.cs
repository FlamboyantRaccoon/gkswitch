using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MainMenuStateObject : MonoBehaviour
{
    public System.Action<MainMenuHud.MainMenuState> m_changeMenuStateDlg;

    public virtual void Setup()
    {

    }

    public virtual void Clean()
    {

    }

    protected void ChangeMainMenuState(MainMenuHud.MainMenuState mainMenuState)
    {
        m_changeMenuStateDlg?.Invoke(mainMenuState);
    }
}
