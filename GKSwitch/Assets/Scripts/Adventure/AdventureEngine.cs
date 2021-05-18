using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.ResourceManagement.AsyncOperations;

public class AdventureEngine
{
    const string PLACE_PATH = "Assets/FinalAssets/Adventure/MainPlaceRoot.prefab";

    public bool isInit { get; private set; }


    private AdventurePlace m_adventurePlace = null;

    public void Init()
    {
        isInit = false;

        // meal
        RR_AdressableAsset.instance.LoadAsset<GameObject>(PLACE_PATH, OnPlaceLoad);
    }

    private void OnPlaceLoad(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            Debug.Log("Successfully loaded object.");
            GameObject bkg = GameObject.Instantiate(loadedObject);
            m_adventurePlace = bkg.GetComponent<AdventurePlace>();
        }
        isInit = true;
    }
}
