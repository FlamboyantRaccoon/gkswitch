using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class GameContext : lwSingleton<GameContext>
{
    public ToastyCollection m_toastyCollection;
    public GameSettings m_settings;

    public void Init()
    {
        m_toastyCollection = Resources.Load<ToastyCollection>("Toasties");
        m_settings = Resources.Load<GameSettings>("GameSetting");
    }
   
}
