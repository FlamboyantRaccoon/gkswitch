using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EX_Character : MonoBehaviour
{
    public enum CharacterState { appear, idle, disappear, found };
    public enum CharacterPart { body, leftEye, rightEye, mouth };

    [SerializeField]
    public SpriteRenderer[] m_spriteRenderers;
    [SerializeField]
    public Animator m_animator;
    [SerializeField]
    public SpriteRenderer[] m_glowSpriteRenderers;

    public bool bFound { get { return m_bFound; } }
    public ushort nExpressionMask { get { return m_nExpressionMask; } }

    System.Action<EX_Character> m_deleteAction;
    System.Func<EX_Character, int, bool> m_onPressAction;

    private AnimationCurve m_apparitionCurve;
    private EX_MoveConfig m_moveConfig;
    private EX_Logic.ItemSpawnInfo m_spawnInfo;

    private CharacterState m_state;
    private float m_stateTimer = 0f;
    private bool m_bFound;
    private ushort m_nExpressionMask;
    private float m_fSetupTimer = 0f;

    public void Setup(Sprite[] m_sprites, ushort _nExpressionMask, AnimationCurve apparitionCurve, EX_MoveConfig moveConfig, EX_Logic.ItemSpawnInfo spawn, System.Action<EX_Character>  deleteItemAction, System.Func<EX_Character, int, bool> onItemPressAction)
    {
        m_apparitionCurve = apparitionCurve;
        m_moveConfig = moveConfig;
        m_spawnInfo = spawn;
        m_deleteAction = deleteItemAction;
        m_onPressAction = onItemPressAction;
        m_state = CharacterState.appear;
        m_stateTimer = Time.time;
        m_bFound = false;
        m_nExpressionMask = _nExpressionMask;
        m_fSetupTimer = Time.time;

        for ( int nSpriteId = 0; nSpriteId< m_spriteRenderers.Length; nSpriteId++ )
        {
            m_spriteRenderers[nSpriteId].sortingOrder = m_moveConfig.m_nOrderLayer;
            if( m_sprites[nSpriteId]!=null )
            {
                m_spriteRenderers[nSpriteId].sprite = m_sprites[nSpriteId];
            }
        }

        for (int nSpriteId = 0; nSpriteId < m_glowSpriteRenderers.Length; nSpriteId++)
        {
            m_glowSpriteRenderers[nSpriteId].sortingOrder = m_moveConfig.m_nOrderLayer;
        }
        m_animator.Play( "Emote_idle" );
    }

    public float ComputeShowTime()
    {
        return (Time.time - m_fSetupTimer); // Mathf.Clamp01( (Time.time - m_fSetupTimer) / (m_spawnInfo.fAppearTime + m_spawnInfo.fIdleTime + m_spawnInfo.fDisAppearTime) );
    }

    public void KillMeSoftly() // With his song
    {
        if (m_deleteAction != null)
        {
            m_deleteAction(this);
        }
    }

    public void BadMove()
    {
        m_bFound = true;
        m_animator.SetTrigger("Wrong");
        m_state = CharacterState.found;

        for (int nSpriteId = 0; nSpriteId < m_spriteRenderers.Length; nSpriteId++)
        {
            m_spriteRenderers[nSpriteId].sortingOrder = 10;
        }

        for (int nSpriteId = 0; nSpriteId < m_glowSpriteRenderers.Length; nSpriteId++)
        {
            m_glowSpriteRenderers[nSpriteId].sortingOrder =10;
        }
    }

    public void Found()
    {
        m_bFound = true;
        m_animator.SetTrigger("Found");
        m_state = CharacterState.found;

        for (int nSpriteId = 0; nSpriteId < m_spriteRenderers.Length; nSpriteId++)
        {
            m_spriteRenderers[nSpriteId].sortingOrder = 10;
        }

        for (int nSpriteId = 0; nSpriteId < m_glowSpriteRenderers.Length; nSpriteId++)
        {
            m_glowSpriteRenderers[nSpriteId].sortingOrder = 10;
        }
    }

    public void Update()
    {
        float fElapsedTime = Time.time - m_stateTimer;
        switch( m_state )
        {
            case CharacterState.appear:
                {
                    if (fElapsedTime > m_spawnInfo.fAppearTime)
                    {
                        float fOffsetTime = fElapsedTime - m_spawnInfo.fAppearTime;
                        m_stateTimer = Time.time - fOffsetTime;
                        m_state = CharacterState.idle;
                        SetPosition(1f);
                    }
                    else
                    {
                        float fCoeff = (fElapsedTime / m_spawnInfo.fAppearTime);
                        fCoeff = m_apparitionCurve.Evaluate(fCoeff);
                        SetPosition(fCoeff);
                    }
                }
                break;
            case CharacterState.idle:
                {
                    if (fElapsedTime > m_spawnInfo.fIdleTime )
                    {
                        float fOffsetTime = fElapsedTime - m_spawnInfo.fIdleTime;
                        m_stateTimer = Time.time - fOffsetTime;
                        m_state = CharacterState.disappear;
                    }
                }
                break;
            case CharacterState.disappear:
                {
                    if( fElapsedTime > m_spawnInfo.fDisAppearTime )
                    {
                        KillMeSoftly();
                    }
                    else
                    {
                        float fCoeff = 1f - (fElapsedTime / m_spawnInfo.fDisAppearTime);
                        SetPosition(fCoeff);
                    }
                }
                break;
        }
    }

    private void SetPosition( float fCoeff )
    {
        Vector3 v = m_moveConfig.m_vStartPoint + (m_moveConfig.m_vEndPoint - m_moveConfig.m_vStartPoint) * fCoeff;
        transform.position = v;
    }



    public void OnShoot( int playerId)
    {
        if( !m_bFound && m_onPressAction!=null )
        {
            if( m_onPressAction(this, playerId))
            {
                m_bFound = true;
            }
        }
    }

}
