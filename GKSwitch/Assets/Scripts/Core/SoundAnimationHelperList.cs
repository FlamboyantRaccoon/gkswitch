using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundAnimationHelperList : SoundAnimationHelper
{
    public static int sCounter = 0;

    [SerializeField]
    private string[] soundsList = null;

    public void PlayIncrementalSound()
    {
        sCounter = (sCounter + 1) % soundsList.Length;

        RRSoundManager.instance.PlaySound(soundsList[sCounter]);
        //FMODUnity.RuntimeManager.PlayOneShot(sSoundName); // "event:/GoodMove");
    }

}
