using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : RRSoundManager
{



    public enum SoundFx { navMove, navValide, navCancel, buildingOpen, buildingClose, eventWin, eventLose, phoneRing, phoneTexto }
    public enum SoundAmb { none, menu, light, loud }

    [System.Serializable] private class FxClip : lwEnumArray<SoundFx, AudioClip> { }; // dummy definition to use Unity serialization
    [System.Serializable] private class AmbClip : lwEnumArray<SoundAmb, AudioClip> { }; // dummy definition to use Unity serialization

    
    [SerializeField]
    FxClip m_clips = null;
    [SerializeField]
    AmbClip m_ambiance = null;

    private SoundAmb m_currentAmbiance;

    public void PlaySound(SoundFx soundType)
    {
        int index = GetFreeAudioSourceIndex();
        if (index >= 0)
        {
            AudioSource audioSource = m_sfxSources[index];
            audioSource.clip = m_clips[(int)soundType];
            audioSource.volume = PlayerData.instance.m_soundVolumes[(int)RRSoundManager.SoundType.Sfx];
            audioSource.Play();
        }
    }


    public void StartAmbiance(SoundAmb soundAmb)
    {
        if (soundAmb == m_currentAmbiance)
        {
            return;
        }
        // stop previous
        if (m_currentAmbiance != SoundAmb.none)
        {
            AmbianceData ambianceData = GetAmbianceData(m_currentAmbiance);
            StopAmbiance(ambianceData);
        }

        m_currentAmbiance = soundAmb;
        // start new one
        if (m_currentAmbiance != SoundAmb.none)
        {
            AmbianceData ambianceData = GetAmbianceData(m_currentAmbiance);
            StartAmbiance(ambianceData);
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
        string key = soundAmb.ToString();
        if( m_ambianceDico.ContainsKey(key))
        {
            return m_ambianceDico[key];
        }

        AmbianceData ambianceData = new AmbianceData();
        ambianceData.m_source = gameObject.AddComponent<AudioSource>();
        ambianceData.m_source.clip = m_ambiance[soundAmb];
        ambianceData.m_source.loop = true;
        m_ambianceDico.Add(key, ambianceData);
        return ambianceData;
    }



    protected override void Awake()
    {
        base.Awake();
        m_currentAmbiance = SoundAmb.none;
    }
   
}
