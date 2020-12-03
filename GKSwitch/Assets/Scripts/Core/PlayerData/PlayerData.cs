using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;


public class PlayerData : lwSingleton<PlayerData>
{
    private const int SAVE_VERSION = 1;

    public enum SoundType
    {
        Amb = 0,
        Sfx
    }

    private lwGameSave m_gameSave;
    private bool m_bLoaded = false;

    public float[] m_soundVolumes = new float[2] { 1f, 1f };
    public bool m_isDirty = false;

    public string m_gameInfo;


    public PlayerData()
    {
#if !UNITY_SWITCH
        string name = GameConstants.GAME_NAME;
        m_gameSave = new lwGameSave(GameConstants.COMPANY_NAME, name, !lwGameSave.HasLocalStorage(), false, false);
#endif
        Reset();
    }

    public void Reset()
    {
    }

    public void Load()
    {
        Reset();

#if !UNITY_SWITCH
        m_gameSave.Load();
        int gameVersion = m_gameSave.GetInt("SAVE_VERSION", 0);
        if( gameVersion != SAVE_VERSION )
        {
            CleanSave();
            m_bLoaded = true;
            InternalSave();
            return;
        }

        m_soundVolumes[(int)SoundType.Amb] = (m_gameSave.GetInt("AMB_VOL", (int)(m_soundVolumes[(int)SoundType.Amb]*100))/100f);
        m_soundVolumes[(int)SoundType.Sfx] = (m_gameSave.GetInt("SFX_VOL", (int)(m_soundVolumes[(int)SoundType.Sfx]*100))/ 100f);
       
#endif
        m_bLoaded = true;
    }

    public void CleanSave()
    {
        CleanSaveKey("gameInfo");
    }

    private void CleanSaveKey( string key )
    {
        if( m_gameSave.HasKey(key))
        {
            m_gameSave.RemoveKey(key);
        }
    }


    public void InternalSave()
    {
        if (!m_bLoaded)
            return;

#if !UNITY_SWITCH
        m_gameSave.SetInt("SAVE_VERSION", SAVE_VERSION);
        // Save
        m_gameSave.Save();
#endif

    }
    public void UpdateSounds()
    {
    }


    public void UpdateSoundsVolume(int soundType, float fNewValue )
    {
        float fold = m_soundVolumes[soundType];
        m_soundVolumes[soundType] = fNewValue;
        m_isDirty = true;

        switch ( soundType )
        {
            case (int)SoundType.Amb:
                {
                    SoundManager.instance.UpdateAmbVolume(fold, fNewValue);
                }
                break;
            case (int)SoundType.Sfx:
                {
                    SoundManager.instance.PlaySound(SoundManager.SoundFx.navValide);
                }
                break;
        }
    }

    public void CheckAndSaveChange()
    {
        if( m_isDirty )
        {
            m_isDirty = false;
            InternalSave();
        }
    }

    public void PauseSounds()
    {
        // amb
#if USE_FMOD
        FMOD.Studio.Bus bus = FMODUnity.RuntimeManager.GetBus("bus:/AMB");
        bus.setVolume(0f);

        // Sfx
        bus = FMODUnity.RuntimeManager.GetBus("bus:/SFX");
        bus.setVolume(0f);
#endif
    }


}
