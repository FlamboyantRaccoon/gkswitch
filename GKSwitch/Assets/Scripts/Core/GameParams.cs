using System.Collections.Generic;
using UnityEngine;

public static class GameParams
{
    public static float m_fInGameSpeed = 1f; // hoursBySecond






    //public static Dictionary<string, FMOD.Studio.EventInstance> m_persistentSound;
    
    public static void PlayPersistentSoundStatic(string sSoundName)
    {
        //Debug.Log("PlayPersistentSoundStatic : " + sSoundName);
 /*       if (m_persistentSound == null)
        {
            m_persistentSound = new Dictionary<string, FMOD.Studio.EventInstance>();
        }

        FMOD.Studio.EventInstance sound;
        if (m_persistentSound.TryGetValue(sSoundName, out sound))
        {
            return;
        }
        sound = FMODUnity.RuntimeManager.CreateInstance(sSoundName);
        sound.start();
        m_persistentSound.Add(sSoundName, sound);*/
    }

    public static void StopPersistentSoundStatic(string sSoundName)
    {
        //Debug.Log("StopPersistentSoundStatic : " + sSoundName);

  /*      if (m_persistentSound == null)
        {
            return;
        }

        FMOD.Studio.EventInstance sound;
        if (m_persistentSound.TryGetValue(sSoundName, out sound))
        {
            sound.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            sound.release();
            m_persistentSound.Remove(sSoundName);
        }*/
    }

}
