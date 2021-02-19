using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class EX_MoveConfig : MonoBehaviour
{
    [SerializeField]
    public float m_fScale;
    [SerializeField]
    public int m_nOrderLayer;
    [SerializeField]
    public Vector3 m_vStartPoint;
    [SerializeField]
    public Vector3 m_vEndPoint;
#if UNITY_EDITOR
    [Range(0f, 1f)]
    public float m_fValueTest;
    [SerializeField]
    public EX_Character m_characterTest;

    private void Update()
    {
        if( m_characterTest!=null )
        {
            for( int i=0; i< m_characterTest.m_spriteRenderers.Length; i++ )
            {
                m_characterTest.m_spriteRenderers[i].sortingOrder = m_nOrderLayer;
            }
            Vector3 v = m_vStartPoint + (m_vEndPoint - m_vStartPoint) * m_fValueTest;
            m_characterTest.transform.position = v;
        }
    }

#endif


    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        Gizmos.DrawCube(m_vStartPoint, new Vector3(1f, 1f, 1f));
        Gizmos.DrawCube(m_vEndPoint, new Vector3(1f, 1f, 1f));

        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(m_vStartPoint, m_vEndPoint);

        Gizmos.DrawIcon((m_vStartPoint+m_vEndPoint)/2f, "IMG_expression.png", true);
    }
}
