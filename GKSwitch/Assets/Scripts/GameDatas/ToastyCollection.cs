using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Toasties", menuName = "GameData/ToastyCollection", order = 2)]
public class ToastyCollection : ScriptableObject
{
    [SerializeField]
    public int test;
    [SerializeField]
    public ToastyData[] toastyDatas;

    internal ToastyData GetToasty(string sToastyId)
    {
        int index = 0;
        ToastyData toasty = null;
        while( toasty==null && index<toastyDatas.Length )
        {
            if( toastyDatas[index].sId == sToastyId )
            {
                toasty = toastyDatas[index];
            }
            else
            {
                index++;
            }
        }
        return toasty;
    }
}
