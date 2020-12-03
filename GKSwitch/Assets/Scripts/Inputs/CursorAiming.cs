using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorAiming : MonoBehaviour
{
    public virtual void UpdateVector( Vector2 v)
    {

    }

    public virtual void UpdateMousePosition(Vector2 v)
    {

    }

    public virtual Vector2 GetCursorPos()
    {
        return Vector2.zero;
    }
}
