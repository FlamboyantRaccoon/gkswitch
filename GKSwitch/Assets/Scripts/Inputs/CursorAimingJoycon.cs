using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if UNITY_SWITCH
    using nn.hid;
#endif

public class CursorAimingJoycon : CursorAiming
{
    private const string RAYCAST_LAYER = "RaycastQuad_";
    Vector2 m_position = Vector2.zero;
    private float m_semiAmplitudeX = 30f;
    private float m_semiAmplitudeY = 20f;
    private float m_tvDistance = 1000f;

#if UNITY_SWITCH
    private nn.hid.NpadId nPadId;

    private Quaternion m_referenceQuaternion;

    NpadStyle nStyle;
    NpadState nState;

    private GameObject debugCube;
    private GameObject debugpoint;
    private float calibrationTimer = -1f;

    private SixAxisSensorHandle[] handle = new SixAxisSensorHandle[2];
    private SixAxisSensorState state = new SixAxisSensorState();
    private int handleCount = 0;
    private Quaternion m_rawQuaternion;
    private nn.util.Float4 npadQuaternion = new nn.util.Float4();
    private RRPlayerInput playerInput;

    private GameObject m_projectionQuad;

    private bool m_bOnFire = false;

    public void SetNpadId( nn.hid.NpadId id )
    {
        nPadId = id;
        nStyle = Npad.GetStyleSet(nPadId);
        nState = new NpadState();
        InitSensor();
        debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
        debugCube.transform.localScale = new Vector3(1f, 0.5f, 3f);
        debugpoint = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    }

    internal void SetPlayerInput(RRPlayerInput rRPlayerInput)
    {
        playerInput = rRPlayerInput;
    }

    public void Calibrate()
    {
        calibrationTimer = Time.time;
        m_referenceQuaternion.Set( m_rawQuaternion.x, m_rawQuaternion.y, m_rawQuaternion.z, m_rawQuaternion.w );
        Debug.Log("m_referenceQuaternion : " + m_referenceQuaternion.eulerAngles);

        if( m_projectionQuad==null )
        {
            m_projectionQuad = GameObject.CreatePrimitive(PrimitiveType.Quad);
            m_projectionQuad.transform.localScale = new Vector3(960f, 540f, 1f);
            m_projectionQuad.name = "projection_" + nPadId;
            m_projectionQuad.layer = LayerMask.NameToLayer(RAYCAST_LAYER + nPadId.ToString());
            Renderer renderer = m_projectionQuad.GetComponent<Renderer>();
            if( renderer!=null )
            {
                Debug.Log("disable renderer");
                renderer.enabled = false;
            }
        }


        SixAxisSensor.GetState(ref state, handle[0]);

        Vector3 fwd = new Vector3(state.direction.y.x, state.direction.y.z, state.direction.y.y);
        Vector3 up = new Vector3(state.direction.z.x, state.direction.z.z, state.direction.z.y);

        // If we want to re-center the cursor we move the quad billboard
        m_projectionQuad.transform.rotation = Quaternion.LookRotation(fwd, up);
        m_projectionQuad.transform.position = fwd * m_tvDistance;
    }

    public void InitSensor()
    {
        Npad.GetState(ref nState, nPadId, nStyle);
        Debug.Log("nPadId : " + nPadId);
        GetSixAxisSensor(nPadId, nStyle);

        SixAxisSensor.GetState(ref state, handle[0]);
        state.GetQuaternion(ref npadQuaternion);
        m_rawQuaternion.Set(npadQuaternion.x, npadQuaternion.z, npadQuaternion.y, -npadQuaternion.w);
        Calibrate();
    }

    private void GetSixAxisSensor(NpadId id, NpadStyle style)
    {
        for (int i = 0; i < handleCount; i++)
        {
            if( i<handle.Length )
            {
                SixAxisSensor.Stop(handle[i]);
            }
        }

        handleCount = SixAxisSensor.GetHandles(handle, 1, id, style);

        for (int i = 0; i < handleCount; i++)
        {
            SixAxisSensor.Start(handle[i]);
        }
    }

