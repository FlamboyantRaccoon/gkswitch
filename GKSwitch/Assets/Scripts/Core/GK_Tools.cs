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
}
