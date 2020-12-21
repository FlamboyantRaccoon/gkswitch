using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GC_PlayerCanvas : MiniGamePlayerCanvas
{
    private GC_GreatClimbing.WallTouch m_wallTouch;
    private float m_nMaxAltitudeReach = 0f;
    private Vector3 m_mainCameraInitialPosition;
    private float m_fFallTime = -1f;

    private float m_fTotalFall;
    private int m_nFlowerKills;
    private Vector2 m_vInputPos;
    private GC_GreatClimbing m_greatClimbing;
    private MG_PlayerCursor m_cursor;

    public void Init(int playerId, GC_GreatClimbing greatClimbing)
    {
        base.Init(playerId);
        m_greatClimbing = greatClimbing;
        m_wallTouch = null;

        m_mainCameraInitialPosition = Camera.main.transform.position;
        m_camera.transform.position = m_mainCameraInitialPosition;

        m_fTotalFall = 0f;
        m_nFlowerKills = 0;

        m_cursor = GameObject.Instantiate<MG_PlayerCursor>(AssetHolder.instance.playerCursorPrefab);
        m_cursor.Setup(playerId);
    }

    public override void Clean()
    {
        GameObject.Destroy(m_cursor.gameObject);
    }

    internal void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        m_vInputPos = v;

        switch (buttonPhase)
        {
            case RRPlayerInput.ButtonPhase.press:
                {
                    Vector3 rayOrigin = m_camera.ViewportToWorldPoint(new Vector3(v.x, v.y, 0));
                    //GameObject spere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //spere.transform.position = rayOrigin;
                    Ray ray = new Ray(rayOrigin, Vector3.forward);
                    RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray); // Camera.main.ScreenPointToRay(v));

                    if (rayHit.transform != null)
                    {
                        GC_GripImg grip = rayHit.transform.GetComponent<GC_GripImg>();
                        if (grip != null)
                        {
                            grip.OnPlayerInput(m_playerId, v, buttonPhase);
                        }
                    }
                }
                break;
            case RRPlayerInput.ButtonPhase.on:
                {

                }
                break;
            case RRPlayerInput.ButtonPhase.release:
                {
                    if (m_wallTouch!=null)
                    {
                        m_wallTouch.m_grip.SetHold(false);
                        m_wallTouch = null;

                    }
                }
                break;
        }
    }

    internal void OnGrip(Vector2 v, GC_Grip grip)
    {
        Vector2 vViewport = v;
        Vector3 vPos = m_camera.ViewportToWorldPoint(vViewport);
        //        Debug.Log("OnGripDownDlg! " + eventData.position + " / " + vPos );
        m_wallTouch = new GC_GreatClimbing.WallTouch(grip, 0, vPos);
        vPos.z = 0f;
        m_cursor.transform.position = vPos;
        grip.SetHold(true);
    }

    internal void OnFlowerHit()
    {
        m_nFlowerKills++;
    }

    internal void ManageScroll()
    {
        Vector2 vOldWallPos = m_greatClimbing.GetWallCoord(m_camera.transform.position);
        // look if strong hand
        if (m_wallTouch!=null)
        {
            m_fFallTime = -1f;
            Vector2 vViewport = m_vInputPos;
            Vector3 vPos = m_camera.ViewportToWorldPoint(vViewport);
            Vector3 vTranslate = vPos - m_wallTouch.m_vPosition;

            if (m_camera.transform.position.y - vTranslate.y < m_mainCameraInitialPosition.y)
            {
                vTranslate.y = -(m_camera.transform.position.y - m_mainCameraInitialPosition.y);
                vPos = m_wallTouch.m_vPosition + vTranslate;
            }

            m_camera.transform.Translate(-vTranslate);
            vPos = m_camera.ViewportToWorldPoint(vViewport);
            m_wallTouch.m_vPosition = vPos;
        }
        else // it's a fall !
        {
            float fDeltatTime = Time.deltaTime;
            m_fFallTime = m_fFallTime < 0f ? fDeltatTime : m_fFallTime + fDeltatTime;

            if (m_fFallTime >= m_greatClimbing.m_fTimeBeforeFall)
            {
                float fFallCoeff = Mathf.Clamp(m_fFallTime - m_greatClimbing.m_fTimeBeforeFall, 0f, m_greatClimbing.m_fTimeToReachFallSpeed) / m_greatClimbing.m_fTimeToReachFallSpeed;
                float fSpeed = Mathf.Sin(fFallCoeff * (Mathf.PI / 2)) * m_greatClimbing.m_fFallSpeed;
                Vector3 vPos = m_camera.transform.position;
                vPos.y = Mathf.Max(m_mainCameraInitialPosition.y, vPos.y - fSpeed * fDeltatTime);

                m_fTotalFall += (m_camera.transform.position.y - vPos.y);
                m_camera.transform.position = vPos;
            }
        }
        Vector2 vNewWallPos = m_greatClimbing.GetWallCoord(m_camera.transform.position);

        // check if need to generate new elements
        m_greatClimbing.CheckAndGenerateNewWallElements(vOldWallPos, vNewWallPos);

        // check point
        if (m_camera.transform.position.y - m_mainCameraInitialPosition.y > m_nMaxAltitudeReach)
        {
            m_nMaxAltitudeReach = m_camera.transform.position.y - m_mainCameraInitialPosition.y;
            float fMeters = (m_nMaxAltitudeReach / ((float)m_greatClimbing.m_nMeterUnits));
            int nPointsWin = (int)(fMeters * m_greatClimbing.m_nMeterPointsWin);
            BattleContext.instance.SetPoint(nPointsWin, m_playerId);
            //m_hud.UpdateAltitude(fMeters);
        }
    }
}
