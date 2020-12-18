using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;


#if UNITY_SWITCH
using nn.hid;
#endif

public class RRInputManager : lwSingletonMonoBehaviour<RRInputManager>
{
    public enum MoveDirection { none, left, top, right, bottom }

    #if UNITY_SWITCH
    private NpadId[] npadIds = { NpadId.No1, NpadId.No2, NpadId.No3, NpadId.No4 };
    private NpadId npadId = NpadId.Invalid;
    private NpadStyle npadStyle = NpadStyle.Invalid;
    private NpadState npadState = new NpadState();
    private ControllerSupportArg controllerSupportArg = new ControllerSupportArg();
    private nn.Result result = new nn.Result();
    
    private Dictionary<NpadId, RRPlayerInput> m_playerInputPadDico = new Dictionary<NpadId, RRPlayerInput>();
#endif

    public delegate bool ManageInputDelegate(InputActionType inputActionType, MoveDirection moveDirection = MoveDirection.none );

    public GameObject playerInputPrefab = null;

    public enum InputActionType { Move, ButtonRight, Fire }
    public enum InputType { Touch, Controller }

    public static InputType m_lastInputType = InputType.Touch;


    public List<RRPlayerInput> playerList { get { return m_playerInputs; } }

    private Stack<ManageInputDelegate> m_inputStack = new Stack<ManageInputDelegate>();
    private List<RRPlayerInput> m_playerInputs = new List<RRPlayerInput>();


    public static void RemoveInputSafe(ManageInputDelegate inputDelegate)
    {
        if (RRInputManager.IsInstanceValid())
        {
            RRInputManager.instance.RemoveInput(inputDelegate);
        }
    }

    public List<RRPlayerInput> GetPlayerInputs()
    {
        return m_playerInputs;
    }

    public int AddPlayerInput(RRPlayerInput playerInput)
    {
        m_playerInputs.Add(playerInput);
        return m_playerInputs.Count - 1;
    }

    #region unityEvent
    public void Move(InputAction.CallbackContext context)
    {
        MoveDirection moveDirection = MoveDirection.none;
        if (context.started)
        {
            Vector2 vector2 = context.ReadValue<Vector2>();
            if (vector2 == null)
            {
                Debug.LogError("My Vector is null");
                return;
            }
            if (vector2.x <= -0.5f)
            {
                ManageInput(InputActionType.Move, MoveDirection.left );
            }
            else if (vector2.x >= 0.5f)
            {
                ManageInput(InputActionType.Move, MoveDirection.right);
            }

            if (vector2.y <= -0.5f)
            {
                ManageInput(InputActionType.Move, MoveDirection.bottom);
            }
            else if (vector2.y >= 0.5f)
            {
                ManageInput(InputActionType.Move, MoveDirection.top);
            }


        }
    }

    public IEnumerator InitInput()
    {
#if UNITY_SWITCH
        Npad.Initialize();
        Npad.SetSupportedIdType(npadIds);
        NpadJoy.SetHoldType(NpadJoyHoldType.Vertical);

        Npad.SetSupportedStyleSet(
           NpadStyle.JoyLeft | NpadStyle.JoyRight);
        ShowControllerSupport();

#endif
        bool bInit = false;
        while( !bInit )
        {
#if UNITY_EDITOR
            PlayerInput keyboardPlayer = PlayerInput.Instantiate(playerInputPrefab, 0, "Keyboard", 0, Keyboard.current );
            Gamepad[] pads = Gamepad.all.ToArray();
            if( pads!=null && pads.Length > 0 )
            {
                for( int i=0; i<pads.Length; i++ )
                {
                    PlayerInput player = PlayerInput.Instantiate(playerInputPrefab, i+1, "GamePad", i+1, pads[i]);
                }
            }
            else
#endif
            {
                yield return null;
            }
            bInit = true;
        }
        GameSingleton.instance.gameStateMachine.ChangeState(new GameLogoState());
    }

#if UNITY_SWITCH
    protected override void Update()
    {
        /*System.Text.StringBuilder stringBuilder = new System.Text.StringBuilder();
        stringBuilder.Length = 0;

        if (UpdatePadState())
        {
            if (npadState.GetButtonDown(NpadButton.A))
            {
                Debug.Log("NpadButton.A Down");
            }
            else if (npadState.GetButtonUp(NpadButton.A))
            {
                Debug.Log("NpadButton.A Up");
            }
        }*/

    }

