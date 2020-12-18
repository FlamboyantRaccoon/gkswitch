using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class InterRoundHud : MonoBehaviour
{
    [SerializeField]
    private TMP_Text m_titleLabel;

    [SerializeField]
    private Transform m_playerRoot;
    [SerializeField]
    private InterRoundPlayerBoard m_playerBoardPrefab;

    public void OnEnable()
    {
        lwTools.DestroyAllChildren(m_playerRoot.gameObject);
        BattleContext battleContext = BattleContext.instance;

        RRInputManager.instance.PushInput(InterRoundInput);

        // title
        string label = "ROUND " + battleContext.currentRound.ToString() + " / " + battleContext.totalRound.ToString();
        m_titleLabel.text = label;

        int playerCount = BattleContext.instance.playerCount;
        for( int i=0; i<playerCount; i++ )
        {
            InterRoundPlayerBoard playerBoard = GameObject.Instantiate<InterRoundPlayerBoard>(m_playerBoardPrefab, m_playerRoot);
            RectTransform rt = playerBoard.GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(0f, -i * 200f - 20f);
            playerBoard.Setup(BattleContext.instance.GetPlayer(i));
        }
    }

    public void OnDisable()
    {
        RRInputManager.RemoveInputSafe(InterRoundInput);
    }

    private bool InterRoundInput(RRInputManager.InputActionType inputActionType, RRInputManager.MoveDirection moveDirection)
    {
        switch( inputActionType )
        {
            case RRInputManager.InputActionType.ButtonRight:
            case RRInputManager.InputActionType.Fire:
                {
                    GameSingleton.instance.gameStateMachine.ChangeState(new MiniGameState());
                }
                break;
        }
        return true;
    }
}
