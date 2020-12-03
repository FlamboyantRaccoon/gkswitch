using UnityEngine;
using nn.hid;
using UnityEngine.UI;

// This demo shows how to use motion aim on the Switch in a way similar to how it is done in Splatoon.
// Notes:
// In Splatoon, the camera angle is limited so that it never goes directly above or below the player. This prevents rotation issues that occur
// at those points. Since that is a stylistic choice, this demo does not limit the camera movement in that manner.
// Splatoon and most games with a follow camera also prevent the camera from passing through the terrain. That is beyond the scope of this demo.

public class MotionAimScript : MonoBehaviour
{
    // Camera gimbals
    public Transform gimbalYaxis;
    public Transform gimbalXaxis;

    // joystick and sensors
    struct SensorData
    {
        public float x;
        public float y;
    }
    SensorData previousSensorData; // Change in rotation is calculated by the difference between the current and previous sensor data
    bool[] previousSensorDataSet = new bool[2];
    float joystickMultiple = .00008F; // 8:100,000 ratio of joystick input to camera turn. This number was chosen because it gives smooth movement.
    float sensorMultiple = 2F; //  2:1 ratio of controller turn to camera turn (same as Splatoon). A 2:1 ratio is more comfortable than 1:1 for many cases.
    int joystickThreshold = 3000; // The joystick must return a value greater than this to trigger a response. This is to remove oversensitivity.
    float sensorThreshold = .02F; // Input threshold to remove jitters from sensor fluctuations
    int thresholdForRotationNormalization = 180; // If the rotation in a single turn is greater than 180 degrees, assume the user crossed the 360 degree/0 degree line
                                        // Since the algorithms here are using angles in degrees, we need a way to normalize the difference in sensor values
                                        // should the reading go from a very large number to a very high number, and vice versa. For example, if a reading
                                        // goes from 256 degrees to 2 degrees, this should be understood as a 6 degree difference. 
                                        // This would not be needed if we had a 1:1 ratio for controller rotation to camera rotation.

    // debugging text
    public Text xJoystickAxisText; 
    public Text yJoystickAxisText;
    public Text xSensorAxisText;
    public Text ySensorAxisText;

    //  Create Controllers
    const int numberOfControllers = 2; // 2 controllers = handheld + 1st player (wireless channel) 
    Controller[] controllers = new Controller[numberOfControllers]; 

    void Start()
    {
        Npad.Initialize();

        //  Set supported styles
        //  FullKey = Pro Controller
        //  JoyDual = Two Joy-Con used as one controller
        //  see nn::hid::SetSupportedNpadStyleSet (NpadStyleSet style) in the SDK docs for API details
        Npad.SetSupportedStyleSet(NpadStyle.FullKey | NpadStyle.JoyDual | NpadStyle.Handheld);

        //  NpadJoy.SetHoldType only affects how controllers behave when using a system applet.
        //  NpadJoyHoldType.Vertical is the default setting, but most games would want to use NpadJoyHoldType.Horizontal as it can support all controller styles
        //  If you use NpadJoyHoldType.Vertical,  Npad.SetSupportedStyleSet must only list controller types supported by NpadJoyHoldType.Vertical.
        //  If you don't, the controller applet will fail to load.
        //  Supported types for NpadJoyHoldType.Vertical are: NpadStyle.JoyLeft, NpadStyle.JoyRight
        NpadJoy.SetHoldType(NpadJoyHoldType.Horizontal);

        NpadJoy.SetHandheldActivationMode(NpadHandheldActivationMode.Dual); // both controllers must be docked for handheld mode.

        // You must call Npad.SetSupportedIdType for all supported controllers.
        // Your game may run if you don't call this, but not calling this may lead to crashes in some circumstances.
        NpadId[] npadIds = { NpadId.Handheld, NpadId.No1 };
        Npad.SetSupportedIdType(npadIds);

        // create controllers used in the demo
        for (int i = 0; i < numberOfControllers; i++)
        {
            controllers[i] = ScriptableObject.CreateInstance<Controller>();
            previousSensorDataSet[i] = false; // previous sensor data is not valid until it can first be read
        }
        controllers[0].initController(NpadId.Handheld);
        controllers[1].initController(NpadId.No1);

        callControllerApplet();

    }

    void Update()
    {
#if UNITY_SWITCH
        //  Get updates from each controller
        for (int i = 0; i < numberOfControllers; i++)
        {
            controllers[i].updateState();
            //  Show the sensor values, and update the models on screen
            controlCamera(controllers[i], i);
        }
#endif
    }

