using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BQ_PlayerInfos
{
    private BQ_OrderView m_orderView = null;
    private BQ_Meal m_dragObject = null;
    private int m_playerId;

    public void Setup( int playerId )
    {
        m_playerId = playerId;
    }

    public void SetOrderView( BQ_OrderView orderView )
    {
        m_orderView = orderView;
    }

    internal void ManageFireInput(Vector2 v, RRPlayerInput.ButtonPhase buttonPhase)
    {
        switch (buttonPhase)
        {
            case RRPlayerInput.ButtonPhase.press:
                {
                    Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(v.x, v.y, 0));
                    //GameObject spere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                    //spere.transform.position = rayOrigin;
                    Ray ray = new Ray(rayOrigin, Vector3.forward);
                    RaycastHit2D rayHit = Physics2D.GetRayIntersection(ray); // Camera.main.ScreenPointToRay(v));

                    if (rayHit.transform != null)
                    {
                        BQ_MealElt meal = rayHit.transform.GetComponent<BQ_MealElt>();
                        if (meal != null)
                        {
                            m_dragObject = meal.m_meal;
                            m_dragObject.OnPlayerInput(m_playerId, v, buttonPhase);
                        }
                    }
                }
                break;
            case RRPlayerInput.ButtonPhase.on:
                {
                    if( m_dragObject!=null )
                    {
                        m_dragObject.OnPlayerInput(m_playerId, v, buttonPhase);
                    }
                }
                break;
            case RRPlayerInput.ButtonPhase.release:
                {
                    if (m_dragObject != null)
                    {
                        m_dragObject.OnPlayerInput(m_playerId, v, buttonPhase);
                        m_dragObject = null;
                    }
                }
                break;
        }
    }

    internal void SpawnOrder(BQ_Order order)
    {
        m_orderView.SetOrder(order);
    }

    internal Vector3 GetOrderPos()
    {
        return m_orderView.transform.position;
    }

    internal void Clear()
    {
        GameObject.Destroy(m_orderView.gameObject);
    }
}
