using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IH_MainObject : MonoBehaviour
{
    [SerializeField]
    private GameObject[] SidesObject;

    [HideInInspector]
    public Rect[] sideRect;

    private void Awake()
    {
        sideRect = new Rect[SidesObject.Length];
        for( int i=0; i<sideRect.Length; i++ )
        {
            sideRect[i] = ComputeRectFromObject(SidesObject[i]);
        }
    }

    private Rect ComputeRectFromObject( GameObject obj )
    {
        Rect r = new Rect();
        r.width = obj.transform.lossyScale.x;
        r.height = obj.transform.lossyScale.y;
        r.x = obj.transform.position.x - r.width/2f;
        r.y = obj.transform.position.y - r.height/2f;
        return r;
    }


}
