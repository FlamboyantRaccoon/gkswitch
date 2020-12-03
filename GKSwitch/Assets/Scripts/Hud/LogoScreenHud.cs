using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LogoScreenHud : MonoBehaviour
{

    [SerializeField]
    private CanvasGroup m_canvasGroup = null;
    [SerializeField]
    private float m_fFadeTime = 0.5f;


    private Coroutine m_fadeOutRoutine = null;
    // Use this for initialization
    public void FadeOut()
    {
        m_fadeOutRoutine = StartCoroutine(FadeOutRoutine());
    }

    public void Reset()
    {
        if (m_fadeOutRoutine != null)
        {
            StopCoroutine(m_fadeOutRoutine);
        }
        m_canvasGroup.alpha = 1f;
    }

    private IEnumerator FadeOutRoutine()
    {
        float fElapsedTime = 0;
        float fStartTime = Time.time;

        while (fElapsedTime < m_fFadeTime)
        {
            fElapsedTime = Time.time - fStartTime;
            if (fElapsedTime < m_fFadeTime)
            {
                float fCoeff = (fElapsedTime / m_fFadeTime);
                m_canvasGroup.alpha = 1f - fCoeff;
                yield return null;
            }
        }

        m_canvasGroup.alpha = 1f;
        HudManager.instance.HideHud(HudManager.GameHudType.logoScreen);
    }
}