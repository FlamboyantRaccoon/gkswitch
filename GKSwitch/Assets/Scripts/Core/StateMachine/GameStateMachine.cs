using UnityEngine;
using System.Collections;

public class GameStateMachine 
{
	private GameState m_CurrentState;
	private GameState m_GlobalState;
	private GameState m_PreviousState;
	
	private bool						m_bPause = false;
	
	
	// ------------------------------------------------------------------
	// Created PC 16/03/12
	// ------------------------------------------------------------------
	public void Update () 
	{
		if( !m_bPause )
		{
			if( m_CurrentState != null )
			{
				m_CurrentState.Update();
			}
		}
		
		if( m_GlobalState != null )
			m_GlobalState.Update();
	}
	
	// ------------------------------------------------------------------
	// Created PC 16/03/12
	// ------------------------------------------------------------------
	public void ChangeState(GameState newState )
	{
//		Debug.Log( "Changing from : " + m_CurrentState + " To : " + newState );
		
		if( m_CurrentState != null )
		{
			m_CurrentState.Exit();
			m_PreviousState = m_CurrentState;
		}
		
		m_CurrentState = newState;
		
		if( m_CurrentState != null )
			m_CurrentState.Enter();
	}
	
	// ------------------------------------------------------------------
	// Created PC 23/03/12
	// ------------------------------------------------------------------
	public void SetGlobalState( GameState gState )
	{
		m_GlobalState = gState;
	}
	
	// ------------------------------------------------------------------
	// Created PC 16/03/12
	// ------------------------------------------------------------------
	public bool HandleMessage( string sMsg )
	{
		if( !m_bPause )
		{
			if( m_CurrentState!=null && m_CurrentState.HandleMessage( sMsg ) )
				return true;
		}
		
		if( m_GlobalState!=null && m_GlobalState.HandleMessage( sMsg ) )
			return true;
		return false;
	}
	
	// ------------------------------------------------------------------
	// Created PC 16/03/12
	// ------------------------------------------------------------------
	public void BackToPreviousState(  )
	{
		ChangeState(  m_PreviousState );
	}
	
	public void SetPause( bool bPause )
	{
		m_bPause = bPause;
	}
	
	public bool IsPaused
	{
		get { return m_bPause; }
	}
	
	public System.Type GetTypeOfCurrentState()
	{
		return m_CurrentState.GetType();
	}
	
	public void Clean( )
	{
		if( m_CurrentState != null )
		{
			m_CurrentState.Exit();
			m_CurrentState = null;
		}
	}
	
	public void Destroy()
	{
		m_CurrentState = null;
		m_GlobalState = null;
		m_PreviousState = null;
	}
	
}
