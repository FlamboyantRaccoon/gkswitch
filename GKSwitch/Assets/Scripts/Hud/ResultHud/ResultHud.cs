using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResultHud : MonoBehaviour
{
    [SerializeField]
    private ResultHudRank[] m_ranks;

    public void OnEnable()
    {
        BattleContext battleContext = BattleContext.instance;
        RRInputManager.instance.PushInput(ResultInput);

        int playerCount = BattleContext.instance.playerCount;
        GKPlayerData[] player = new GKPlayerData[playerCount];

        for (int i = 0; i < playerCount; i++)
        {
            player[i] = battleContext.GetPlayer(i);
        }
        System.Array.Sort(player, new PlayerDataComparer());
        for( int i=0; i<m_ranks.Length; i++ )
        {
            if( i>= playerCount )
            {
                m_ranks[i].gameObject.SetActive(false);
            }
            else
            {
                m_ranks[i].gameObject.SetActive(true);
                m_ranks[i].Setup(player[i]);
            }
        }
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(ResultInput);
    }

    private bool ResultInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch (inputActionType)
        {
            case RRInputManager.InputActionType.ButtonRight:
            case RRInputManager.InputActionType.Fire:
                {
                    GameSingleton.instance.gameStateMachine.ChangeState(new MainMenuState());
                }
                break;
        }
        return true;
    }
}
