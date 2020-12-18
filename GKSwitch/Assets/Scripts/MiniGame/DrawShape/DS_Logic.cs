using System.Collections;using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class DS_Logic : MiniGameLogic
{
    public bool bIsInitialized { get { return m_gameData!=null; } }

    private DS_DrawShape.DrawShapeData m_gameData;
    //private Vector2[] m_vPointArray;

    private List<Vector2[]> m_shapeList = new List<Vector2[]>();
    private int m_nCurrentShape = 0;
    private Rect m_modelPlaneRect;

    public void Init(DS_DrawShape.DrawShapeData gameData, byte nGameData, Rect rect)
    {
        m_nGameDataId = nGameData;
        m_gameData = gameData;
                
        m_modelPlaneRect = rect;
        m_nCurrentShape = 0;
    }

    public void GenerateFirstShape()
    {
        uint nSeed = (uint)Random.Range(1, int.MaxValue);
        GenerateShape( nSeed);
    }


    public Vector2[] GetShape( int nShapeIndex )
    {
        while ( m_shapeList.Count <= nShapeIndex)
        {
            uint nSeed = (uint)Random.Range(1, int.MaxValue);
            GenerateShape( nSeed);
        }

        return m_shapeList[nShapeIndex];
    }

    private void Awake()
    {
    }

    
    private void GenerateShape(uint nSeed, bool bRefresh = true )
    {
        if( !bIsInitialized )
        {
            return;
        }
        RrRndHandler.RndSeed(nSeed);
        int nGridColumnCount = m_gameData.nGridColumnCount;
        int nGridRowCount = m_gameData.nGridRowCount;
        float fColumnSize = m_modelPlaneRect.width / (float)nGridColumnCount;
        float fRowSize = m_modelPlaneRect.height / (float)nGridRowCount;
        float fStartX = m_modelPlaneRect.x - m_modelPlaneRect.width/2f;
        float fStartY = m_modelPlaneRect.y - m_modelPlaneRect.height / 2f;

        Debug.Log("Generate Shape rect : " + fStartX + ", " + fStartY + " // " + m_modelPlaneRect.width);

        lwRndArray rndArray = new lwRndArray((uint)(nGridColumnCount * nGridRowCount));
        
        int nNodeCount = RrRndHandler.RndRange((int)m_gameData.pointsCount.x, (int)m_gameData.pointsCount.y);
        Vector2[] vPointArray = new Vector2[nNodeCount];
        for (int nNodeId = 0; nNodeId < nNodeCount; nNodeId++)
        {
            int nCellId = (int)rndArray.ChooseValue( true );
            float fX = RrRndHandler.RndRange(0f, fColumnSize) + (nCellId % nGridColumnCount) * fColumnSize + fStartX;
            float fY = RrRndHandler.RndRange(0f, fRowSize) + ((int)(nCellId / nGridColumnCount)) * fRowSize + fStartY;
            vPointArray[nNodeId] = new Vector2(fX, fY );
        }

        m_shapeList.Add(vPointArray);
        Debug.Log("GenerateShape end ");
    }
}