    private bool UpdatePadState()
    {
        NpadStyle handheldStyle = Npad.GetStyleSet(NpadId.Handheld);
        NpadState handheldState = npadState;
        if (handheldStyle != NpadStyle.None)
        {
            Npad.GetState(ref handheldState, NpadId.Handheld, handheldStyle);
            if (handheldState.buttons != NpadButton.None)
            {
                npadId = NpadId.Handheld;
                npadStyle = handheldStyle;
                npadState = handheldState;
                return true;
            }
        }

        NpadStyle no1Style = Npad.GetStyleSet(NpadId.No1);
        NpadState no1State = npadState;
        if (no1Style != NpadStyle.None)
        {
            Npad.GetState(ref no1State, NpadId.No1, no1Style);
            if (no1State.buttons != NpadButton.None)
            {
                npadId = NpadId.No1;
                npadStyle = no1Style;
                npadState = no1State;
                return true;
            }
        }

        if ((npadId == NpadId.Handheld) && (handheldStyle != NpadStyle.None))
        {
            npadId = NpadId.Handheld;
            npadStyle = handheldStyle;
            npadState = handheldState;
        }
        else if ((npadId == NpadId.No1) && (no1Style != NpadStyle.None))
        {
            npadId = NpadId.No1;
            npadStyle = no1Style;
            npadState = no1State;
        }
        else
        {
            npadId = NpadId.Invalid;
            npadStyle = NpadStyle.Invalid;
            npadState.Clear();
            return false;
        }
        return true;
    }
#endif
    public void Manageinput(InputAction.CallbackContext context)
    {
        InputActionType inputActionType = lwParseTools.ParseEnumSafe<InputActionType>(context.action.name, InputActionType.Move);
        Debug.Assert(inputActionType != InputActionType.Move, "Invalid Name for input action : " + context.action.name);
        //Debug.Log("Manage input : " + inputActionType);
        if (context.started)
        {
            ManageInput(inputActionType);
        }
    }
    #endregion


    public void PushInput(ManageInputDelegate inputDelegate)
    {
        m_inputStack.Push(inputDelegate);
    }

    public void RemoveInput(ManageInputDelegate inputDelegate)
    {
        Stack<ManageInputDelegate> tempory = new Stack<ManageInputDelegate>();
        bool inputFound = false;
        while (!inputFound && m_inputStack.Count > 0)
        {
            ManageInputDelegate current = m_inputStack.Pop();
            if (current == inputDelegate)
            {
                inputFound = true;
            }
            else
            {
                tempory.Push(current);
            }
        }

        while (tempory.Count > 0)
        {
            ManageInputDelegate current = tempory.Pop();
            m_inputStack.Push(current);
        }
    }


    public void ManageInput(InputActionType inputActionType, MoveDirection direction = MoveDirection.none )
    {
        Stack<ManageInputDelegate> tempory = new Stack<ManageInputDelegate>(new Stack<ManageInputDelegate>(m_inputStack));
        bool inputManaged = false;
        while (!inputManaged && tempory.Count > 0)
        {
            ManageInputDelegate current = tempory.Pop();
            inputManaged = current(inputActionType, direction);
        }
    }

#if UNITY_SWITCH
    void ShowControllerSupport()
    {
        controllerSupportArg.SetDefault();
        controllerSupportArg.playerCountMax = (byte)(npadIds.Length);
        
        controllerSupportArg.enableIdentificationColor = false;
        controllerSupportArg.enableExplainText = false;

        Debug.Log(controllerSupportArg);
        result = ControllerSupport.Show(controllerSupportArg);
        Debug.Log("result.IsSuccess : " + result.IsSuccess());

        if (!result.IsSuccess()) { Debug.Log(result); }

        List<InputDevice> devices = new List<InputDevice>(InputSystem.devices);

        for (int i = 0; i < npadIds.Length; i++)
        {
            NpadId npadId = npadIds[i];
            NpadStyle npadStyle = Npad.GetStyleSet(npadId);
 
            Debug.Log("npadStyle " + npadStyle);
            if (npadStyle == NpadStyle.None) { continue; }
            
            PlayerInput player = PlayerInput.Instantiate(playerInputPrefab, 10+i, "SwitchPro");
            RRPlayerInput playerInput = player.GetComponent<RRPlayerInput>();
            playerInput.nPadId = npadId;
            playerInput.CreateSwitchAiming();
            m_playerInputPadDico.Add(npadId, playerInput);
        }
        Debug.Log("input switch ");
    }
#endif
}