    void Update()
    {
        ManageButtons();

        
        SixAxisSensor.GetState(ref state, handle[0]);
        state.GetQuaternion(ref npadQuaternion);
        m_rawQuaternion.Set(npadQuaternion.x, npadQuaternion.z, npadQuaternion.y, -npadQuaternion.w);

        /* Quaternion correctedQuaternion = m_rawQuaternion * Quaternion.Inverse( m_referenceQuaternion );

         float fAngleY = yRotation(correctedQuaternion).eulerAngles.y;
         float fAngleX = xRotation(correctedQuaternion).eulerAngles.x;
         //Debug.Log("fAngleX : " + fAngleX);

         Vector2 vOld;
         if( fAngleY > 180f )
         {
             fAngleY -= 360f;
         }
         vOld.x = Mathf.Clamp((fAngleY / m_semiAmplitudeX), -1f, 1f);
         if (fAngleX > 180f)
         {
             fAngleX -= 360f;
         }
         vOld.y = Mathf.Clamp((fAngleX / m_semiAmplitudeY), -1f, 1f);
        SixAxisSensor.GetState(ref state, handle[0]);
         */

        Vector3 fwd = new Vector3(state.direction.y.x, state.direction.y.z, state.direction.y.y);
        Vector3 up = new Vector3(state.direction.z.x, state.direction.z.z, state.direction.z.y);

        Ray ray = new Ray(Vector3.zero, fwd);

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity, LayerMask.GetMask(RAYCAST_LAYER + nPadId.ToString())))
        {
            debugpoint.transform.position = hit.point;
            var localHit = m_projectionQuad.transform.InverseTransformPoint(hit.point);
            Vector2 p = new Vector2(localHit.x * 2f, localHit.y * 2f);
            m_position = p;
            //            Debug.Log("m_position : " + m_position + " localHit " + localHit );
        }

        debugCube.transform.rotation = m_rawQuaternion;
    }

    private Quaternion yRotation(Quaternion q)
    {
        float theta = Mathf.Atan2(q.y, q.w);

        // quaternion representing rotation about the y axis 
        return new Quaternion(0, Mathf.Sin(theta), 0, Mathf.Cos(theta));
    }

    private Quaternion xRotation(Quaternion q)
    {
        float theta = Mathf.Atan2(q.x, q.w);

        // quaternion representing rotation about the y axis 
        return new Quaternion(Mathf.Sin(theta),0, 0, Mathf.Cos(theta));
    }

    private Quaternion zRotation(Quaternion q)
    {
        float theta = Mathf.Atan2(q.z, q.w);

        // quaternion representing rotation about the y axis 
        return new Quaternion(0, 0, Mathf.Sin(theta), Mathf.Cos(theta));
    }

    public override Vector2 GetCursorPos()
    {
        return m_position;
    }

    private void ManageButtons()
    {
        // search if recalibrate
        Npad.GetState(ref nState, nPadId, nStyle);

        if ((((nState.buttons & NpadButton.ZR) != 0 && (nState.buttons & NpadButton.R) != 0) || ((nState.buttons & NpadButton.ZL) != 0 && (nState.buttons & NpadButton.L) != 0)))
        {
            if(Time.time - calibrationTimer >= 1f )
            {
                Debug.Log("Recalibrate");
                Calibrate();
            }
        }
        else 
        {
            RRPlayerInput.ButtonPhase firePhase = ManageFirePhase(nState);
            if (playerInput.m_fireDlg == null)
            {
                if( firePhase== RRPlayerInput.ButtonPhase.press )
                {
                    RRInputManager.instance.ManageInput(RRInputManager.InputActionType.Fire);
                }
            }
            else
            {
                playerInput.UpdateFire(firePhase);
            }
        }

        // direction
        if( nState.GetButtonDown(NpadButton.StickLLeft) || nState.GetButtonDown(NpadButton.StickRLeft))
        {
            ManageMove(RRInputManager.MoveDirection.left);
        }
        if (nState.GetButtonDown(NpadButton.StickLRight) || nState.GetButtonDown(NpadButton.StickRRight))
        {
            ManageMove(RRInputManager.MoveDirection.right);
        }
        if (nState.GetButtonDown(NpadButton.StickLUp) || nState.GetButtonDown(NpadButton.StickRUp))
        {
            ManageMove(RRInputManager.MoveDirection.top);
        }
        if (nState.GetButtonDown(NpadButton.StickLDown) || nState.GetButtonDown(NpadButton.StickRDown))
        {
            ManageMove(RRInputManager.MoveDirection.bottom);
        }

        if (nState.GetButtonDown(NpadButton.A) || nState.GetButtonDown(NpadButton.Right))
        {
            ManageButton(RRInputManager.InputActionType.ButtonRight);
        }

    }

    private void ManageMove(RRInputManager.MoveDirection moveDirection )
    {
        if( playerInput.m_inputActionDlg!=null )
        {
            playerInput.m_inputActionDlg(playerInput.Id, RRInputManager.InputActionType.Move, moveDirection);
        }
        else
        {
            RRInputManager.instance.ManageInput(RRInputManager.InputActionType.Move, moveDirection);
        }
    }

    private void ManageButton(RRInputManager.InputActionType inputActionType )
    {
        if (playerInput.m_inputActionDlg != null)
        {
            playerInput.m_inputActionDlg(playerInput.Id, inputActionType);
        }
        else
        {
            RRInputManager.instance.ManageInput(inputActionType);
        }
    }

    private RRPlayerInput.ButtonPhase ManageFirePhase( NpadState npadState )
    {
        if (npadState.GetButtonDown(NpadButton.ZR) || npadState.GetButtonDown(NpadButton.ZL))
        {
            m_bOnFire = true;
            return RRPlayerInput.ButtonPhase.press;
        }
        if(npadState.GetButton(NpadButton.ZR) || npadState.GetButton(NpadButton.ZL))
        {
            m_bOnFire = true;
            return RRPlayerInput.ButtonPhase.on;
        }
        if (m_bOnFire )
        {
            m_bOnFire = false;
            return RRPlayerInput.ButtonPhase.release;
        }
        return RRPlayerInput.ButtonPhase.off;
    }
#endif
}
