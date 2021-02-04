//#define USE_ADRESSABLE

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class RR_AdressableAsset : lwSingletonMonoBehaviour<RR_AdressableAsset>
{
    public string objectToLoadAddress;
    public AssetReference accessoryObjectToLoad;
    private GameObject instantiatedObject;
    private GameObject instantiatedAccessoryObject;

    protected override void Start()
    {
        base.Start();
//        Addressables.LoadAssetAsync<GameObject>(objectToLoadAddress).Completed += ObjectLoadDone;
    }

    private void ObjectLoadDone(AsyncOperationHandle<GameObject> obj)
    {
        if (obj.Status == AsyncOperationStatus.Succeeded)
        {
            GameObject loadedObject = obj.Result;
            Debug.Log("Successfully loaded object.");
            instantiatedObject = Instantiate(loadedObject);
            Debug.Log("Successfully instantiated object.");
            if (accessoryObjectToLoad != null)
            {
                accessoryObjectToLoad.InstantiateAsync(instantiatedObject.transform).Completed += op =>
                {
                    if (op.Status == AsyncOperationStatus.Succeeded)
                    {
                        instantiatedAccessoryObject = op.Result;
                        Debug.Log("Successfully loaded and instantiated accessory object.");
                    }
                };
            }
        }
    }

    public void LoadAsset<T>(AssetReference assetReference, System.Action<AsyncOperationHandle<T>> dlg ) where T : UnityEngine.Object
    {
        Addressables.LoadAssetAsync<T>(assetReference).Completed += dlg;
    }

    public T Load<T>(string path ) where T : UnityEngine.Object
    {
#if USE_ADRESSABLE
        UnityEngine.ResourceManagement.AsyncOperations.AsyncOperationHandle<T> obj = Addressables.LoadAsset<T>(path);
        return obj.Result;
#else
        return Resources.Load<T>(path);
#endif
    }
}
