using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniGameSpecialRoulette : MonoBehaviour
{
    [System.Serializable]
    public struct MinigameData
    {
        public Material m_material;
        public string m_sSound;
    }

    [System.Serializable] private class MinigameDatas : lwEnumArray<MiniGameManager.MiniGames, MinigameData> { }; // dummy definition to use Unity serialization


    [SerializeField]
    Animator m_animator;
    [SerializeField]
    MiniGameCardBase m_miniGameCardBase;
    [SerializeField]
    private ParticlePlayground.PlaygroundParticlesC m_specialRouletteParticles;
    [SerializeField]
    private MinigameDatas m_miniGameDatas;

    private System.Action m_onEndPlay;

    public void Disable()
    {
        gameObject.SetActive(false);
    }

    public void Setup( System.Action onEndPlay, int nMiniGameId )
    {
        m_onEndPlay = onEndPlay;
        m_miniGameCardBase.InitGame(nMiniGameId);
        gameObject.SetActive(true);
        m_animator.Play("RouletteSpecial");

        if(m_specialRouletteParticles!=null && m_miniGameDatas[nMiniGameId].m_material!=null )
        {
            m_specialRouletteParticles.particleSystemRenderer.sharedMaterial = m_miniGameDatas[nMiniGameId].m_material;
        }
    }

    public void OnEndPlayAnim()
    {
        if( m_onEndPlay!=null )
        {
            m_onEndPlay();
        }
    }

}
