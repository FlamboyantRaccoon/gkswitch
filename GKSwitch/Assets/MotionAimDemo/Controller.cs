using UnityEngine;
using nn.hid;

public class Controller : ScriptableObject
{
    private NpadId npadId; // ID of the controller
    private NpadState npadState = new NpadState(); // All states other than the six axis sensor states (buttons, etc.)
    private NpadStyle npadStyle = NpadStyle.Invalid; // Handlheld, JoyDual, etc.
    public NpadStyle Style { get { return npadStyle; } }
    public NpadState State { get { return npadState; } }
    public NpadId Id { get { return npadId; } }

    // Six axis sensor vars
    const int maxNumberOfhandles = 2; // A controller can have up to 2 handles (two joy-con) for the six axis sensor
    private SixAxisSensorHandle[] sixAxisHandle = new SixAxisSensorHandle[maxNumberOfhandles];
    public int sixAxisHandleCount; // Count of how many six axis handles are used (1 or 2)
    public SixAxisSensorState[] sixAxisState = new SixAxisSensorState[maxNumberOfhandles]; // With two handles, the controller would have two states    
    public Quaternion[] baseRotation = new Quaternion[maxNumberOfhandles]; // The controller position that serves as a base '0' for the input      
    public Quaternion[] rawSensorRotation = new Quaternion[maxNumberOfhandles]; // The raw values output by the controller      
    public Quaternion[] correctedRotation = new Quaternion[maxNumberOfhandles]; // The calibrated rotation: the sensor values quaternion in relation to the zero point   
    public bool sixAxisEnabled = false;
    private bool sixAxisShouldBeEnabled = false;


    /// <summary>
    /// Initializer - Must be called
    /// </summary>
    /// <param name="newNpadId"> NpadID of controller being connected </param>
    /// <param name="useSixAxis"> Start controller with six axis sensor enabled </param>
    public void initController(NpadId newNpadId, bool useSixAxis = true)
    {
        npadId = newNpadId;
        npadStyle = Npad.GetStyleSet(npadId);

        for (int i = 0; i < maxNumberOfhandles; i++)
        {
            sixAxisState[i] = new SixAxisSensorState();
            baseRotation[i] = Quaternion.identity;
            rawSensorRotation[i] = Quaternion.identity;
        }

        sixAxisShouldBeEnabled = useSixAxis;
        if (sixAxisShouldBeEnabled && npadStyle != NpadStyle.None && npadStyle != NpadStyle.Invalid)
        {
            enableSixAxis();
        }

    }


    /// <summary>
    /// Updates the states of the controller
    /// Call this each frame
    /// </summary>
    public void updateState()
    {

        NpadStyle previousStyle = npadStyle;

        // You must get the style each frame to make sure the controller was not disconnected
        npadStyle = Npad.GetStyleSet(npadId);
        if (npadStyle == NpadStyle.None || npadStyle == NpadStyle.Invalid) //if disconnected, return
        {
            //del this npadState = new NpadState();
            return;
        }
        // If using six axis sensors, if the style has changed (e.g. Joy Right -> Joy Dual), or it is otherwise not enabled, enable it. 
        if (sixAxisShouldBeEnabled && (previousStyle != npadStyle || sixAxisEnabled == false))
        {
            enableSixAxis();
        }

        Npad.GetState(ref npadState, npadId, npadStyle); //Get button/joystick state
        // Get sensor state for each handle
        if (sixAxisEnabled)
        {
            nn.util.Float4 float4TempSensorData = new nn.util.Float4(); // Needed for GetQuaternion
            for (int i = 0; i < sixAxisHandleCount; i++)
            {
                SixAxisSensor.GetState(ref sixAxisState[i], sixAxisHandle[i]);
                sixAxisState[i].GetQuaternion(ref float4TempSensorData);

                // Convert the Float4 Quaternion into Unity's Quaternion representation
                rawSensorRotation[i] = new Quaternion(float4TempSensorData.x, float4TempSensorData.z, float4TempSensorData.y, float4TempSensorData.w * -1F);
                // Get the rotation in relation to the base rotation
                correctedRotation[i] = Quaternion.Inverse(baseRotation[i]) * rawSensorRotation[i];
            }
        }
    }



    /// <summary>
    /// Enable the six axis sensors, and initialize the handling of the six axis sensor states/handles
    /// </summary>
    public void enableSixAxis()
    {

        // GetHandles([out]controller handle array, max number of handles (2), NpadId, NpadStyle);
        //see nn::hid::GetSixAxisSensorHandles in the API for more details
        sixAxisHandleCount = SixAxisSensor.GetHandles(sixAxisHandle, maxNumberOfhandles, npadId, npadStyle);
        //start the sensor for each handle
        for (int i = 0; i < sixAxisHandleCount; i++)
        {
            SixAxisSensor.Start(sixAxisHandle[i]);
        }

        sixAxisEnabled = true;

    }

    /// <summary>
    /// Turn off the six axis sensors on all handles.
    /// This should be done when not use as it improves battery life.
    /// </summary>
    public void disableSixAxis()
    {
        for (int i = 0; i < sixAxisHandleCount; i++)
        {
            SixAxisSensor.Stop(sixAxisHandle[i]);
        }
        sixAxisEnabled = false;
    }

    public void setBaseRotation(int handleIndex)
    {
        baseRotation[handleIndex] = rawSensorRotation[handleIndex];
    }
}