    void callControllerApplet()
    {
#if UNITY_SWITCH && !UNITY_EDITOR

        //  set the arguments for the applet
        //  see nn::hid::ControllerSupportArg::SetDefault () in the SDK documentation for details
        ControllerSupportArg controllerSupportArgs = new ControllerSupportArg();
        controllerSupportArgs.SetDefault();
        controllerSupportArgs.playerCountMax = 1;
        controllerSupportArgs.playerCountMin = 0; // must be set to 0 if you want to allow someone to play in handheld mode only

        nn.hid.ControllerSupport.Show(controllerSupportArgs);
#endif

    }

    // Apply threshold and multiple to joystick data
    float cleanJoystickInput(int input)
    {
        if (Mathf.Abs(input) > joystickThreshold)
        {
            return (input * joystickMultiple);
        }
        return (0F);
    }

    // Normalize sensor data if it crosses the 360/0 degree threshold
    float normalizeSensorData(float previousValue, float currentValue)
    {
        float output = currentValue - previousValue;
        if (Mathf.Abs(output) > sensorThreshold)
        {
            if (output < (thresholdForRotationNormalization * -1))
            {
                output += 360;
            }
            else if (output > thresholdForRotationNormalization)
            {
                output = 360 - output;
            }
            return (output * sensorMultiple);
        }

        return (0);
    }

    // put sensor data in correct format for algorithm, and normalize it
    Vector3 cleanSensorInput(Vector3 input)
    {
        Vector3 output = new Vector3(0, 0, 0);
        output.x = normalizeSensorData(previousSensorData.x, input.x);
        output.y = normalizeSensorData(previousSensorData.y, input.y);        
        return (output);
    }

    // put previous sensor data into a buffer
    void setPreviousSensorData(Vector3 sensorData)
    {
        previousSensorData.x = sensorData.x;
        previousSensorData.y = sensorData.y;
    }
    void outputDebugText(Controller controller)
    {
        xJoystickAxisText.text = controller.State.analogStickL.x.ToString();
        yJoystickAxisText.text = controller.State.analogStickL.y.ToString();
        xSensorAxisText.text = controller.rawSensorRotation[0].eulerAngles.x.ToString();
        ySensorAxisText.text = controller.rawSensorRotation[0].eulerAngles.y.ToString();
    }

    // apply controller input to the game models, camera angle, and debug text
    void controlCamera(Controller controller, int controllerNumber)
    {
        // if a controller is disconnected or invalid, flag the previous sensor data buffer as false, and exit this function
        if (controller.Style == NpadStyle.None || controller.Style == NpadStyle.Invalid)
        {
            previousSensorDataSet[controllerNumber] = false;
            return;
        }

        // always make sure you have valid previous values from the sensors
        if (previousSensorDataSet[controllerNumber] == false)
        {
            setPreviousSensorData(controller.rawSensorRotation[0].eulerAngles);
            previousSensorDataSet[controllerNumber] = true;
            return;
        }


        // Rotate camera based on joystick input
        // (joystick X axis input corresponds to rotation of the camera on the Y axis, and vice versa)
        gimbalYaxis.Rotate(new Vector3(0, cleanJoystickInput(controller.State.analogStickL.x), 0));
        gimbalXaxis.Rotate(new Vector3(cleanJoystickInput(controller.State.analogStickL.y), 0, 0));

        // Rotate camera based on sensor input
        Vector3 cleanedSensorData = cleanSensorInput(controller.correctedRotation[0].eulerAngles);
        setPreviousSensorData(controller.rawSensorRotation[0].eulerAngles);
        gimbalYaxis.Rotate(new Vector3(0, cleanedSensorData.y, 0)); 
        gimbalXaxis.Rotate(new Vector3(cleanedSensorData.x, 0, 0)); 
        

        // Recenter the camera if Y is pressed
        if (controller.State.GetButtonDown(NpadButton.Y))
        {
            gimbalXaxis.localRotation = new Quaternion (0F, 0F, 0F, 0F);
            for (int i = 0; i < numberOfControllers; i++)
            {
                controllers[i].setBaseRotation(i);
            }
        }

        //  Call the controller app if either + or - is pressed
        if ((controller.State.GetButtonDown(NpadButton.Minus)) || (controller.State.GetButtonDown(NpadButton.Plus)))
        {
            callControllerApplet();
        }

        //output sensor and joystick values to screen for debug
        outputDebugText(controller);
    }
}
