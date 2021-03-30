using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class GK_Tools
{
    public static Vector3 ComputeVectorWithSegmentAndAbcisse(Vector3 vA, Vector3 vB, float fX)
    {
        float a = (vB.y - vA.y) / (vB.x - vA.x);
        float b = vA.y - (a * vA.x);
        float fY = a * fX + b;
        return new Vector3(fX, fY, vA.z);
    }

    public static List<GameObject> GetHoveredObjects( Vector2 v)
    {
        Vector3 rayOrigin = Camera.main.ViewportToWorldPoint(new Vector3(v.x, v.y, 0));

        //GameObject spere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        //spere.transform.position = rayOrigin;
        Ray ray = new Ray(rayOrigin, Vector3.forward);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayOrigin, Vector3.forward);

        List<GameObject> objects = new List<GameObject>();
        for( int i=0; i<hits.Length; i++)
        {
            if( hits[i].transform!=null )
            {
                objects.Add(hits[i].transform.gameObject);
            }
        }
        return objects;
    }
}
