using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BD_BalloonSpawner : MonoBehaviour {

    [SerializeField]
    private float m_fSize;
    [SerializeField]
    private Vector2 m_vDir;

    System.Func<float> m_getSpeedMul;
    System.Func<Vector2> m_getMoveModificator;
    System.Func<bool> m_canDestroyBalloon;
    lwObjectPool<BD_Balloon> m_balloonPool;
    System.Func<int,int, Vector3, bool> m_drillBalloon;

    public void Init(System.Func<float> getSpeedMul,
        System.Func<Vector2> getMoveModificator,
        lwObjectPool<BD_Balloon> balloonPool,
        System.Func<int,int, Vector3, bool> drillBalloon,
        System.Func<bool> canDestroyBalloon)
    {
        m_getSpeedMul = getSpeedMul;
        m_balloonPool = balloonPool;
        m_getMoveModificator = getMoveModificator;
        m_drillBalloon = drillBalloon;
        m_canDestroyBalloon = canDestroyBalloon;
    }

    public void SpawnBalloon(float fPosRatio, float fSpeed, int nColorId, float fDepth)
    {
        BD_Balloon balloon = m_balloonPool.GetInstance(transform);
        float fZ = -10f + fDepth * 20f;
        float fScale = 1f - (0.5f * fDepth);
        balloon.transform.localPosition = new Vector3((fPosRatio - 0.5f) * m_fSize, 0f, fZ) ;
        balloon.transform.localScale = new Vector3(fScale, fScale, fScale);
        balloon.Setup(nColorId, m_vDir, fSpeed, m_getSpeedMul, m_getMoveModificator, DeleteBalloon, CanDestroyBalloon,m_drillBalloon);
    }

    private void DeleteBalloon(BD_Balloon balloon)
    {
        m_balloonPool.PoolObject(balloon);
    }

    private bool CanDestroyBalloon()
    {
        if( m_canDestroyBalloon!=null )
        {
            return m_canDestroyBalloon();
        }
        return false;
    }

    void OnDrawGizmosSelected()
    {
        // Display the explosion radius when selected
        Gizmos.color = new Color(1, 0, 0, 0.75F);
        Gizmos.DrawCube(transform.position, new Vector3( m_fSize, 1f, 1f ) );

        Gizmos.color = Color.yellow;
        Vector3 vOffset = (Vector3)m_vDir.normalized * 3000;
        Vector3 vStart = transform.position + Vector3.left * m_fSize / 2f;
        Vector3 vEnd = vStart + vOffset;
        Gizmos.DrawLine(vStart, vEnd);

        vStart = transform.position + Vector3.right * m_fSize / 2f;
        vEnd = vStart + vOffset;
        Gizmos.DrawLine(vStart, vEnd);
    }

}
