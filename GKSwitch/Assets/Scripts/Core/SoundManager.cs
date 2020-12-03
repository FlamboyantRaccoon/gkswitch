using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : lwSingletonMonoBehaviour<SoundManager>
{
    public const float EVENT_AMB_VOLUME = 0.3f;
    public class AmbianceData
    {
        public AudioSource m_source;
        public Coroutine m_fadeRoutine;
        public float m_fCurrentVolume;
    }



    public enum SoundFx { navMove, navValide, navCancel, buildingOpen, buildingClose, eventWin, eventLose, phoneRing, phoneTexto }
    public enum SoundAmb { none, menu, light, loud }

    [System.Serializable] private class FxClip : lwEnumArray<SoundFx, AudioClip> { }; // dummy definition to use Unity serialization
    [System.Serializable] private class AmbClip : lwEnumArray<SoundAmb, AudioClip> { }; // dummy definition to use Unity serialization

    [SerializeField]
    private float m_ambianceFadeDuration = 0.5f;
    [SerializeField]
    private int m_sfXSourceMax = 8;
    [SerializeField]
    FxClip m_clips = null;
    [SerializeField]
    AmbClip m_ambiance = null;

    private AudioSource[] m_sfxSources;
    private Dictionary<SoundAmb, AmbianceData> m_ambianceDico;

    private SoundAmb m_currentAmbiance;

    public void PlaySound(SoundFx soundType)
    {
        int index = GetFreeAudioSourceIndex();
        if (index >= 0)
        {
            AudioSource audioSource = m_sfxSources[index];
            audioSource.clip = m_clips[(int)soundType];
            audioSource.volume = PlayerData.instance.m_soundVolumes[(int)PlayerData.SoundType.Sfx];
            audioSource.Play();
        }
    }

    public void StartAmbiance( SoundAmb soundAmb )
    {
        if( soundAmb==m_currentAmbiance )
        {
            return;
        }
        // stop previous
        if( m_currentAmbiance!= SoundAmb.none )
        {
            AmbianceData ambianceData = GetAmbianceData(m_currentAmbiance);
            StopAmbiance(ambianceData);
        }

        m_currentAmbiance = soundAmb;
        // start new one
        if (m_currentAmbiance != SoundAmb.none )
        {
            AmbianceData ambianceData = GetAmbianceData(m_currentAmbiance);
            StartAmbiance(ambianceData);
        }
    }

    public void UpdateAmbVolume( float previousVolume, float newVolume )
    {
        foreach( KeyValuePair<SoundAmb, AmbianceData> pair in m_ambianceDico )
        {
            pair.Value.m_source.volume = pair.Value.m_fCurrentVolume * newVolume;
        }
    }

    public void FadeAmbiance( float fVolumeTarget )
    {
        AmbianceData ambianceData = GetAmbianceData(m_currentAmbiance);
        if (ambianceData.m_fadeRoutine != null)
        {
            StopCoroutine(ambianceData.m_fadeRoutine);
        }
        ambianceData.m_fadeRoutine = StartCoroutine(FadeRoutine(ambianceData, ambianceData.m_fCurrentVolume, fVolumeTarget, m_ambianceFadeDuration));
    }

    private AmbianceData GetAmbianceData(SoundAmb soundAmb)
    {
        if( m_ambianceDico.ContainsKey(soundAmb))
        {
            return m_ambianceDico[soundAmb];
        }

        AmbianceData ambianceData = new AmbianceData();
        ambianceData.m_source = gameObject.AddComponent<AudioSource>();
        ambianceData.m_source.clip = m_ambiance[soundAmb];
        ambianceData.m_source.loop = true;
        m_ambianceDico.Add(soundAmb, ambianceData);
        return ambianceData;
    }

    private void StartAmbiance( AmbianceData ambianceData )
    {
        if( ambianceData.m_fadeRoutine!=null )
        {
            StopCoroutine(ambianceData.m_fadeRoutine);
        }
        ambianceData.m_fadeRoutine = StartCoroutine(FadeRoutine(ambianceData, 0f, 1f, m_ambianceFadeDuration));
    }

    private void StopAmbiance(AmbianceData ambianceData)
    {
        if (ambianceData.m_fadeRoutine != null)
        {
            StopCoroutine(ambianceData.m_fadeRoutine);
        }
        ambianceData.m_fadeRoutine = StartCoroutine(FadeRoutine(ambianceData, 1f, 0f, m_ambianceFadeDuration));
    }

    private IEnumerator FadeRoutine(AmbianceData ambianceData, float start, float end, float duration )
    {
        float fAmbVolume = PlayerData.instance.m_soundVolumes[(int)PlayerData.SoundType.Amb];
        if(!ambianceData.m_source.isPlaying ) // start == 0 )
        {
            ambianceData.m_source.Play();
        }
        ambianceData.m_source.volume = start * fAmbVolume;
        
        float elapsedTime = 0f;
        while( elapsedTime < duration )
        {
            yield return null;
            elapsedTime += Time.deltaTime;
            ambianceData.m_fCurrentVolume = Mathf.Lerp(start, end, Mathf.Clamp01(elapsedTime / duration) );
            ambianceData.m_source.volume = ambianceData.m_fCurrentVolume * fAmbVolume;
        }

        /*if (end == 0)
        {
            ambianceData.m_source.Stop();
        }*/
    }

    private void Awake()
    {
        m_sfxSources = new AudioSource[m_sfXSourceMax];
        m_currentAmbiance = SoundAmb.none;
        m_ambianceDico = new Dictionary<SoundAmb, AmbianceData>();
    }

    private int GetFreeAudioSourceIndex()
    {
        bool found = false;
        int index = 0;

        while (!found && index < m_sfxSources.Length)
        {
            if (m_sfxSources[index] == null || !m_sfxSources[index].isPlaying)
            {
                found = true;
            }
            else
            {
                index++;
            }
        }

        if (found)
        {
            if (m_sfxSources[index] == null)
            {
                m_sfxSources[index] = gameObject.AddComponent<AudioSource>();
            }
            return index;
        }
        return -1;
    }

}
