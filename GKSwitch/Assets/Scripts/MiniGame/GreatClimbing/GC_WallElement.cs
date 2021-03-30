using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class GC_WallElement : MonoBehaviour
{
    [SerializeField]
    SpriteRenderer m_spriteRenderer;
    [SerializeField]
    Sprite[] m_wallSprite;
    [SerializeField]
    SpriteRenderer m_foliageSprite;

    lwObjectPool<GC_Grip> m_gripPool;
    private GC_GreatClimbing.WallEltData m_data;
    private List<GC_Grip> m_gripsList = new List<GC_Grip>();

    public void Setup( GC_GreatClimbing.WallEltData data, lwObjectPool<GC_Grip> gripPool, GC_GripImg.OnGripDlg onGripDlg, System.Action<int> onObstacleTouch )
    {
        m_data = data;
        m_spriteRenderer.sprite = m_wallSprite[data.wallEltId];
        m_gripPool = gripPool;
        
        float fSize = GetEltSize();
        Vector3 vPos = Vector3.zero;
        vPos.z = -10f;
        for( int nGripId=0; nGripId<data.gripsArray.Length; nGripId++ )
        {
            vPos.x = data.gripsArray[nGripId].vPos.x * fSize - fSize/2;
            vPos.y = data.gripsArray[nGripId].vPos.y * fSize - fSize/2;
            GC_Grip grip = m_gripPool.GetInstance( transform);
            grip.gameObject.name = "GripRoot_" +m_data.nX.ToString() + "_" + m_data.nY.ToString() + "_" + nGripId;
            grip.transform.localPosition = vPos;
            grip.Setup(data.gripsArray[nGripId], onGripDlg, onObstacleTouch );
            m_gripsList.Add(grip);
        }

        m_foliageSprite.gameObject.SetActive(data.bHaveFoliage);
        if(data.bHaveFoliage )
        {
            m_foliageSprite.transform.localRotation = Quaternion.Euler(0f, 0f, data.foliageRotation);
        }
    }

    public void Clean()
    {
        for( int gripId = 0; gripId<m_gripsList.Count; gripId++ )
        {
            m_gripPool.PoolObject(m_gripsList[gripId]);
        }
        m_gripsList.Clear();
    }

    public float GetEltSize()
    {
        return m_wallSprite[0].bounds.size.x;
    }

}
